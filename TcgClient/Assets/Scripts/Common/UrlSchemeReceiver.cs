using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class UrlSchemeReceiver
{
	static bool consumed;
	static string savedScheme;

	static UrlSchemeReceiver()
	{
		#if UNITY_EDITOR
		savedScheme = PlayerPrefs.GetString("StartupWindow.Url", "");
		if (savedScheme == "") savedScheme = null; // ""はnull扱い
		#elif UNITY_ANDROID
		savedScheme = PlayerPrefs.GetString("BootScheme", "");
		if (savedScheme == "") savedScheme = null; // ""はnull扱い
		PlayerPrefs.DeleteKey("BootScheme");
		#elif UNITY_IOS
		// TODO: これから
		#elif UNITY_SWITCH
		// DO NOTHING
		#else
		// コマンドライン引数を処理する
		var args = System.Environment.GetCommandLineArgs();
		for( int i = 1; i < args.Length; i++)
		{
			switch( args[i])
			{
				case "--url":
					savedScheme = args[i + 1];
					i++;
					break;
			}
		}
		#endif
	}

	public static bool HasUrlScheme()
	{
		return !consumed && !string.IsNullOrEmpty(savedScheme);
	}

	/// <summary>
	/// URLスキーマを取得する。
	/// </summary>
	/// <returns>URLスキーマの文字列。ない場合はnull</returns>
	public static string PeekUrlScheme()
	{
		if( consumed || string.IsNullOrEmpty(savedScheme))
		{
			return null;
		}
		else
		{
			return savedScheme;
		}
	}

	/// <summary>
	/// URLスキーマを取得し、同時にクリアする。
	/// </summary>
	/// <returns>>URLスキーマの文字列。ない場合はnull</returns>
	public static string GetUrlScheme()
	{
		if (consumed)
		{
			return null;
		}
		else
		{
			consumed = true;
			return savedScheme;
		}
	}

	/// <summary>
	/// URLスキーマを指定する
	/// デバッグ時のみ使用する
	/// </summary>
	/// <param name="val">URLスキーマの値</param>
	public static void SetUrlScheme(string val)
	{
		consumed = false;
		savedScheme = val;
	}

	#if UNITY_EDITOR
	public static void SetUrlInEditor(string url)
	{
		PlayerPrefs.SetString("StartupWindow.Url", url);
	}
	#endif

}
