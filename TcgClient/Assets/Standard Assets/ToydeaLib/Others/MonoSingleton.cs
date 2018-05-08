/// <summary>
/// Generic Mono singleton.
/// </summary>
using UnityEngine;
using System;

/// <summary>
/// シングルトン
/// Sceneのヒエラルキに最初から生成してあるのが前提のシングルトン。
/// ヒエラルキ内にない場合は、instanceはnullを返す。
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{

	/// <summary>
	/// インスタンスを返す。
	/// </summary>
	public static T Instance { get; private set;}

	public static bool HasInstance { get { return Instance != null; } }

	protected virtual void Awake()
	{
		if (Instance != null && Instance != this )
		{
			DestroyImmediate (gameObject);
			return;
		}
		Instance = (T)this;
		if( transform.parent == null)
		{
			DontDestroyOnLoad (gameObject);
		}
	}

	protected virtual void OnDestroy()
	{
		if( Instance == this )
		{
			Instance = null;
		}
	}
}

/// <summary>
/// シングルトン
/// Sceneのヒエラルキに最初から生成してあるのが前提のシングルトン。
/// ヒエラルキ内にない場合は、instanceはnullを返す。
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : Singleton<T>, new()
{

	/// <summary>
	/// インスタンスを返す。
	/// </summary>
	public static T Instance
	{
		get
		{
			if (instance_ != null )
			{
				return instance_;
			}
			else
			{
				return getInstance ();
			}
		}
	}

	static T instance_;

	public static bool HasInstance { get { return instance_ != null; } }

	protected virtual void Initialize()
	{
	}

	static T getInstance()
	{
		if (instance_ != null)
		{
			return instance_;
		}

		instance_ = new T ();
		instance_.Initialize ();
		return instance_;
	}

}
