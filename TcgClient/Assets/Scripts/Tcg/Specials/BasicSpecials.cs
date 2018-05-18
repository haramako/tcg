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
			f.ShowMessage(Marker.T("{0}のダメージ！").Format(T.Amount));
			p.Target.Hp -= T.Amount;
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("ダメージ({0})").Format(T.Amount);
		}
	}

	public class Defense : Special
	{
		public override void Execute(Field f, SpecialParam p)
		{
			f.ShowMessage(Marker.T("{0}のブロック！").Format(T.Amount));
			p.Executer.IncrementStatus(CharacterStatus.Block, T.Amount);
			Playing.Redraw(f);
		}

		public override TextMarker GetDesc()
		{
			return Marker.T("ブロック({0})").Format(T.Amount);
		}
	}
}
