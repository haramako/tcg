using System.Collections.Generic;
using Game;

namespace GameLog
{

	public interface IRequest
	{
		void Process(Field f);
	}

	public partial class AckResponseRequest : IRequest
	{
		public void Process(Field f)
		{

		}
	}

	public partial class ShutdownRequest : IRequest
	{
		public void Process(Field f)
		{

		}
	}

	public partial class PlayCardRequest : IRequest
	{
		public void Process(Field f)
		{
			var card = f.FindCard(CardId);
			Playing.Play(f, card);
		}
	}

	public partial class DrawCardRequest : IRequest
	{
		public void Process(Field f)
		{
			Playing.Draw(f);
		}
	}

}
