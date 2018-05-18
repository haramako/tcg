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
			Special = SpecialTemplate.Select(t => Game.Special.Create(t)).ToArray();
		}
	}
}
