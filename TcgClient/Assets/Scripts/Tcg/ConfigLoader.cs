using System.Collections;
using System;

namespace Game
{
	public class ConfigLoader
	{
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

}
