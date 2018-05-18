using System.Collections.Generic;
using System.Linq;


namespace Game
{
	public static class Playing
	{
		static public void Play(Field f, Card card)
		{
			Logger.Assert(card.IsHands);
			Logger.Assert(f.FieldInfo.Power >= card.T.Cost);

            f.SendAndWait(new GameLog.FocusCard { CardId = card.Id });

			f.FieldInfo.Power -= card.T.Cost;
			var param = new SpecialParam { Card = card };
			card.ExecuteSpecial(f, param);

			f.MoveToGrave(card);
			Playing.Redraw(f);
		}

		static public void Draw(Field f)
		{
			if( !f.HasStack() ){
				CardMoving.RecycleGrave(f);
			}

			Logger.Assert(f.Stack.Count > 0);
			var card = f.StackTop();
			f.MoveToHands(card);
			Playing.Redraw(f);
		}

		static public void Redraw(Field f)
		{
			f.SendAndWait(new GameLog.Redraw());
		}

	}
}