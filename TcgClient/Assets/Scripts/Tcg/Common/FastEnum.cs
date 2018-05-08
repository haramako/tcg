using System.Collections.Generic;
using System;

namespace Game
{

	/// <summary>
	/// 型ごとのキャッシュ機構付きの高速なEnum
	/// </summary>
	public static class FastEnum<T>
	{
		static Dictionary<string, T> parseCache_ = new Dictionary<string, T>();
		static Dictionary<T, string> nameCache_ = new Dictionary<T, string>();

		/// <summary>
		/// stringから変換する
		///
		/// 大文字小文字の区別はしない
		/// </summary>
		public static T Parse(string name)
		{
			T result;
			name = name.ToLowerInvariant();
			if (parseCache_.TryGetValue(name, out result))
			{
				return result;
			}
			else
			{
				result = (T)Enum.Parse(typeof(T), name, true);
				parseCache_[name] = result;
				return result;
			}
		}

		/// <summary>
		/// stringに変換する
		/// </summary>
		public static string ToString(T val)
		{
			string result;
			if (nameCache_.TryGetValue(val, out result))
			{
				return result;
			}
			else
			{
				result = val.ToString();
				nameCache_[val] = result;
				return result;
			}
		}
	}
}
