using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RSG;
using Master;
using Game;

public class CardSelector : MonoBehaviour
{
	public Text MessageText;
	public PoolBehaviour ListItemPool;
	public ScrollRect ScrollRect;

	class Item
	{
		public Card Card;
		public CardRenderer CardRenderer;
	}

	Promise<Card> onClose_ = new Promise<Card>();
	List<Item> items_;

	/// <summary>
	/// エラーダイアログ
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public static IPromise<Card> Open(string message, IEnumerable<Card> cards)
	{
		return ResourceCache.Create<GameObject>("CardSelector")
			   .WithScreenLock()
			   .Then(obj =>
		{
			var dialog = obj.GetComponent<CardSelector>();
			dialog.open(message, cards);
			ScreenLocker.Modal(obj);
			return (IPromise<Card>)dialog.onClose_;
		});
	}

	void open(string message, IEnumerable<Card> cards)
	{
		MessageText.text = message;
		var s = GameScene.Instance;

		items_ = cards.Select(c => new Item { Card = c }).ToList();
		foreach( var item in items_)
		{
			var obj = ListItemPool.Create();
			item.Card.Reversed = false;
			var cr = s.CreateCardRenderer(obj, item.Card);
			cr.transform.localPosition = Vector3.zero;
			cr.OnClick = card => close(card);
			item.CardRenderer = cr;
		}

		ScrollRect.content.ForceUpdateRectTransforms();
		ScrollRect.verticalNormalizedPosition = 1.0f;
	}

	void close(Card card)
	{
		var s = GameScene.Instance;
		foreach (var item in items_)
		{
			s.ReleaseCardRenderer(item.CardRenderer.gameObject);
		}

		onClose_.Resolve(card);
		ScreenLocker.Unmodal(gameObject);
	}

}
