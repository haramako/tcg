using System;
using System.Collections.Generic;
using Master;

namespace Game 
{
	public class SpecialParam
    {
		public Card Card;
    }

	/// <summary>
    /// カードの特殊能力を処理するクラス
    /// </summary>
    public abstract class Special
    {
        public Master.SpecialTemplate T;

        static Dictionary<SpecialType, Type> specialsByType_ = new Dictionary<SpecialType, Type>();

        static Special()
        {
            foreach (var name in Enum.GetNames(typeof(SpecialType)))
            {
                var type = Type.GetType("Game.Specials." + name, false, true);
                if (type != null)
                {
                    var val = (SpecialType)Enum.Parse(typeof(SpecialType), name);
                    specialsByType_[val] = type;
                }
                else
                {
                    // UnityEngine.Debug.LogError("Game.Thinkings." + name + " not found");
                }
            }
        }

        public virtual void Execute(Field f, SpecialParam p)
        {
        }

		public virtual TextMarker GetDesc()
        {
			return Marker.T("GetDesc()が未実装");
        }

        static public Special Create(Master.SpecialTemplate t)
        {
            Type type;
            if (specialsByType_.TryGetValue(t.Type, out type))
            {
                var instance = (Special)Activator.CreateInstance(type);
                instance.T = t;
                return instance;
            }
            else
            {
                throw new Exception(Marker.D("Special '{0}' が存在しません。").Format(t.Type).Text);
            }
        }

    }
}
