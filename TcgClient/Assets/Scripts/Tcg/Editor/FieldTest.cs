using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System;

namespace Game
{
	public class FieldTest
	{
		Field f_;

		List<Card> cards_;

		Card makeCard(int cardTemplateId)
		{
			return new Card { CardTemplateId = cardTemplateId };
		}

		List<Card> makeCards(int n, int cardTemplateId)
		{
			var list = new List<Card>(n);
			for (int i = 0; i < n; i++)
			{
				list.Add(new Card { CardTemplateId = cardTemplateId });
			}
			return list;
		}

		[SetUp]
		public void SetUp()
		{
			f_ = new Field();
		}

		[TearDown]
		public void TearDown()
		{
			f_.Validate();
		}

		[Test]
		public void TestInit()
		{
			Assert.AreEqual(f_.AllCards.Count, 0);
			Assert.AreEqual(f_.Hands.Count, 0);
			Assert.AreEqual(f_.Stack.Count, 0);
			Assert.AreEqual(f_.Opened.Count, 0);
		}

		[Test]
		public void TestAddAndRemoveCard()
		{
			var c = makeCards(2, 1);

			f_.AddCard(c[0]);
			Assert.AreEqual(f_.AllCards.Count, 1);

			f_.AddCard(c[1]);
			Assert.AreEqual(f_.AllCards.Count, 2);

			f_.RemoveCard(c[0]);
			Assert.AreEqual(f_.AllCards.Count, 1);

			f_.RemoveCard(c[1]);
			Assert.AreEqual(f_.AllCards.Count, 0);

			Assert.Catch(() => f_.RemoveCard(c[0]));

		}

		[Test]
		public void TestMove()
		{
			var c = makeCards(3, 1);

			f_.AddCards(c);
			Assert.AreEqual(f_.AllCards.Count, 3);

			f_.MoveToHands(c[0]);
			Assert.AreEqual(f_.Hands.Count, 1);
			Assert.AreEqual(c[0].Place, CardPlace.Hands);

			f_.MoveToStack(c[1]);
			Assert.AreEqual(f_.Stack.Count, 1);
			Assert.AreEqual(c[1].Place, CardPlace.Stack);

			f_.MoveToOpened(c[2]);
			Assert.AreEqual(f_.Opened.Count, 1);
			Assert.AreEqual(c[2].Place, CardPlace.Opened);

			f_.MoveToHands(c[1], 0);
			Assert.AreEqual(f_.Hands.Count, 2);
			Assert.AreEqual(f_.Stack.Count, 0);
			Assert.AreEqual(f_.Hands[0], c[1]);

			f_.MoveToHands(c[2], -2);
			Assert.AreEqual(f_.Hands.Count, 3);
			Assert.AreEqual(f_.Opened.Count, 0);
			Assert.AreEqual(f_.Hands[1], c[2]);

			f_.MoveToTemp(c[2]);
			Assert.AreEqual(f_.Hands.Count, 2);

			f_.RemoveCard(c[2]);
		}


	}
}