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
			Playing.Redraw(f);
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
			Playing.Redraw(f);
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
