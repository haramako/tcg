using System.Collections;
using System.Linq;

namespace Game
{
	public struct TextMarker
	{
		public string Text
		{
			get
			{
				var str = G.ResolveMessage("T", str_, variant_);
				if (params_ == null)
				{
					return str;
				}
				else
				{
					try
					{
						return string.Format(str, params_);
					}
					catch(System.FormatException ex)
					{
						Logger.Error(ex.ToString());
						return str;
					}
				}
			}
		}

		public override string ToString() => Text;

		public string OriginalText
		{
			get
			{
				return str_;
			}
		}

		public int Hash
		{
			get
			{
				return hash_;
			}
		}

		public string Variant
		{
			get
			{
				return variant_;
			}
		}

		public object[] Params
		{
			get
			{
				return params_;
			}
		}

		int hash_;
		string str_;
#pragma warning disable 414
		string variant_;
#pragma warning restore 414
		object[] params_;

		public TextMarker(int hash, string str)
		{
			str_ = str;
			hash_ = hash;
			variant_ = "";
			params_ = null;
		}

		public TextMarker(int hash, string str, string variant)
		{
			str_ = str;
			hash_ = hash;
			variant_ = variant;
			params_ = null;
		}

		public TextMarker Format(params object[] param)
		{
			if (params_ != null)
			{
				params_ = params_.Concat(param).ToArray();
			}
			else
			{
				params_ = param;
			}
			return this;
		}
	}

	public struct RandomMarker
	{
		TextMarker textMarker_;
		// MEMO: デフォルトの {get; private set;} では、コンパイラのバージョンによっては通らないので、手動で定義している
		public TextMarker TextMarker { get { return textMarker_; } }
		public RandomMarker(int hash, string str)
		{
			textMarker_ = new TextMarker(hash, str);
		}
	}

	public class Marker
	{

		// TODO: そのうちちゃんと最終ビルドでは最適化されるものにする

		/// <summary>
		/// テキストをマークする
		///
		///
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static TextMarker T(string msgText)
		{
			return new TextMarker(0, msgText);
		}

		public static TextMarker T(string msgText, string msgText2)
		{
			return new TextMarker(0, msgText, msgText2);
		}

		/// <summary>
		/// デバッグ用のテキストを記載する
		///
		/// これらのテキストはユーザーの目には触れないので、翻訳は必須ではないが、Tと全く同じ処理が行われる
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static TextMarker D(string msgText)
		{
			return new TextMarker(0, msgText);
		}

		/// <summary>
		/// サウンド名を指定する
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static TextMarker Sound(string msgText)
		{
			return new TextMarker(0, msgText);
		}

		/// <summary>
		/// ボイス名を指定する
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static TextMarker Voice(string msgText)
		{
			return new TextMarker(0, msgText);
		}

		/// <summary>
		/// アニメーションクリップ名を指定する
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static TextMarker Animation(string msgText)
		{
			return new TextMarker(0, msgText);
		}

		/// <summary>
		/// ランダム記録用のテキストを記載する
		///
		/// これらのテキストはユーザーの目には触れないので、翻訳は必須ではないが、Tと全く同じ処理が行われる
		/// </summary>
		/// <param name="msgText">Message text.</param>
		public static RandomMarker Rand(string msgText)
		{
			return new RandomMarker(0, msgText);
		}

		// TODO: そのうち作る
		public static TextMarker RawText(string rawText)
		{
			return new TextMarker(-1, rawText);
		}

		public static int DungeonId(int id)
		{
			return id;
		}

		public static int ItemTemplateId(int id)
		{
			return id;
		}

		public static int SkillId(int id)
		{
			return id;
		}

		public static int CharacterTemplateId(int id)
		{
			return id;
		}

		public static int ThinkingTypeId(int id)
		{
			return id;
		}

		public static string EffectSymbol(string symbol)
		{
			return symbol;
		}

		public static int TutorialDialogId(int id)
		{
			return id;
		}

	}
}