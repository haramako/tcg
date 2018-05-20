using System.Collections.Generic;
using System.Linq;


namespace Game
{
	public static class CardMoving
	{
		static public Card AddCard(Field f, int cardId)
		{
			var template = G.FindCardTemplateById(cardId);
			var card = new Card { CardTemplateId = cardId };
			f.AddCard(card);
			f.Send(new GameLog.AddCard { CardId = card.Id });
			return card;
		}

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
