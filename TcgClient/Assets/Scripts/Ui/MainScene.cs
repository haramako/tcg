using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Master;
using System.IO;

public class MainScene : MonoBehaviour
{
	public PoolBehaviour CardPool;
	public GameObject Field;

	List<Card> cards = new List<Card>();

	void Start ()
	{
		Configure.Init();
		ResourceCache.Init();

		G.Initialize(new LocalFileSystem(Path.Combine("..", "Output")));
		G.LoadAll();

		for( int i = 0; i < 10; i++)
		{
			var n = Random.Range(0, G.CardTemplates.Count);
			var c = new Card { Id = i, CardTemplateId = G.CardTemplates[n].Id };
			cards.Add(c);
		}

		redrawCards();
	}

	void redrawCards()
	{
		foreach( var card in cards)
		{
			var obj = CardPool.Create(Field);
			var tc = obj.GetComponent<CardRenderer>();
			tc.Redraw(card);
			tc.transform.localPosition = new Vector3(card.Id * 100, 0, 0);
		}
	}

	public void OnCardClick()
	{
	}

}
