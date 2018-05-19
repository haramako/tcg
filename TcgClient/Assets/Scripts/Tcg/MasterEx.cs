using System.Collections.Generic;
using System.Linq;
using Game;

namespace Master
{
	public partial class CardTemplate
	{
		/// <summary>
		/// 特殊能力
		///
		/// SpecialTemplateからロード時に生成される
		/// </summary>
		public Special[] Special { get; private set; }

		public void OnLoaded()
		{
			foreach (var t in SpecialTemplate)
			{
				Logger.Info("{0} {1}", Name, t.Type);
			}
			Special = SpecialTemplate.Select(t => Game.Special.Create(t)).ToArray();
		}
	}
}
