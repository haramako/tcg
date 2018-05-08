using System.Text;

namespace Game
{
	public class TextUtility
	{
		/// <summary>
		/// キーボードかどうか
		/// </summary>
		public static bool IsKeyboard;

		/// <summary>
		/// PS4でO/Xボタンを交換しているかどうか
		/// </summary>
		public static bool ExchangeEnterButton;

		static System.Text.StringBuilder sb = new System.Text.StringBuilder();

		/// <summary>
		/// 外字コードをプラットフォームの違いに合わせて変更する
		/// </summary>
		static public string GaijiConvert(string fromText)
		{
			sb.Length = 0;
			foreach (char c in fromText)
			{
				var charCode16 = (int)c;
				if (0xe0e0 <= charCode16 && charCode16 <= 0xe11f)
				{
					switch (Config.Platform)
					{
						case Master.PlatformType.Wsa:
						case Master.PlatformType.Steam:
							charCode16 += (IsKeyboard) ? 0x100 : 0x40;
							break;
						case Master.PlatformType.Ps4:
							if (ExchangeEnterButton)
							{
								// ○☓反転
								if (charCode16 == 0xe0e0)
								{
									charCode16 = 0xe0e1;
								}
								else if (charCode16 == 0xe0e1)
								{
									charCode16 = 0xe0e0;
								}
							}
							charCode16 += 0x80;
							break;
						case Master.PlatformType.Switch:
							// DO NOTHING
							break;
					}
				}
				sb.Append((char)charCode16);
			}
			return sb.ToString();
		}
	}
}