package main

import (
	"bufio"
	"fmt"
	"io"
	"io/ioutil"
	"net/http"
	"os"
	"os/exec"
	"runtime"
	"strconv"
	"strings"
	"sync"

	"github.com/cratonica/trayhost"
	"github.com/gin-gonic/gin"
	"github.com/gorilla/websocket"
)

var wsupgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
}

func readLines(readers ...io.Reader) <-chan string {
	c := make(chan string)
	wg := &sync.WaitGroup{}

	for _, reader := range readers {
		wg.Add(1)
		go func(rd io.Reader) {
			r := bufio.NewReader(rd)
			for {
				line, err := r.ReadBytes('\n')
				if err != nil {
					break
				}
				c <- strings.TrimRight(string(line), "\n")
			}
			wg.Done()
		}(reader)
	}

	go func() {
		wg.Wait()
		close(c)
	}()
	return c
}

func wshandler(w http.ResponseWriter, r *http.Request) {

	conn, err := wsupgrader.Upgrade(w, r, nil)
	if err != nil {
		fmt.Println("Failed to set websocket upgrade: %+v", err)
		return
	}
	defer conn.Close()

	t, msg, err := conn.ReadMessage()
	if err != nil {
		return
	}

	defer func() {
		if err != nil {
			println(err.Error())
			conn.WriteMessage(t, []byte(err.Error()))
		}
	}()

	commands := strings.Split(string(msg), " ")

	env := make([]string, 0)
	for strings.Contains(commands[0], "=") {
		env = append(env, commands[0], "=")
		commands = commands[1:]
	}
	fmt.Println(commands)
	cmd := exec.Command(commands[0], commands[1:]...)
	cmd.Env = append(os.Environ(), env...)
	cmd.Dir = rootFilepath

	stdout, err := cmd.StdoutPipe()
	if err != nil {
		fmt.Println(err)
		return
	}

	stderr, err := cmd.StderrPipe()
	if err != nil {
		fmt.Println(err)
		return
	}

	out_chan := readLines(stdout, stderr)

	err = cmd.Start()
	if err != nil {
		fmt.Println(err)
		return
	}

	for line := range out_chan {
		conn.WriteMessage(t, []byte(line))
	}

	err = cmd.Wait()
	if err != nil {
		fmt.Println(err)
		conn.WriteMessage(t, []byte("ERROR: "+err.Error()))
		return
	}

	println("finish")
}

func hostList(c *gin.Context) {
	response, err := http.Get("http://133.242.235.150:7000/host_list.txt")
	if err != nil {
		c.AbortWithError(500, err)
	}
	defer response.Body.Close()

	body, err := ioutil.ReadAll(response.Body)
	if err != nil {
		c.AbortWithError(500, err)
	}

	c.String(200, string(body))
}

func startServer(port int) {
	r := gin.Default()
	r.GET("/host_list", hostList)
	r.GET("/cmd", func(c *gin.Context) {
		wshandler(c.Writer, c.Request)
	})

	r.StaticFile("/", "./webui_assets/index.html")
	r.StaticFS("/assets", http.Dir("./webui_assets"))

	err := r.Run(":" + strconv.Itoa(port))
	if err != nil {
		os.Exit(1)
	}
}

var isDebug bool
var rootFilepath string

func main() {
	portStr := os.Getenv("PORT")
	port, _ := strconv.Atoi(portStr)
	if port == 0 {
		port = 3020
	}

	rootFilepath = os.Getenv("ROOT_PATH")

	isDebug = (len(os.Getenv("DEBUG")) != 0)
	if isDebug {
		startServer(port)
	} else {

		// EnterLoop must be called on the OS's main thread
		runtime.LockOSThread()

		go func() {
			// Run your application/server code in here. Most likely you will
			// want to start an HTTP server that the user can hit with a browser
			// by clicking the tray icon.
			startServer(port)

		}()

		println("I'm in Task Tray! Click Me.")
		println("タスクトレイに常駐しました。タスクトレイの輪入道をクリックしてね。")

		// Be sure to call this to link the tray icon to the target url
		trayhost.SetUrl("http://localhost:" + strconv.Itoa(port) + "/assets/#")

		// Enter the host system's event loop
		trayhost.EnterLoop("DragonFang", iconData)

		// This is only reached once the user chooses the Exit menu item
		fmt.Println("Exiting")
	}
}
