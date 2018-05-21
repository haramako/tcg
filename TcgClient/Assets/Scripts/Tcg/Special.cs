using System;
using System.Collections.Generic;
using Master;

namespace Game
{
	public class SpecialParam
	{
		public Card Card;
		public Character Target;
		public Character Executer;

		// Prepareで更新するもの
		public List<Card> PreparedSelectedCard;

		// 結果として返すもの
		public bool ResultDiscard;
	}

	/// <summary>
	/// カードの特殊能力を処理するクラス
	/// </summary>
	public abstract class Special
	{
		public Master.SpecialTemplate T;

		static Dictionary<SpecialType, Type> specialsByType_ = new Dictionary<SpecialType, Type>();

		static Special()
		{
			foreach (var name in Enum.GetNames(typeof(SpecialType)))
			{
				var type = Type.GetType("Game.Specials." + name, false, true);
				if (type != null)
				{
					var val = (SpecialType)Enum.Parse(typeof(SpecialType), name);
					specialsByType_[val] = type;
				}
				else
				{
					// UnityEngine.Debug.LogError("Game.Thinkings." + name + " not found");
				}
			}
		}

		public virtual bool IsPlayable(Field f, SpecialParam p)
		{
			return true;
		}

		public virtual bool Prepare(Field f, SpecialParam p)
		{
			return true;
		}

		public virtual void Execute(Field f, SpecialParam p)
		{
		}

		public virtual TextMarker GetDesc()
		{
			return Marker.T("GetDesc()が未実装");
		}

		static public Special Create(Master.SpecialTemplate t)
		{
			Type type;
			if (specialsByType_.TryGetValue(t.Type, out type))
			{
				var instance = (Special)Activator.CreateInstance(type);
				instance.T = t;
				return instance;
			}
			else
			{
				throw new Exception(Marker.D("Special '{0}' が存在しません。").Format(t.Type).Text);
			}
		}

		public int GetAmount(Field f, SpecialParam p)
		{
			if (T.Amount != 0)
			{
				return T.Amount;
			}

			var c = T.Counter;
			switch (c.Type)
			{
				case SpecialTemplate.Types.CounterType.InHand:
					return f.Hands.Count * c.Multiply;
				default:
					throw new Exception("Unknown counter type " + c.Type);
			}
		}

		public TextMarker GetAmountDesc()
		{
			if( T.Amount != 0)
			{
				return Marker.T("{0}").Format(T.Amount);
			}

			var c = T.Counter;
			switch (c.Type)
			{
				case SpecialTemplate.Types.CounterType.InHand:
					return Marker.T("{0}の枚数x{1}").Format("手札", c.Multiply);
				default:
					throw new Exception("Unknown counter type " + c.Type);
			}
		}

	}
}
