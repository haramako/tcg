﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System;
using System.IO;
using System.Linq;
using DG.Tweening;
using RSG;

using Game;
using Master;

public class GameScene : MonoSingleton<GameScene>
{
	public PoolBehaviour CardPool;
	public PoolBehaviour EnemyPool;

	public GameObject FieldHolder;

	public Graphic HandHolder;
	public Graphic StackHolder;
	public Graphic GraveHolder;
	public Graphic OpenedHolder;
	public Graphic EnemyHolder;

	public Text StackNumText;
	public Text GraveNumText;
	public Text CostText;
	public Text LogText;

	public Text PlayerInfoText;
	public Text EnemyInfoText;

	public Field Field;

	public Dictionary<int, CardRenderer> cardRenderers_ = new Dictionary<int, CardRenderer>();
	public Dictionary<int, EnemyRenderer> enemyRenderers_ = new Dictionary<int, EnemyRenderer>();

	protected override void Awake()
	{
		base.Awake();

		Promise.UnhandledException += promiseExceptionHandler;

		Configure.Init();
		ResourceCache.Init();
		ResourceCache.AssetBundleResource.LocalPathFromFile = localPathFromFile;
	}

	private void promiseExceptionHandler(object sender, ExceptionEventArgs e)
	{
		Debug.LogException(e.Exception);
	}

	ResourceCache.FileInfo localPathFromFile(string path)
	{
		return new ResourceCache.FileInfo
		{
			Path = Path.Combine(Application.streamingAssetsPath, ResourceCache.PlatformDir(), path)
		};
	}

	IEnumerator Start ()
	{
		G.Initialize(new LocalFileSystem(Path.Combine("..", "Output")));
		G.LoadAll();

		foreach( var c in G.ConfigInfos)
		{
			ConfigLoader.SetValue("Game." + c.Id, c.Value);
		}

		Field = new Field();
		Field.Conn.RequestTimeoutMillis = 1000;
		Field.StartThread();

		ShowMessage(Marker.T("ゲーム開始"));

		for( int i = 0; i < Config.DefaultDrawCount; i++)
		{
			var card = AddCardToField(Field.AddCard(randCard()));
			Field.MoveToHands(card);
		}

		for (int i = 0; i < Config.DefaultDeckCount - Config.DefaultDrawCount; i++ )
		{
			var card = AddCardToField(Field.AddCard(randCard()));
			Field.MoveToStack(card);
		}

		foreach( var c in Field.Characters )
		{
			if (c.Id == 1)
			{
				continue;
			}
			var er = CreateEnemyRenderer(EnemyHolder.gameObject, c);
			yield return er.LoadResource(c).AsCoroutine();
		}

		redraw();
	}

	protected override void OnDestroy()
	{
		if( Field != null )
		{
			Field.Conn.Shutdown();
		}
	}

	Card randCard()
	{
		var n = UnityEngine.Random.Range(0, G.CardTemplates.Count);
		return new Card { CardTemplateId = G.CardTemplates[n].Id };
	}

	public Card AddCardToField(Card card)
	{
		var obj = CardPool.Create(FieldHolder);
		var cr = obj.GetComponent<CardRenderer>();
		cardRenderers_.Add(card.Id, cr);
		cr.Redraw(card);
		cr.OnClick = onCardClick;
		return card;
	}

	public CardRenderer CreateCardRenderer(GameObject parent, Card card)
	{
		var obj = CardPool.Create(parent);
		var cr = obj.GetComponent<CardRenderer>();
		cr.Redraw(card);
		return cr;
	}

	public void ReleaseCardRenderer(GameObject obj)
	{
		CardPool.Release(obj);
	}

	public CardRenderer FindCardRenderer(Card card) => cardRenderers_[card.Id];
	public CardRenderer FindCardRenderer(int cardId) => cardRenderers_[cardId];

	public EnemyRenderer CreateEnemyRenderer(GameObject parent, Character character)
	{
		var obj = EnemyPool.Create(parent);
		var er = obj.GetComponent<EnemyRenderer>();
		enemyRenderers_[character.Id] = er;
		//er.Redraw(character);
		return er;
	}

	public void ReleaseEnemyRenderer(GameObject obj)
	{
		EnemyPool.Release(obj);
	}

	public EnemyRenderer FindEnemyRenderer(Character character) => enemyRenderers_[character.Id];
	public EnemyRenderer FindEnemyRenderer(int characterId) => enemyRenderers_[characterId];

	public void Redraw() => redraw();

	void redraw()
	{
		redrawCards();

		StackNumText.text = "" + Field.Stack.Count;
		GraveNumText.text = "" + Field.Grave.Count;
		CostText.text = "" + Field.FieldInfo.Mana;

		PlayerInfoText.text = GetCharacterStatus(Field.Player);
		EnemyInfoText.text = GetCharacterStatus(Field.Characters[1]);
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
				var pos = GraveHolder.transform.localPosition;
				cr.gameObject.transform.SetAsLastSibling();
				cr.transform.DOLocalMove(pos, 0.3f);
				cr.Redraw(cr.Card);
			}
			else
			{
				cr.gameObject.SetActive(false);
			}
			i++;
		}

		i = 0;
		foreach (var card in Field.Discarded)
		{
			var cr = FindCardRenderer(card);
			cr.gameObject.SetActive(false);
			i++;
		}

		i = 0;
		foreach (var c in Field.Characters)
		{
			if( c.Id == 1 )
			{
				continue;
			}
			var er = FindEnemyRenderer(c);
			er.gameObject.SetActive(true);
			var pos = GetCardPosition(EnemyHolder, Field.Characters.Count - 1, i, 400);
			er.gameObject.transform.SetAsLastSibling();
			er.transform.DOLocalMove(pos, 0.3f);
			er.Redraw(c);
			i++;
		}

	}

	void onCardClick(Card card)
	{
		switch( card.Place )
		{
			case CardPlace.Hands:
				if( card.T.Cost > Field.FieldInfo.Mana )
				{
					return;
				}
				Send(new GameLog.PlayCardRequest { CardId = card.Id });
				break;
			case CardPlace.Stack:
				Send(new GameLog.DrawCardRequest { });
				break;
			default:
				break;
		}
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
	}

	public void OnTurnEndClick()
	{
		Send(new GameLog.TurnEndRequest { });
		//CommonDialog.Open("HOGE?").Done(n => Debug.Log("Done " + n));
	}

	public void OnTestClick()
	{
		CardSelector.Open("えらんでね", Field.Stack).Done(card => { Debug.Log(card.T.Name); });
	}

	const int CardWidth = 160;
	const int CardHeight = 230;

	public Vector3 GetCardPosition(Graphic placeHolder, int num, int idx, int space = CardWidth)
	{
		var rt = placeHolder.rectTransform;
		var rect = rt.rect;
		var width = rect.width;
		Vector3 localPos;
		if( width >= space * num )
		{
			localPos = new Vector3(rect.xMin + space * idx + space / 2, rect.center.y);
		}
		else
		{
			var margin = (width - space) / (num - 1);
			localPos = new Vector3(rect.xMin + margin * idx + space / 2, rect.center.y);
		}
		return FieldHolder.transform.InverseTransformPoint(placeHolder.transform.TransformPoint(localPos));
	}

	List<GameLog.ICommand> commands_ = new List<GameLog.ICommand>();

	public void Send(GameLog.IRequest request)
	{
		Game.Logger.Assert(commands_.Count <= 0);

		if (request == null)
		{
			throw new System.ArgumentNullException("Request must not be null");
		}

		if (Field.Conn.State == ConnectionState.Shutdowned)
		{
			throw new InvalidOperationException(Marker.D("Shutdown済みのFieldにリクエストを送ろうとしました").Text);
		}

		Profiler.BeginSample("Request");
		commands_ = Field.Conn.Request(request);
		Profiler.EndSample();

		Game.Logger.Assert(commands_ != null);

		// エラーが起こっていた場合、
		if (Field.Conn.State == ConnectionState.Shutdowned)
		{
			Debug.LogException(Field.Conn.ShutdownError);
			return;
		}

		updateProcesssCommand(); // 最初のコマンドをすぐに実行する
	}

	void updateProcesssCommand()
	{
		GetComponent<GraphicRaycaster>().enabled = false;
		if (commands_.Count <= 0)
		{
			switch (Field.Conn.WaitingType)
			{
				case WaitingType.None:
					//throw new Exception("Invalid WatingType==None");
					break;
				case WaitingType.Ack:
					Send(new GameLog.AckResponseRequest());
					return;
			}
			GetComponent<GraphicRaycaster>().enabled = true;
			return;
		}

		var command = commands_[0];
		commands_.RemoveAt(0);

		CommandProcessor.Process(this, command).DelayFrame(2).Done(updateProcesssCommand);
	}

	List<string> messages = new List<string>();
	public void ShowMessage(TextMarker message)
	{
		var str = message.Text;
		Debug.Log(str);
		messages.Add(str);
		if (messages.Count > 4)
		{
			messages.RemoveAt(0);
		}
		LogText.text = string.Join("\n", messages.ToArray());
	}


	public string GetCharacterStatus(Character c)
	{
		var r = new System.Text.StringBuilder();

		r.AppendFormat("HP: {0}\n", c.Hp);
		foreach( var stat in c.StatusList)
		{
			if (stat.Count > 0)
			{
				var info = G.FindStatusInfoById((int)stat.Status);
				r.AppendFormat("{0}({1})\n", info.Name, stat.Count);
			}
		}

		return r.ToString();
	}

}
