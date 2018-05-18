using System.Collections.Generic;
using System.Linq;


namespace Game
{
	public static class CardMoving
	{
		static public void RecycleGrave(Field f)
        {
            var shuffledGrave = f.Grave.Shuffle(99);
            foreach (var card in shuffledGrave)
            {
                f.MoveToStack(card);
            }
            Playing.Redraw(f);
        }
	}
}
