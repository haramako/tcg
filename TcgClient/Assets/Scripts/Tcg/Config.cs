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

		public static int DefaultMaxHp = 20;
		public static int DefaultMana = 4;
		public static int DefaultDrawCount = 5;
		public static int DefaultDeckCount = 20;
	}
}
