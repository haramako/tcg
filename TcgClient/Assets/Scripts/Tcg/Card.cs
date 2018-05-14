using System.Collections;
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
	}

}
