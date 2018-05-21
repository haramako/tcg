using System.Collections.Generic;
using UnityEngine;
using Game;
using Master;

namespace Game.Specials
{

	public class Attack : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			f.ShowMessage(Marker.T("{0}のダメージ！").Format(amount));
			p.Target.Hp -= amount;
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("{0}のダメージ").Format(GetAmountDesc().Text);
		}
	}

	public class Defense : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			f.ShowMessage(Marker.T("{0}のブロック！").Format(amount));
			p.Executer.IncrementStatus(CharacterStatus.Block, amount);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("{0}のブロック").Format(GetAmountDesc().Text);
		}
	}

	public class Draw : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			for( int i = 0; i < T.Amount; i++)
			{
				Playing.Draw(f);
			}
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("{0}枚カードを引く").Format(T.Amount);
		}
	}

	public class AddStatus : Special
	{
		public string GetStatusMessage()
		{
			var msgList = new List<string>();
			foreach (var stat in T.Status)
			{
				msgList.Add(G.FindStatusInfo(stat).Name);
			}
			return string.Join(",", msgList.ToArray());
		}

		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			foreach (var stat in T.Status)
			{
				p.Target.IncrementStatus(stat, amount);
			}
			f.ShowMessage(Marker.T("{0}を{1}付与した").Format(GetStatusMessage(), amount));
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("{0}を{1}付与").Format(GetStatusMessage(), GetAmountDesc());
		}
	}

	public class AddCard : Special
	{

		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			for( int i = 0; i < amount; i++)
			{
				var card = CardMoving.AddCard(f, T.CardId);
				f.MoveToLocation(T.Location, card);
			}
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			var card = G.FindCardTemplateById(T.CardId);
			return Marker.T("{0}を{1}枚{2}に追加").Format(card.Name, GetAmountDesc(), G.DisplayName(T.Location));
		}
	}

	public class MoveCard : Special
	{
		public override bool IsPlayable(Field f, SpecialParam p)
		{
			if (f.Stack.Count <= 0)
			{
				return false;
			}
			return true;
		}

		public override bool Prepare(Field f, SpecialParam p)
		{
			var req = new GameLog.SelectCard();
			f.SendAndWait(req);
			var card = f.FindCard(req.OutCardId);
			p.PreparedSelectedCard = new List<Card> { card };
			return true;
		}

		public override void Execute(Field f, SpecialParam p)
		{
			foreach (var card in p.PreparedSelectedCard)
			{
				f.MoveToHands(card);
			}
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("カードを{0}枚選んで{1}に加える").Format(GetAmountDesc(), "手札");
		}
	}

	public class Discard : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			p.ResultDiscard = true;
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("破棄");
		}
	}

	public class CloneCard : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			for (int i = 0; i < amount; i++)
			{
				var card = CardMoving.AddCard(f, p.Card.T.Id);
				f.MoveToLocation(T.Location, card);
			}
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("コピーを{0}枚{1}に加える").Format(GetAmountDesc(), G.DisplayName(T.Location));
		}
	}

	public class AddMana : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			var amount = GetAmount(f, p);
			f.FieldInfo.Mana += amount;
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("マナを{0}に得る").Format(GetAmountDesc());
		}
	}


	#if false // テンプレート
	public class Draw : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
		}

		public override TextMarker GetDesc()
		{
		}
	}
	#endif
}
