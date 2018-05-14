using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using Master;
using System.IO;
using DG.Tweening;

public class MainScene : MonoBehaviour
{
	public PoolBehaviour CardPool;
	public GameObject FieldHolder;

	public Graphic HandHolder;
	public Graphic StackHolder;
	public Graphic GraveHolder;
	public Graphic OpenedHolder;

	public Text StackNumText;
	public Text GraveNumText;

	public Field Field;

	public Dictionary<int, CardRenderer> cardRenderers_ = new Dictionary<int, CardRenderer>();

	private void Awake()
	{
		Configure.Init();
        ResourceCache.Init();
	}

	void Start ()
	{
		G.Initialize(new LocalFileSystem(Path.Combine("..", "Output")));
		G.LoadAll();

		Field = new Field();

		for( int i = 0; i < 10; i++)
		{
			Field.MoveToHands(AddCardToField(randCard()));
		}

		for (int i = 0; i < 20; i++ ){
			Field.MoveToStack(AddCardToField(randCard()));
		}

		redraw(); 
	}

	Card randCard()
	{
		var n = Random.Range(0, G.CardTemplates.Count);
        return new Card { CardTemplateId = G.CardTemplates[n].Id };
	}

	public Card AddCardToField(Card card)
	{
		Field.AddCard(card);
		var obj = CardPool.Create(FieldHolder);
        var cr = obj.GetComponent<CardRenderer>();
		cardRenderers_.Add(card.Id, cr);
		cr.Redraw(card);
		return card;
    }

	public CardRenderer FindCardRenderer(Card card) => cardRenderers_[card.Id];
	public CardRenderer FindCardRenderer(int cardId) => cardRenderers_[cardId];

	void redraw()
	{
		redrawCards();

		StackNumText.text = "" + Field.Stack.Count;
		GraveNumText.text = "" + Field.Grave.Count;
	}

	void redrawCards()
	{
		var i = 0;
		foreach( var card in Field.Hands)
		{
			var cr = FindCardRenderer(card);
			var pos = GetCardPosition(HandHolder, Field.Hands.Count, i);
			cr.gameObject.SetActive(true);
			cr.gameObject.transform.SetAsLastSibling();
			cr.transform.DOLocalMove(pos, 0.3f);
			card.Reversed = false;
			cr.Redraw(cr.Card);
			i++;
		}

		i = 0;
        foreach (var card in Field.Opened)
        {
			var cr = FindCardRenderer(card);
            var pos = GetCardPosition(OpenedHolder, Field.Opened.Count, i);
			cr.gameObject.SetActive(true);
			cr.transform.DOLocalMove(pos, 0.3f);
			cr.gameObject.transform.SetAsLastSibling();
			card.Reversed = false;
			cr.Redraw(cr.Card);
            i++;
        }

		i = 0;
        foreach (var card in Field.Stack)
        {
			var cr = FindCardRenderer(card);
			if (i == Field.Stack.Count - 1)
			{
				cr.gameObject.SetActive(true);
				card.Reversed = true;
				cr.transform.localPosition = StackHolder.transform.localPosition;
				cr.gameObject.transform.SetAsLastSibling();
				cr.Redraw(cr.Card);
			}
			else
			{
                cr.gameObject.SetActive(false);
			}
            i++;
        }

		i = 0;
        foreach (var card in Field.Grave)
        {
            var cr = FindCardRenderer(card);
            if (i == Field.Grave.Count - 1)
            {
                cr.gameObject.SetActive(true);
                card.Reversed = true;
                cr.transform.localPosition = GraveHolder.transform.localPosition;
                cr.gameObject.transform.SetAsLastSibling();
                cr.Redraw(cr.Card);
            }
            else
            {
                cr.gameObject.SetActive(false);
            }
            i++;
        }
	}

	public void OnCardClick(GameObject target)
	{
		var id = target.GetId();
		var card = Field.FindCard(id);
		switch( card.Place ){
			case CardPlace.Hands:
				Playing.Play(Field, card);
				break;
			case CardPlace.Stack:
				Playing.Draw(Field);
                break;
			default:
				break;
		}
		redraw();
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
	}

	public void OnButtonClick()
	{
		CommonDialog.Open("HOGE?").Done(n => Debug.Log("Done " + n));
	}

	int CardWidth = 120;
	int CardHeight = 180;

	public Vector3 GetCardPosition(Graphic placeHolder, int num, int idx)
	{
		var rt = placeHolder.rectTransform;
		var rect = rt.rect;
		var width = rect.width;
		Vector3 localPos;
		if( width >= CardWidth * num )
		{
			localPos = new Vector3(rect.xMin + CardWidth * idx + CardWidth / 2, rect.center.y);
		}
		else
		{
			var margin = (width - CardWidth) / (num - 1);
			localPos = new Vector3(rect.xMin + margin * idx + CardWidth / 2, rect.center.y);
		}
		return FieldHolder.transform.InverseTransformPoint(placeHolder.transform.TransformPoint(localPos));
	}

}
