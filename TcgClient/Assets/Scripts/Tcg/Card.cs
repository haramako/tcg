using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Master;

namespace Game
{

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

	}

}
