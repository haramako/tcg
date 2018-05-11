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
	}

}
