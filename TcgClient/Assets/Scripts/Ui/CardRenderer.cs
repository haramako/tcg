﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;

public class CardRenderer : MonoBehaviour
{
	public Image CardImage;
	public Text NameText;
	public Text DescText;

	public void Redraw(Card card)
	{
		NameText.text = card.T.Name;
		DescText.text = card.T.Desc;
		var spriteName = string.Format("{0:D4}", card.T.ImageId);
		ResourceCache.Load<Sprite>(spriteName).Done(spr =>
		{
			CardImage.sprite = spr;
		});
	}

}