using System;
using System.Collections.Generic;
using System.Linq;
using Master;
using Google.ProtocolBuffers;

namespace Game
{
	public partial class G
	{
		public static IFileSystem FileSystem;

		public static void Initialize(IFileSystem fs)
		{
			FileSystem = fs;
		}

		public static List<T> LoadPbFiles<T>(Func<T> constructor, string type, Func<string, bool> filter = null) where T : Message
		{
			List<T> list = new List<T>();
			byte[] buf = new byte[1024 * 1024];
			var postfix = "_" + type + ".pb";
			foreach( var file in FileSystem.GetFiles().Where(f => f.EndsWith(postfix) ))
			{
				if( filter != null && !filter(file))
				{
					continue;
				}

				int size;
				using (var s = FileSystem.OpenRead (file))
				{
					size = s.Read (buf, 0, buf.Length);
				}
				list.AddRange( PbFile.ReadPbList(constructor, buf, 0, size).ToList() );
			}
			if (list.Count <= 0)
			{
				// TODO: 一時的に無視
				//throw new Exception ("no items found for " + type);
				Logger.Error("no items found for " + type);
			}
			// Debug.Log ("loaded " + type + " "  + list.Count + " items");
			return list;
		}

		public static void loadOther()
		{
			LoadI18n("Ja");
		}

		public static void unloadOther()
		{
			UnloadI18n();
		}

		public static string ResolveMessage(string tag, string messageId, string variant, bool nullIfNotFound = false)
		{
			var messages = FindI18nMessageById(messageId, false);
			string noVariant = null;
			if( messages != null )
			{
				foreach (var mes in messages)
				{
					if (mes.Tag == tag)
					{
						if (mes.Variant == variant)
						{
							return TextUtility.GaijiConvert(mes.Text);
						}
						else if (string.IsNullOrEmpty(mes.Variant))
						{
							noVariant = mes.Text;
						}
					}
				}
			}

			if( noVariant != null)
			{
				//Logger.Info("Message:" + messageId);
				return TextUtility.GaijiConvert(noVariant);
			}
			else if( nullIfNotFound )
			{
				return null;
			}
			else
			{
				return messageId;
			}
		}

		public static string ResolveMessage(string tag, int messageId, string fallback)
		{
			var text = ResolveMessage(tag, "" + messageId, "", true);
			if (text == null)
			{
				return TextUtility.GaijiConvert(fallback);
			}
			else
			{
				return text;
			}
		}

		// ===========================================
		// I18nMessageの読み込み等

		/// <summary>
		/// 言語を指定してI18nメッセージを読み込む
		/// </summary>
		/// <param name="lang">"Ja"/"En"などの言語コード</param>
		public static void LoadI18n(string lang)
		{
			var langPart = "_" + lang + "_";

			I18nMessages_ = LoadPbFiles<I18nMessage>(I18nMessage.CreateInstance, "I18nMessage", f =>
			{
				// "Ps4"が含まれるファイルは PS4 プラットフォームでだけ読み込まれる
				var matchPs4 = !f.Contains("Ps4") || (Config.Platform == PlatformType.Ps4);
				return matchPs4 && f.Contains(langPart);
			});
			readOnlyI18nMessages_ = I18nMessages_.AsReadOnly();

			MessageFusions_ = LoadPbFiles<MessageFusion>(MessageFusion.CreateInstance, "MessageFusion", f => f.Contains(langPart));
			readOnlyMessageFusions_ = MessageFusions_.AsReadOnly();

			{
				I18nMessageById_ = new Dictionary<string, List<I18nMessage>>();
				int len = I18nMessages.Count;
				for (int i = 0; i < len; i++)
				{
					var obj = I18nMessages[i];
					if (obj.Id == default(string)) { continue; }
					List<I18nMessage> list;
					if (!I18nMessageById_.TryGetValue(obj.Id, out list))
					{
						list = new List<I18nMessage>();
						I18nMessageById_.Add(obj.Id, list);
					}
					list.Add(obj);
				}
			}
		}

		public static void UnloadI18n()
		{
			I18nMessages_ = null;
			readOnlyI18nMessages_ = null;

			MessageFusions_ = null;
			readOnlyMessageFusions_ = null;
		}

		public static StatusInfo FindStatusInfo(CharacterStatus stat) => FindStatusInfoById((int)stat);

		public static TextMarker DisplayName(CardLocation v)
		{
			return new TextMarker(0, "CardLocation." + v);
		}
	}
}