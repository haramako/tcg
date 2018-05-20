using System.Collections.Generic;

namespace GameLog
{

	public interface ICommand {}

	public partial class CardPlayed : ICommand { }
	public partial class FocusCard : ICommand { }
	public partial class Redraw : ICommand { }
	public partial class SelectCard : ICommand { }
	public partial class ShowMessage : ICommand { }

}
