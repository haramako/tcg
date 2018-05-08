using UnityEngine;
using System.Collections.Generic;
using System;
using DebuggerNonUserCodeAttribute = System.Diagnostics.DebuggerNonUserCodeAttribute;

public partial class Configure
{
	static Dictionary<string, string> dict_;

	public static int Hoge;
	public static string Fuga;
	public static bool TestBool;

	static void init()
	{
		if( dict_ != null ) return;
		dict_ = new Dictionary<string, string>();

		LoadFromResource("ConfigProduction");

		#if UNITY_ANDROID
		LoadFromResource("ConfigAndroid");
		#elif UNITY_IPHONE
		LoadFromResource("ConfigIos");
		#elif UNITY_SWITCH
		LoadFromResource("ConfigSwitch");
		#elif UNITY_PS4
		LoadFromResource("ConfigPs4");
		#elif UNITY_WSA
		LoadFromResource("ConfigWsa");
		#elif UNITY_N3DS
		LoadFromResource("ConfigN3ds");
		#endif

		LoadFromResource("ConfigRelease");

		#if UNITY_EDITOR
		try
		{
			LoadFromResource("ConfigDevelopment");
		}
		catch( System.Exception err)
		{
			Debug.LogException (err);
		}
		#endif

		LoadFromFile("Config.txt");
	}

	public static void LoadFromResource(string resourceName)
	{
		var asset = Resources.Load<TextAsset>(resourceName);
		if (asset == null) return;
		Parse(asset.text);
	}

	public static void LoadFromFile(string path)
	{
		#if UNITY_STANDALONE
		if (System.IO.File.Exists(path))
		{
			Parse(System.IO.File.ReadAllText(path));
		}
		#endif
	}

	static char[] Separators = new char[] {'#'};

	public static void Init()
	{
		init();
	}

	public static void Reload()
	{
		dict_ = null;
		init();
	}

	public static void Parse(string text)
	{
		init();
		foreach( string rawLine in text.Split ('\n') )
		{
			string line = rawLine.Split(Separators, 2, System.StringSplitOptions.None)[0]; // #を文字列の中で使いたいため、スペース# を対象にいている
			line = line.Trim ();
			if( line == "" || line[0] == '#' ) continue;
			var keyValue = line.Split (new char[] {'='}, 2);
			if( keyValue.Length != 2 )
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log ( "invalid config '" + line + "'" );
				}
				continue;
			}
			SetValue (keyValue [0].Trim (), keyValue [1].Trim ());
		}
	}

	public static void SetValue(string key, string value)
	{
		// 最後の'.'位置を取得する
		int dotPos = -1;
		for (int i = 0; i < key.Length; i++)
		{
			if (key [i] == '.')
			{
				dotPos = i;
			}
		}

		// クラスとフィールド名を取得する
		Type type;
		string fieldName;
		if (dotPos >= 0)
		{
			var typeName = key.Substring(0, dotPos);
			fieldName = key.Substring(dotPos + 1);
			type = null;
			foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				type = asm.GetType(typeName, false, true);
				if (type != null)
				{
					break;
				}
			}
		}
		else
		{
			type = typeof(Configure);
			fieldName = key;
		}

		// 値を設定する
		try
		{
			var field = type.GetField (fieldName);
			var ft = field.FieldType;
			if (ft == typeof(Int32))
			{
				field.SetValue (null, Int32.Parse (value));
			}
			else if (ft == typeof(Int64))
			{
				field.SetValue (null, Int64.Parse (value));
			}
			else if (ft == typeof(Int16))
			{
				field.SetValue (null, Int16.Parse (value));
			}
			else if (ft == typeof(string))
			{
				field.SetValue (null, value);
			}
			else if (ft == typeof(float))
			{
				field.SetValue (null, float.Parse (value));
			}
			else if (ft == typeof(double))
			{
				field.SetValue (null, double.Parse (value));
			}
			else if (ft == typeof(bool))
			{
				field.SetValue (null, (value.ToLowerInvariant () == "true"));
			}
			else if( ft.IsEnum )
			{
				field.SetValue (null, (Enum.Parse(ft, value, true)));
			}
			else
			{
				throw new Exception ("unkonown field type " + ft);
			}
		}
		catch(Exception)
		{
			Debug.LogError("Configure: property not found, " + key);
			throw;
		}
	}

	public static string GetRaw(string key, string _default = null)
	{
		init();
		string r;
		if (dict_.TryGetValue (key.ToLower (), out r))
		{
			return r;
		}
		else
		{
			return _default;
		}
	}

	/// <summary>
	/// Platformによって違う可能性のある設定を取得する.
	/// GetWithPlatform("Hoge") なら "Hoge.Android" , "Hoge" の順に検索して
	/// 見つからなかった場合は、_defaultで指定された値を返す。
	/// </summary>
	/// <returns>取得した設定値（みつからなかった場合は, _default を返す）</returns>
	/// <param name="key">キー</param>
	/// <param name="_default">見つからなかった場合に使用されるデフォルト値</param>
	public static string Get(string key, string _default = null)
	{
		var platform = GetRaw("platform");
		string r = GetRaw(key + "." + platform );
		if (r != null) return r;
		r = GetRaw (key);
		if (r != null) return r;
		return _default;
	}

	public static bool GetBool(string key, bool _default = false)
	{
		string v = Get(key);
		return (v == null) ? _default : (v == "true");
	}

	public static int GetInt(string key, int _default = 0 )
	{
		string v = Get(key);
		return (v == null) ? _default : int.Parse (v);
	}

	public static float GetFloat(string key, float _default = 0 )
	{
		string v = Get(key);
		return (v == null) ? _default : float.Parse (v);
	}

}

// テスト用のクラス
// Configureと同じアセンブリにいないといけないため、ここにおいておく
public class ConfTest
{
	public static int IntTest;
	public static Int64 Int64Test;
	public static string StringTest;
	public static bool BoolTest;
	public static float FloatTest;
	public static double DoubleTest;
}

