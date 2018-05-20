using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RSG;
using DG.Tweening;
using System;
using UnityEngine;

using Game;
using Master;
using GameLog;
using UnityEngine.Profiling;

public partial class CommandProcessor
{
	public static IPromise ProcessCardPlayied(GameScene scene, CardPlayed com)
	{
		return Promise.Resolved();
	}

	public static IPromise ProcessRedraw(GameScene scene, Redraw com)
	{
		scene.Redraw();
		return Promise.Resolved();
	}

	public static IPromise ProcessFocusCard(GameScene scene, FocusCard com)
	{
		return processFocusCard(scene, com).AsPromise();
	}

	public static IEnumerator processFocusCard(GameScene scene, FocusCard com)
	{
		var card = scene.Field.FindCard(com.CardId);
		var cr = scene.FindCardRenderer(card);
		cr.transform.SetAsLastSibling();
		var tween = cr.transform.DOLocalMove(Vector3.zero, 0.2f);
		cr.transform.DOScale(2.0f, 0.2f);
		yield return tween.WaitForCompletion();
		yield return new WaitForSeconds(0.2f);
		cr.transform.DOScale(1.0f, 0.1f);
	}

	public static IPromise ProcessShowMessage(GameScene scene, ShowMessage com)
	{
		scene.ShowMessage(Marker.RawText(com.Text));
		return Promise.Resolved();
	}

	public static IPromise ProcessSelectCard(GameScene scene, SelectCard com)
	{
		return CardSelector.Open(Marker.T("えらんでね").Text, scene.Field.Stack).Then(card =>
		{
			com.OutCardId = card.Id;
			return Promise.Resolved();
		});
	}

	public static IPromise ProcessAddCard(GameScene scene, AddCard com)
	{
		var card = scene.Field.FindCard(com.CardId);
		scene.AddCardToField(card);
		return Promise.Resolved();
	}
}
