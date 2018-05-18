using System.Collections.Generic;
using Master;

namespace Game
{
	/// <summary>
	/// ゲーム内のキャラクター
	/// </summary>
	public sealed partial class Character : Google.ProtocolBuffers.Message
	{
		/// <summary>
		/// カウント0のステータスがあるかどうか(GarbageCollectで使う)
		/// </summary>
		bool hasGarbageStatus_ = false;

		public bool IsDead { get; private set; }
		public bool IsPlayer => Type == CharacterType.Player;
		public bool IsMonster => Type == CharacterType.Enemy;

		/// <summary>
		/// 特定の状態異常を返す
		/// </summary>
		/// <param name="stat">Stat.</param>
		public StatusChange GetStatus(CharacterStatus stat, bool assertDead = true)
		{
			if (assertDead)
			{
				Logger.Assert(!IsDead);
			}

			var len = StatusList.Count;
			for (int i = 0; i < len; i++)
			{
				if (StatusList[i].Status == stat)
				{
					return StatusList[i];
				}
			}
			return null;
		}

		public int GetStatusCount(CharacterStatus stat)
		{
			var s = GetStatus(stat);
			if (s == null)
			{
				return 0;
			}
			else
			{
				return s.Count;
			}
		}

		public bool IsStatusActive(CharacterStatus stat)
		{
			var count = GetStatusCount(stat);
			return count > 0;
		}

		public bool IsStatusInfinite(CharacterStatus stat)
		{
			var count = GetStatusCount(stat);
			return count >= 255;
		}

		public struct SetStatusResult
		{
			/// <summary>
			/// 相殺した状態異常
			/// </summary>
			public List<CharacterStatus> Againsts;

			/// <summary>
			/// 指定した状態異常をセットしたか
			/// </summary>
			public bool Result;
		}

		public void IncrementStatus(CharacterStatus stat, int count)
		{
			var orig = GetStatusCount(stat);
			SetStatus(stat, orig + count);
		}

		/// <summary>
		/// 状態異常を追加する
		/// </summary>
		/// <returns>対象の状態異常が存在した場合、trueを返す</returns>
		public SetStatusResult SetStatus(CharacterStatus stat, int count)
		{
			Logger.Assert(!IsDead);

			var res = new SetStatusResult();

			var len = StatusList.Count;
			for (int i = 0; i < len; i++)
			{
				if (StatusList[i].Status == stat)
				{
					StatusList[i].Count = count;
					res.Result = true;
					return res;
				}
			}

			// 相殺・上書きする状態異常があったら、その処理を行う
			bool againstExists = false; // 相殺ステータスが存在したかどうか
			var newStatInfo = G.FindStatusInfoById((int)stat); // 今回追加するステータス情報
			res.Againsts = new List<CharacterStatus>();
			for (int i = 0; i < len; i++)
			{
				var curStat = StatusList[i];
				if (newStatInfo.Overwrite.Contains(curStat.Status))
				{
					// 上書きなら、既存のステータスを消す
					RemoveStatus(curStat.Status);
				}
				else if (newStatInfo.Against.Contains(curStat.Status))
				{
					// 相殺なら、既存のステータスを消して、相殺フラグを立てる
					RemoveStatus(curStat.Status);
					againstExists = true;

					res.Againsts.Add(stat);
				}
			}

			if (!againstExists)
			{

				StatusList.Add(new StatusChange { Status = stat, Count = count });
			}

			res.Result = false;
			return res;
		}

		/// <summary>
		/// 状態異常を削除する
		/// </summary>
		/// <returns>対象の状態異常が存在した場合、trueを返す</returns>
		public bool RemoveStatus(CharacterStatus stat)
		{
			Logger.Assert(!IsDead);

			var len = StatusList.Count;
			for (int i = 0; i < len; i++)
			{
				if (StatusList[i].Status == stat && StatusList[i].Count > 0)
				{
					StatusList[i].Count = 0;
					hasGarbageStatus_ = true;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 状態異常をすべて削除する
		/// </summary>
		/// <returns>対象の状態異常が一つ以上存在した場合、trueを返す</returns>
		public bool RemoveAllStatus()
		{
			Logger.Assert(!IsDead);
			if (StatusList.Count == 0)
			{
				return false;
			}
			StatusList.Clear();
			hasGarbageStatus_ = true;
			return true;
		}

		/// <summary>
		/// 状態異常のカウントを下げる
		/// </summary>
		/// <returns>変化の結果、カウントが０になった場合のみtrueを返す（元から０の場合や０にならなかった場合、falseを返す）</returns>
		public bool DecrementStatus(CharacterStatus stat)
		{
			var s = GetStatus(stat);
			if (s == null || s.Count >= 255 || s.Count == 0)
			{
				return false;
			}

			s.Count--;
			if (s.Count == 0)
			{
				hasGarbageStatus_ = true;
			}

			return (s.Count == 0);
		}

		/// <summary>
		/// 状態異常のカウントが０のものを消す
		/// </summary>
		public void GarbageCollectStatus()
		{
			if (!hasGarbageStatus_)
			{
				return;
			}

			// カウントが０の要素を後ろから消していく
			var len = StatusList.Count;
			for (int i = len - 1; i >= 0; i--)
			{
				if (StatusList[i].Count <= 0)
				{
					StatusList.RemoveAt(i);
				}
			}

			hasGarbageStatus_ = false;
		}

	}

}
