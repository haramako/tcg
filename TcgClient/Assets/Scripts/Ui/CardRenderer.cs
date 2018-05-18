using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;

public class CardRenderer : MonoBehaviour
{
	public Image CardImage;
	public Text NameText;
	public Text DescText;
	public Text CostText;

	public Card Card { get; private set; }

	public void Redraw(Card card)
	{
		Card = card;
		gameObject.name = "Card:" + card.Id;

		if (card.Reversed)
		{
			NameText.text = "";
			DescText.text = "";
			CostText.text = "";
			CardImage.sprite = null;
		}
		else
		{
			NameText.text = card.T.Name;
			DescText.text = card.GetDesc();
			CostText.text = "" + card.T.Cost;
			var spriteName = string.Format("{0:D4}", card.T.ImageId);
			ResourceCache.Load<Sprite>(spriteName).Done(spr =>
			{
				CardImage.sprite = spr;
			});
		}
	}

}
