using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;
using RSG;
using UnityEngine.Profiling;

/**
 * オブジェクトキャッシュ.
 *
 * アセットバンドル、もしくは、リソースから読み込んでGameObjectを生成する。
 *
 *
 * 使い方:
 * // オブジェクトのインスタンス化を行う
 * ResourceCache.Create<GameObject>("ResourceName"),
 *   .Sibscribe(obj=>{
 *      (...objを使った処理)
 *   });
 *
 * // プリフェッチ（事前にロードのみを行う）
 * ResourceCache.Prefetch<GameObject>("ResourceName");
 *
 */
public partial class ResourceCache: MonoSingleton<ResourceCache>
{
	// TODO: 依存関係の解決
	// TODO: 並列数の指定
	// TODO: プラットフォームの文字列取得の整理
	// TODO: ファイルの容量の概要を取得
	// TODO: LoadAllAssetsを使うか, LoadAssetを使うかの指定

	/// <summary>
	/// 詳細のログを表示するかどうか
	/// </summary>
	static public bool LogEnabled = false;

	/// <summary>
	/// アセバンを使わずにエディタ内のアセットを利用するオプション（エディタでのみ使用可能、それ以外では無視される）
	/// </summary>
	static public bool EnableLoadDirect = false;

	/// <summary>
	/// 使用していないリソースを解放するまでの時間[sec]
	/// </summary>
	static public float ReleaseDelay = 120.0f;

	/// <summary>
	/// アセットバンドルの拡張子
	/// </summary>
	static public string Extension = ".ab";

	/// <summary>
	/// 遅延のエミュレーション
	/// </summary>
	static public Func<string, float> DelayEmurationFunc;

	/// <summary>
	/// タスクキュー
	/// </summary>
	Queue<IEnumerator> tasksQueue_ = new Queue<IEnumerator>();

	/// <summary>
	/// ログを出力する(LogEnabled==trueのときのみ)
	/// </summary>
	[System.Diagnostics.Conditional("DEBUG")]
	public static void Log(object obj)
	{
		if (LogEnabled)
		{
			Debug.Log("ResourceCache: " + obj);
		}
	}

	public Dictionary<string, Resource> LoadedResources = new Dictionary<string, Resource>();

	public Dictionary<string, string[]> Dependencies = new Dictionary<string, string[]>();

	public static void Init()
	{
		PreloadResourceHolder.Init();
		LoadDependencies();
	}

	public static void LoadDependencies()
	{
		// TODO: 現時点では、仮にプレーンテキストから読んでいる
		#if false
		var lines = File.ReadAllLines(Path.Combine(Path.Combine(Application.temporaryCachePath, GameSystem.RuntimePlatform()), "dependency.txt"));
		foreach( var line in lines)
		{
			if (string.IsNullOrEmpty(line)) continue;
			var fileAndDeps = line.Split('=');
			var Deps = fileAndDeps[1].Split(',');
			Dependencies[fileAndDeps[0]] = Deps;
		}
		#endif
	}

	public static string PlatformDir()
	{
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				return "StandaloneWindows64";
			//return "StandaloneWindows64";
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXEditor:
				return "StandaloneOSXIntel64";
			case RuntimePlatform.Android:
				return "Android";
			case RuntimePlatform.IPhonePlayer:
				return "iOS";
			case RuntimePlatform.WSAPlayerX64:
			case RuntimePlatform.WSAPlayerX86:
			case RuntimePlatform.WSAPlayerARM:
				return "WSAPlayer";
			case RuntimePlatform.PS4:
				return "PS4";
			case RuntimePlatform.Switch:
				return "Switch";
			default:
				throw new Exception("unknown platform");
		}
	}

	public IPromise<Object> LoadOrCreateObject(string name, Type type, bool isCreate, GameObject refCountOwner = null, bool nullException = false)
	{
		var pair = SplitResourceName(name);
		return LoadResource(pair[0])
			   .OnLoaded
			   .Then((res) =>
		{
			var obj = res.LoadAsset(pair[1], type);
			if( obj == null)
			{
				if (nullException)
				{
					return null;
				}
				Debug.LogError("ResourceCache: cannot find object '" + pair[1] + "' in '" + pair[0] + "'");
				throw new Exception("cannot find object " + pair[1] + " in " + pair[0]);
			}
			if (isCreate && type == typeof(GameObject))
			{
				obj = Object.Instantiate(obj);
				var refcount = ((GameObject)obj).AddComponent<ResourceRefcountBehaviour>();
				refcount.SetTargetResource(res);
			}
			if (refCountOwner != null)
			{
				var refcount = refCountOwner.AddComponent<ResourceRefcountBehaviour>();
				refcount.SetTargetResource(res);
			}
			return obj;
		});
	}

	public static string[] SplitResourceName(string name)
	{
		var pair = name.Split ('/');
		var result = new string[2];
		if (pair.Length > 1)
		{
			result[0] = pair[0];
			result[1] = pair[pair.Length - 1];
		}
		else
		{
			result[0] = pair[0];
			result[1] = pair[0];
		}
		return result;
	}

	public Resource LoadResource(string filename)
	{
		filename = filename.ToLowerInvariant();

		// キャッシュor読み込み中の中にあるならそれを返す
		Resource res;
		if (LoadedResources.TryGetValue (filename, out res))
		{
			res.LastLoadTime = Time.time;
			return res;
		}

		if ( res == null )
		{
			PreloadResource.TryCreate(filename, out res);
		}

		#if UNITY_EDITOR
		if ( res == null && EnableLoadDirect)
		{
			DirectResource.TryCreate(filename, out res);
		}
		#endif

		if (res == null )
		{
			Profiler.BeginSample("AssetBundleResource.TryCreate");
			AssetBundleResource.TryCreate(filename, out res);
			Profiler.EndSample();
		}

		if ( res == null )
		{
			throw new ArgumentException ("Cannot find resource " + filename);
		}

		LoadedResources [filename] = res;
		res.LastLoadTime = Time.time;

		// 読み込み遅延のエミュレーション
		float delay = 0;
		if (!res.IsLoaded && DelayEmurationFunc != null)
		{
			delay = DelayEmurationFunc(filename);
		}

		if (delay > 0)
		{
			Log("Delay emulation " + filename + " " + delay);
			res.OnLoaded = res.OnLoaded.Delay(delay);
		}

		ResourceCache.Log("Loading resource '" + filename + "' by " + res.GetType() + ((delay > 0) ? (" with delay " + delay) : ""));

		return res;
	}

	public void AddTask(IEnumerator op)
	{
		tasksQueue_.Enqueue(op);
	}

	public IEnumerator Start()
	{
		while (true)
		{
			if (tasksQueue_.Count == 0)
			{
				yield return null;
				continue;
			}

			var op = tasksQueue_.Dequeue();
			yield return op;
		}
	}

	public void DoReleaseForce(string filename)
	{
		return;
		#if false // CS0162
		ResourceCache.Log ( "RleaseForce resource '" + filename);
		Resource res;
		if (Bundles.TryGetValue (filename, out res))
		{
			res.Dispose ();
			Bundles.Remove (filename);
		}
		#endif
	}

	public void DoRelease(string filename)
	{
		ResourceCache.Log ( "Rlease resource '" + filename);
		Resource res;
		if (LoadedResources.TryGetValue (filename, out res))
		{
			if (res.RefCount <= 0)
			{
				res.Dispose ();
				LoadedResources.Remove (filename);
			}
		}
	}

	public void DoReleaseAll(float forceDelay = -1f)
	{
		ResourceCache.Log ( "Rlease all");
		var delay = ReleaseDelay;
		if (forceDelay >= 0)
		{
			delay = forceDelay;
		}
		LoadedResources = LoadedResources.Where (kv =>
		{
			if (kv.Value.RefCount <= 0 && Time.time >= kv.Value.LastLoadTime + delay)
			{
				ResourceCache.Log( "Release " + kv.Value.Name);
				kv.Value.Dispose ();
				return false;
			}
			else
			{
				return true;
			}
		}).ToDictionary(kv => kv.Key, kv => kv.Value);
	}

	public void DoReleaseAllForce()
	{
		ResourceCache.Log ( "Rlease all force");
		foreach (var b in LoadedResources.Values)
		{
			b.Dispose ();
		}
		LoadedResources.Clear ();
	}

	public void DoShowStatus()
	{
		ResourceCache.Log ("ShowStatus: " + LoadedResources.Count);
		foreach (var b in LoadedResources.Values)
		{
			ResourceCache.Log ("Name: " + b.Name + " RefCount=" + b.RefCount);
		}
	}

	public static Resource GetBundle(string name)
	{
		return Instance.LoadResource(name);
	}

	public static bool IsExistBundle(string name)
	{
		return Instance.IsResource(name.ToLower());
	}

	public bool IsResource(string name)
	{
		Resource res;

		if (LoadedResources.TryGetValue (name, out res))
		{
			return res.IsLoaded;
		}

		return false;
	}

	public static IPromise<Resource> LoadBundle(string name)
	{
		return Instance.LoadResource (name).OnLoaded;
	}

	public static IPromise<T> LoadAndRefCount<T>(string name, GameObject refCountOwner) where T : Object
	{
		return Instance.LoadOrCreateObject (name, typeof(T), false, refCountOwner).Then( obj => (T)obj);
	}

	public static IPromise<T> LoadAndRefCountAsComponent<T>(string name, GameObject refCountOwner) where T : Component
	{
		return Instance.LoadOrCreateObject (name, typeof(GameObject), false, refCountOwner).Then( obj => ((GameObject)obj).GetComponent<T>());
	}

	public static IPromise<T> Load<T>(string name ) where T: Object
	{
		return Instance.LoadOrCreateObject (name, typeof(T), false).Then( obj => (T)obj);
	}

	public static IPromise<T> LoadOrNull<T>(string name) where T : Object
	{
		return Instance.LoadOrCreateObject(name, typeof(T), false, null, true).Then(obj => { return (obj != null ? (T)obj : null); });
	}

	public static T LoadSync<T>(string assetBundleName, string name) where T : Object
	{
		return (T)Instance.LoadResource(assetBundleName).LoadAsset(name, typeof(T));
	}

	public static IPromise<T> Create<T>(string name) where T: Object
	{
		return Instance.LoadOrCreateObject (name, typeof(T), true).Then (o => (T)o);
	}

	public static IPromise<T> CreateAsComponent<T>(string name) where T: Component
	{
		return Create<GameObject> (name).Then (obj => obj.GetComponent<T> ());
	}

	public static void ReleaseForce(string filename)
	{
		Instance.DoReleaseForce (filename);
	}

	public static void ReleaseAllForce()
	{
		Instance.DoReleaseAllForce ();
	}

	/// <summary>
	/// リファレンスカウントが0ならリリースする
	/// </summary>
	/// <param name="filename">リソース名</param>
	public static void Release(string filename)
	{
		Instance.DoRelease (filename);
	}

	/// <summary>
	/// リファレンスカウントがないものをすべて開放する
	/// </summary>
	public static void ReleaseAll(float forceDelay = -1f)
	{
		Instance.DoReleaseAll (forceDelay);
	}

	public static void ShowStatus()
	{
		Instance.DoShowStatus();
	}

}
