using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace Game
{
	public class OpenedCard
	{
		public Card Card;
	}

	public class Field
	{
		/// <summary>
		/// 全てのカード
		/// </summary>
		public IReadOnlyList<Card> AllCards => allCards_;

		/// <summary>
		/// 手札
		/// </summary>
		public IReadOnlyList<Card> Hands => RawHands;

		/// <summary>
		/// 山札
		///
		/// インデックスは[0]は山の底で、[Count-1]が山の頭を表す
		/// </summary>
		public IReadOnlyList<Card> Stack => RawStack;

		/// <summary>
		/// オープンされたカード
		/// </summary>
		public IReadOnlyList<Card> Opened => RawOpened;

		/// <summary>
		/// 手札(編集用のアクセスが必要な時のみ使用して、基本は使わない)
		/// </summary>
		List<Card> RawHands => hands_;

		/// <summary>
		/// 山札(編集用のアクセスが必要な時のみ使用して、基本は使わない)
		/// </summary>
		List<Card> RawStack => stack_;

		/// <summary>
		/// オープンされたカード(編集用のアクセスが必要な時のみ使用して、基本は使わない)
		/// </summary>
		List<Card> RawOpened => opened_;

		/// <summary>
		/// すべてのカード
		/// 移動がちゃんと行われたかを検証するためのもの
		/// </summary>
		List<Card> allCards_ = new List<Card>();

		List<Card> hands_ = new List<Card>();
		List<Card> stack_ = new List<Card>();
		List<Card> opened_ = new List<Card>();

		/// <summary>
		/// 次に加わるカードのID
		/// </summary>
		int nextCardId_ = 1;

		public Field()
		{

		}

		//===================================================
		// 内部用の便利関数
		//===================================================

		void removeCard(Card card)
		{
			bool removed;
			switch (card.Place)
			{
				case CardPlace.Hands:
					removed = hands_.Remove(card);
					break;
				case CardPlace.Stack:
					removed = stack_.Remove(card);
					break;
				case CardPlace.Opened:
					removed = opened_.Remove(card);
					break;
				case CardPlace.Temp:
					removed = true;
					break;
				default:
					throw new InvalidOperationException();
			}

			if (!removed)
			{
				throw new InvalidOperationException("No card to remove.");
			}
		}

		/// <summary>
		/// Listへの挿入をを行う
		/// </summary>
		/// <param name="idx">挿入する場所(0:１番目, 1: ２番目, .. -1: 一番後ろ, -2: 後ろから２番目</param>
		static void insert(List<Card> list, Card card, int idx)
		{
			if (idx >= 0)
			{
				list.Insert(idx, card);
			}
			else if(idx == -1)
			{
				list.Add(card);
			}
			else
			{
				list.Insert(list.Count + 1 + idx, card);
			}
		}

		//===================================================
		// カードの操作
		//===================================================

		public Card FindCard(int cardId)
		{
			return AllCards.FirstOrDefault(c => c.Id == cardId);
		}

		public Card StackTop() => stack_[stack_.Count - 1];
		public bool HasStack() => stack_.Count > 0;

		public Card AddCard(Card card)
		{
			allCards_.Add(card);
			card.Id = nextCardId_;
			nextCardId_++;
			card.place_ = CardPlace.Temp;
			return card;
		}

		public void AddCards(IEnumerable<Card> cards)
		{
			foreach (var card in cards)
			{
				AddCard(card);
			}
		}

		public void RemoveCard(Card card)
		{
			removeCard(card);
			if( !allCards_.Remove(card) )
			{
				throw new InvalidOperationException("No card to remove");
			}
			card.place_ = CardPlace.Temp;
		}

		public void RemoveCards(IEnumerable<Card> cards)
		{
			foreach (var card in cards)
			{
				RemoveCard(card);
			}
		}

		public void MoveToHands(Card card, int idx = -1)
		{
			removeCard(card);
			insert(hands_, card, idx);
			card.place_ = CardPlace.Hands;
		}

		public void MoveToStack(Card card, int idx = -1)
		{
			removeCard(card);
			insert(stack_, card, idx);
			card.place_ = CardPlace.Stack;
		}

		public void MoveToStackTop(Card card) => MoveToStack(card);
		public void MoveToStackBottom(Card card) => MoveToStack(card, 0);

		public void MoveToOpened(Card card, int idx = -1)
		{
			removeCard(card);
			insert(opened_, card, idx);
			card.place_ = CardPlace.Opened;
		}

		public void MoveToTemp(Card card)
		{
			removeCard(card);
			card.place_ = CardPlace.Temp;
		}

		//===================================================
        // バリデーション
        //===================================================

		/// <summary>
		/// 場の状態が正しいかを確認する
		/// </summary>
		/// <param name="allowTemp">Temp状態のカードを許すか</param>
		public void Validate(bool allowTemp = false)
		{
			foreach( var card in hands_ )
			{
				if (card.Place != CardPlace.Hands)
				{
					throw new Exception(string.Format("Card {0} must in hand, but not found", card.Id));
				}
			}

			foreach (var card in stack_)
			{
				if (card.Place != CardPlace.Stack)
				{
					throw new Exception(string.Format("Card {0} must in stack, but not found", card.Id));
				}
			}

			foreach (var card in opened_)
			{
				if (card.Place != CardPlace.Opened)
				{
					throw new Exception(string.Format("Card {0} must in opened, but not found", card.Id));
				}
			}

			// 全てのカードが AllCards に含まれているか確認する
			if (!allowTemp)
			{
				if (allCards_.Count != hands_.Count + stack_.Count + opened_.Count)
				{
					throw new Exception(string.Format("Card count is invalid, all={0}, hands={1}, stack={2}, opened={3}",
													  allCards_.Count, hands_.Count, stack_.Count, opened_.Count));
				}
			}

			// 同じIDのカードがないか確認する
			foreach( var c1 in allCards_)
			{
				if( allCards_.Count(c2 => c2.Id == c1.Id) != 1 )
				{
					foreach( var c in allCards_)
					{
						Console.Write("{0}({1}), ", c.Id, c.Place);
					}
					Console.WriteLine();
					throw new Exception(string.Format("Card {0} duplicated", c1.Id));
				}
			}
		}
	}
}
