using System.Collections.Generic;
using System.Linq;


namespace Game
{
	public static class Playing
	{
		static public void Play(Field f, Card card)
		{
			Logger.Assert(card.IsHands);
			f.MoveToOpened(card);
		}

		static public void Draw(Field f)
		{
			Logger.Assert(f.Stack.Count > 0);
		    var card = f.StackTop();
			f.MoveToHands(card);
		}
	}
}