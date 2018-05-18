using System.Collections.Generic;
using System.Linq;


namespace Game
{
    public static class TurnProcessing
    {
		static public void EndTurn(Field f)
        {
            f.FieldInfo.Power = 10;
            f.FieldInfo.Turn += 1;
            f.ShowMessage(Marker.T("ターン {0} 開始").Format(f.FieldInfo.Turn));

            foreach (var card in f.Hands.ToArray())
            {
                f.MoveToGrave(card);
            }
            Playing.Redraw(f);

            for (int i = 0; i < 5; i++)
            {
                Playing.Draw(f);
            }
            Playing.Redraw(f);
        }
    }
}
