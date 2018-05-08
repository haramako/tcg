using System;
using System.Collections.Generic;

namespace Game
{

	static public class Utility
	{

		/// <summary>
		/// 現在時刻を種として取得
		/// </summary>
		static public int NewSeed
		{
			get { return (int)DateTime.Now.Ticks; }
		}
	}

	/// <summary>
	/// 擬似乱数ジェネレータークラス
	/// </summary>
	public abstract class RandomBase
	{

		/// <summary>
		/// 使用しているseed値
		/// </summary>
		protected int baseSeed;

		public int Seed { get { return baseSeed; } }

		/// <summary>
		/// 疑似乱数を生成した回数
		/// </summary>
		protected int baseCounter;

		public int Counter { get { return baseCounter; } }

		/// <summary>
		/// 擬似乱数を生成
		/// </summary>
		/// <returns>The U int32.</returns>
		public abstract UInt32 NextUInt32();

		/// <summary>
		/// 疑似乱数を生成 - 範囲指定(max値は含まない=unityEngine.Random互換)
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public virtual Int32 RangeInt(int min, int max)
		{
			if (min > max)
			{
				min ^= max;
				max ^= min;
				min ^= max;
			}
			if ((min + 1) >= max)
			{
				return min;
			}
			return (Int32)(min + (NextUInt32() / ((double)UInt32.MaxValue + 1.0f)) * (max - min));
		}

		/// <summary>
		/// 疑似乱数を生成して百分率抽選
		/// int version
		/// </summary>
		/// <param name="rate">Rate.</param>
		public virtual bool Probability(int rate)
		{
			if (rate <= 0)
			{
				return false;
			}
			if (rate >= 100)
			{
				return true;
			}
			int v = (int)((NextUInt32() / ((double)UInt32.MaxValue + 1.0f)) * 100);
			return (v < rate ? true : false);
		}

		/// <summary>
		/// 疑似乱数を生成して百分率抽選
		/// float Version
		/// </summary>
		/// <param name="rate">Rate.</param>
		public virtual bool Probability(float rate)
		{
			if (rate <= 0f)
			{
				return false;
			}
			if (rate >= 100f)
			{
				return true;
			}
			float v = (int)((NextUInt32() / ((double)UInt32.MaxValue + 1.0f)) * 100);
			return (v < rate ? true : false);
		}

		/// <summary>
		/// 疑似乱数を生成してList内抽選
		/// </summary>
		/// <returns>The lots.</returns>
		/// <param name="list">List.</param>
		/// <param name="maxcount">Maxcount.</param>
		public virtual int ListLots(ref List<int> list, ref int maxcount)
		{
			int rand = (int)((NextUInt32() / ((double)UInt32.MaxValue + 1.0f)) * maxcount);
			int idx = 0;
			foreach (int lotsint in list)
			{
				if (lotsint != 0)
				{
					rand -= lotsint;
					if (rand <= 0)
					{
						return idx;
					}
				}
				idx++;
			}
			return idx;
		}

		/// <summary>
		/// Lots the specified list and startIndex.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="startIndex">Start index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual T Lots<T>(ref List<T> list, int startIndex = 0)
		{
			int rand = RangeInt(startIndex, list.Count);
			return list[rand];
		}

		/// <summary>
		/// debug用
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RogueLib.Random.RandomBase"/>.</returns>
		public override string ToString()
		{
			return string.Format("<color=red><b>[RandomBase: Seed={0}, Counter={1}]</b></color>", Seed, Counter);
		}
	}

	/// <summary>
	/// xorshiftの擬似乱数ジェネレータークラス
	/// http://www.jstatsoft.org/v08/i14/paper
	/// </summary>
	public sealed class RandomXS : RandomBase
	{

		/// <summary>
		/// 内部状態ベクトル
		/// </summary>
		private UInt32 w, x, y, z;

		/// <summary>
		/// 擬似乱数ジェネレーターを初期化
		/// </summary>
		/// <param name="seed">Seed.</param>
		/// <param name="count">Count.</param>
		public RandomXS(int seed, int count = 0)
		{
			baseSeed = seed;
			baseCounter = 0;

			// initialize
			//w = (UInt32)seed; x = (w<<16) + (w>>16); y = w + x; z = x ^ y;
			w = (UInt32)seed;
			w = 1812433253U * (w ^ (w >> 30));
			x = 1812433253U * (w ^ (w >> 30)) + 1;
			y = 1812433253U * (x ^ (x >> 30)) + 2;
			z = 1812433253U * (y ^ (y >> 30)) + 3;

			// resume
			while (baseCounter != count)
			{
				NextUInt32();
			}
		}

		public int[] GetState()
		{
			return new int[] { (int)w, (int)x, (int)y, (int)z };
		}

		public void SetState(int[] stat)
		{
			w = (uint)stat[0];
			x = (uint)stat[1];
			y = (uint)stat[2];
			z = (uint)stat[3];
		}

		/// <summary>
		/// 符号なし32bitの擬似乱数を生成
		/// </summary>
		public override UInt32 NextUInt32()
		{
			baseCounter++;

			System.UInt32 t = (x ^ (x << 11));
			x = y;
			y = z;
			z = w;
			return (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)));
		}
	}

	#if false
	/// <summary>
	/// Mersenne Twisterの擬似乱数ジェネレータークラス
	/// http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/ARTICLES/mt.pdf
	/// </summary>
	public sealed class RandomMT : RandomBase
	{
		private const int MEXP = 19937;
		private const int POS1 = 122;
		private const int SL1 = 18;
		private const int SL2 = 1;
		private const int SR1 = 11;
		private const int SR2 = 1;
		private const UInt32 MSK1 = 0xdfffffefU;
		private const UInt32 MSK2 = 0xddfecb7fU;
		private const UInt32 MSK3 = 0xbffaffffU;
		private const UInt32 MSK4 = 0xbffffff6U;

		private const int N = MEXP / 128 + 1;
		private const int N32 = N * 4;
		private const int SL2_x8 = SL2 * 8;
		private const int SR2_x8 = SR2 * 8;
		private const int SL2_ix8 = 64 - SL2 * 8;
		private const int SR2_ix8 = 64 - SR2 * 8;

		private int idx;
		private UInt32[] sfmt;
		private UInt32[] PARITY = new UInt32[] { 0x00000001U, 0x00000000U, 0x00000000U, 0x13c9e684U };

		/// <summary>
		/// 擬似乱数ジェネレーターを初期化
		/// </summary>
		/// <param name="seed">Seed.</param>
		/// <param name="count">Count.</param>
		public RandomMT(int seed, int count = 0)
		{
			baseSeed = seed;
			baseCounter = 0;

			// initialize
			sfmt = new UInt32[N32];
			sfmt[0] = (UInt32)seed;
			for (int i = 1; i < N32; i++)
			{
				sfmt[i] = (UInt32)(1812433253 * (sfmt[i - 1] ^ (sfmt[i - 1] >> 30)) + i);
			}
			period_certification();
			idx = N32;

			// resume
			while (baseCounter != count)
			{
				NextUInt32();
			}
		}

		/// <summary>
		/// 符号なし32bitの擬似乱数を生成
		/// </summary>
		/// <returns>The U int32.</returns>
		public override UInt32 NextUInt32()
		{
			baseCounter++;

			if (idx >= N32)
			{
				idx = 0;
				// re-create new table
				int a = 0;
				int b = POS1 * 4;
				int c = (N - 2) * 4;
				int d = (N - 1) * 4;
				UInt32[] p = this.sfmt;
				do
				{
					p[a + 3] = p[a + 3] ^ (p[a + 3] << 8) ^ (p[a + 2] >> 24) ^ (p[c + 3] >> 8) ^ ((p[b + 3] >> SR1) & MSK4) ^ (p[d + 3] << SL1);
					p[a + 2] = p[a + 2] ^ (p[a + 2] << 8) ^ (p[a + 1] >> 24) ^ (p[c + 3] << 24) ^ (p[c + 2] >> 8) ^ ((p[b + 2] >> SR1) & MSK3) ^ (p[d + 2] << SL1);
					p[a + 1] = p[a + 1] ^ (p[a + 1] << 8) ^ (p[a + 0] >> 24) ^ (p[c + 2] << 24) ^ (p[c + 1] >> 8) ^ ((p[b + 1] >> SR1) & MSK2) ^ (p[d + 1] << SL1);
					p[a + 0] = p[a + 0] ^ (p[a + 0] << 8) ^ (p[c + 1] << 24) ^ (p[c + 0] >> 8) ^ ((p[b + 0] >> SR1) & MSK1) ^ (p[d + 0] << SL1);
					c = d;
					d = a;
					a += 4;
					b += 4;
					if (b >= N32)
					{
						b = 0;
					}
				}
				while (a < N32);
			}
			return sfmt[idx++];
		}

		/// <summary>
		/// 乱数テーブルを作成
		/// </summary>
		private void period_certification()
		{
			UInt32 inner = 0;
			for (int i = 0; i < 4; i++)
			{
				inner ^= sfmt[i] & PARITY[i];
			}
			for (int i = 16; i > 0; i >>= 1)
			{
				inner ^= inner >> i;
			}
			inner &= 1;
			if (inner == 1)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				UInt32 work = 1;
				for (int j = 0; j < 32; j++)
				{
					if ((work & PARITY[i]) != 0)
					{
						sfmt[i] ^= work;
						return;
					}
					work = work << 1;
				}
			}
		}
	}
	#endif
}