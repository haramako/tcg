using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Game
{

	public enum WaitingType
	{
		None,
		Ack,
	}

	public enum ConnectionState
	{
		Initializing,
		Ready,
		Shutdowned,
	};

	public class ShutdownException : Exception
	{
	}

	public class FieldConnection
	{
		/// <summary>
		/// ログを有効とするかどうか
		/// </summary>
		public static bool LogEnabled = true;

		/// <summary>
		/// ゲームロジック用のスレッド
		/// </summary>
		Thread thread_;

		/// <summary>
		/// Sendが何も行わないモード
		///
		/// これは、ゲームの初期化中など、本来Sendするものが何も行わないほうが都合が良い時に使用される
		/// </summary>
		bool silent_ = false;

		ConcurrentQueue<List<GameLog.ICommand>> sendQueue_ = new ConcurrentQueue<List<GameLog.ICommand>>(1);
		ConcurrentQueue<GameLog.IRequest> recvQueue_ = new ConcurrentQueue<GameLog.IRequest>(1);
		List<GameLog.ICommand> commandList_ = new List<GameLog.ICommand>();

		/// <summary>
		/// リクエストの種類
		///
		/// この種類によって、GameScene側で行う処理が変わる
		/// </summary>
		/// <value>The type of the waiting.</value>
		public WaitingType WaitingType { get; private set; }

		public ConnectionState State { get; private set; }

		/// <summary>
		/// スレッド間通信のタイムアウト[ミリ秒]
		///
		/// -1の場合は、タイムアウトなしになる
		/// </summary>
		public int RequestTimeoutMillis = -1;

		Field field_;

		public Exception ShutdownError;


		public FieldConnection(Field field)
		{
			State = ConnectionState.Initializing;
			field_ = field;
		}

		//===============================================
		// プロセス通信（外部から呼ぶもの）
		//===============================================

		void validateReadyToRequest()
		{
			if (State == ConnectionState.Shutdowned)
			{
				throw new System.InvalidOperationException("field is already shutdowned");
			}
			if (recvQueue_.Count > 0)
			{
				throw new System.InvalidOperationException("field has recv queue item");
			}
		}

		public void StartThread(Action process)
		{
			Logger.Assert(State == ConnectionState.Initializing);

			thread_ = new Thread(() => process());
			thread_.Start();

			sendQueue_.Dequeue(RequestTimeoutMillis); // スレッドが開始するまでまつ
		}

		public void Shutdown()
		{
			if (State != ConnectionState.Initializing && State != ConnectionState.Shutdowned)
			{
				recvQueue_.Enqueue(new GameLog.ShutdownRequest());

				try
				{
					sendQueue_.Dequeue(RequestTimeoutMillis); // 反応があるまで待つ
				}
				catch (Exception/* ex */)
				{
					if (thread_ != null && thread_.ThreadState == ThreadState.Running)
					{
						thread_.Abort();
					}
				}
			}

			if (ShutdownError != null)
			{
				throw ShutdownError;
			}
		}

		/// <summary>
		/// Requestを送り、Commandが返ってくるまで待つ.
		///
		/// 返ってきたときにAck待ちの場合は、再度リクエストを投げる。
		/// また、タイムアウトした場合は、例外を返す。
		/// </summary>
		public List<GameLog.ICommand> Request(GameLog.IRequest request)
		{
			Logger.Assert(request != null);

			validateReadyToRequest();
			recvQueue_.Enqueue(request);

			return sendQueue_.Dequeue(RequestTimeoutMillis);
		}

		//===============================================
		// プロセス通信（内部から呼ぶもの）
		//===============================================

		public void BeginSilent()
		{
			silent_ = true;
		}

		public void EndSilent()
		{
			silent_ = false;
		}

		/// <summary>
		/// メッセージを送信する
		/// </summary>
		/// <param name="command">Command.</param>
		public void Send(GameLog.ICommand command)
		{
			if (silent_)
			{
				return;
			}
			if (command == null)
			{
				throw new ArgumentNullException("Command must not be null");
			}

			#if !UNITY_WSA
			if ( Thread.CurrentThread != thread_ )
			{
				throw new Exception(Marker.D("ゲームスレッド以外からSend()が呼ばれました").Text);
			}
			#endif

			commandList_.Add(command);
			if (Config.CommandLog)
			{
				log("SV->CL:" + field_.FieldInfo.Turn + ":" + command + ": " + inspect(command));
			}
		}

		public void SendAndWait(GameLog.ICommand command)
		{
			if (silent_)
			{
				return;
			}
			Send(command);
			WaitForAck();
		}

		public GameLog.IRequest WaitForRequest(WaitingType waitingType)
		{
			WaitingType = waitingType;

			sendQueue_.Enqueue(commandList_);
			commandList_ = new List<GameLog.ICommand>();
			if (Config.CommandLog)
			{
				log("SV->CL:" + field_.FieldInfo.Turn + ":(Waiting) " + waitingType);
			}

			var req = recvQueue_.Dequeue();
			if (Config.RequestLog)
			{
				log("CL<-SV:" + field_.FieldInfo.Turn + ":" + req.GetType() + ": " + inspect(req));
			}
			if (req is GameLog.ShutdownRequest)
			{
				throw new ShutdownException();
			}
			waitingType = WaitingType.None;
			return req;
		}

		public T WaitForRequest<T>(WaitingType waitingType) where T : GameLog.IRequest
		{
			var res = WaitForRequest(waitingType);
			if (res.GetType() != typeof(T))
			{
				throw new InvalidOperationException("Require " + typeof(T) + " but " + res.GetType());
			}
			return (T)res;
		}

		public void WaitForAck()
		{
			WaitForRequest<GameLog.AckResponseRequest>(WaitingType.Ack);
		}

		//===============================================
		//
		//===============================================

		[System.Diagnostics.DebuggerNonUserCode]
		public void log(string message)
		{
			if (LogEnabled)
			{
				Logger.Info(message);
			}
		}

		string inspect(object obj)
		{
			#if UNITY_5_5_OR_NEWER
			return UnityEngine.JsonUtility.ToJson (obj);
			#else
			return null;
			#endif
		}
	}
}
