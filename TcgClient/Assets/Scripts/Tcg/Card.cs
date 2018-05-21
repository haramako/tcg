using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Master;

namespace Game
{
	public enum CardPlace
	{
		Temp,
		Hands,
		Stack,
		Grave,
		Discarded,
		Opened,
	};

	public partial class Card
	{
		CardTemplate t_;
		public CardTemplate T
		{
			get
			{
				if (t_ == null)
				{
					t_ = G.FindCardTemplateById(CardTemplateId);
				}
				return t_;
			}
		}

		/// <summary>
		/// カードがおいてある場所(Field以外からアクセスしてはいけない)
		/// </summary>
		public CardPlace place_;

		/// <summary>
		/// カードがおいてある場所
		/// </summary>
		public CardPlace Place => place_;


		public bool IsHands => place_ == CardPlace.Hands;
		public bool IsStack => place_ == CardPlace.Stack;
		public bool IsGrave => place_ == CardPlace.Grave;
		public bool IsOpened => place_ == CardPlace.Opened;
		public bool IsTemp => place_ == CardPlace.Temp;

		public bool Reversed;

		public bool ExecuteSpecial(Field f, SpecialParam param)
		{
			foreach (var special in T.Special)
			{
				var ok = special.IsPlayable(f, param);
				if (!ok)
				{
					return false;
				}
			}

			foreach (var special in T.Special)
			{
				var ok = special.Prepare(f, param);
				if( !ok )
				{
					return false;
				}
			}

			foreach ( var special in T.Special)
			{
				special.Execute(f, param);
			}

			return true;
		}

		public string GetDesc()
		{
			var descs = T.Special.Select(s => s.GetDesc().Text);
			return string.Join("/", descs.ToArray());
		}
	}

}
