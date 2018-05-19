using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Game;

public class CardRenderer : MonoBehaviour
{
	public GameObject Reverse;
	public GameObject Base;
	public Image CardImage;
	public Text NameText;
	public Text DescText;
	public Text CostText;

	public Card Card { get; private set; }

	public Action<Card> OnClick;

	public void Redraw(Card card)
	{
		Card = card;
		gameObject.name = "Card:" + card.Id;

		if (card.Reversed)
		{
			Reverse.SetActive(true);
			Base.SetActive(false);
		}
		else
		{
			Reverse.SetActive(false);
			Base.SetActive(true);

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

	public void OnCardClick(GameObject target)
	{
		if( OnClick != null)
		{
			OnClick(Card);
		}
	}

}
