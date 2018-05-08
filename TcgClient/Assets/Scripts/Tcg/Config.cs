using System.Collections;
using System;

namespace Game
{
	public class Config
	{
		public static bool CommandLog = false;
		public static bool RequestLog = false;

		public static bool IgnoreKnownError = false; // 既知のエラーを抑え込む（最終ビルド以外はtrueにしないこと。つまり、できるだけエラーを検出して、直せなかったものだけ抑え込むためのもの）

		public static Master.PlatformType Platform = Master.PlatformType.Switch; // 対象のプラットフォーム


		public static bool DFZ1538 = true; // DFZ-1538 グリフォンを倍速にすると３倍速になるの修正が不安なのでフラグで対応(trueなら対応済）


		public static void Setup()
		{
			/*
			foreach (var config in G.ConfigInfos)
			{
				SetValue("Game." + config.Id, config.Value);
			}
			*/
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
				Logger.Error("invalid config key " + key);
				return;
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
				Logger.Error("Configure: property not found, " + key);
				//throw;
			}
		}
	}

	/// <summary>
	/// チート用の設定
	///
	/// この中身は、ユーザー環境ではすべて無効になっているはずのものなので、Configと区別している
	/// </summary>
	public class Snowden
	{
		public static bool MonsterHouse100 = false; // 100%モンスターハウスがでる
		public static int InitialArrow = 0; // 矢の数の初期値
	}
}
