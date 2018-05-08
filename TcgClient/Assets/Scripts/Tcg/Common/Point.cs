using System;
#if UNITY_5_5_OR_NEWER
using UnityEngine;
#endif

namespace Game
{
	/// <summary>
	/// 点を表すクラス.
	///
	/// Yは正方向が北、負方向が南、Xは正方向が東、負方向が西となる
	/// </summary>
	[Serializable]
	public struct Point : IEquatable<Point>
	{
		public const int InvalidValue = -9999;

		// MEMO:
		// JSONUtilityでJSON化できるように、自動プロパティをつかわない。
		// また、読みやすいように、命名規則に反して "_" を末尾につけない。
		#if UNITY_5_5_OR_NEWER
		[SerializeField]
		#endif
		int x;

		#if UNITY_5_5_OR_NEWER
		[SerializeField]
		#endif
		int y;

		/// <summary>
		/// X座標
		/// </summary>
		public int X { get { return x;} }

		/// <summary>
		/// Y座標
		/// </summary>
		/// <value>The y.</value>
		public int Y { get { return y; }}

		public Point(int x_, int y_)
		{
			x = x_;
			y = y_;
		}

		public Point(Point src)
		{
			x = src.x;
			y = src.y;
		}

		/// <summary>
		/// 無効な値を表す点.
		/// </summary>
		public static Point None
		{
			get
			{
				return new Point(InvalidValue, InvalidValue);
			}
		}

		public static Point operator + (Point a, Point b)
		{
			return new Point (a.x + b.x, a.y + b.y);
		}

		public static Point operator - (Point a, Point b)
		{
			return new Point (a.x - b.x, a.y - b.y);
		}

		public static Point operator + (Point a, Direction d)
		{
			return a + d.ToPos();
		}

		public static Point operator - (Point a, Direction d)
		{
			return a - d.ToPos();
		}

		public static bool operator == (Point a, Point b)
		{
			return a.Equals(b);
		}
		public static bool operator != (Point a, Point b)
		{
			return !a.Equals(b);
		}

		public bool Equals (Point a)
		{
			return x == a.x && y == a.y;
		}
		public bool Equals (int x_, int y_)
		{
			return x == x_ && y == y_;
		}
		public override bool Equals (object obj)
		{
			if (obj == null || !(obj is Point))
			{
				return false;
			}
			Point p = (Point)obj;
			return p.x == x && p.y == y;
		}

		public bool IsOrigin
		{
			get { return (x == 0 && y == 0); }
		}

		public bool IsNone
		{
			get { return (x == InvalidValue || y == InvalidValue); }
		}

		public bool IsValid
		{
			get { return (x != InvalidValue && y != InvalidValue); }
		}


		public override int GetHashCode ()
		{
			return x * 1000 + y;
		}

		public override string ToString ()
		{
			return string.Format("({0},{1})", x, y);
		}

		static Direction[] pos2dir = new Direction[]
		{
			Direction.SouthWest, Direction.South, Direction.SouthEast,
			Direction.West, Direction.None, Direction.East,
			Direction.NorthWest, Direction.North, Direction.NorthEast,
		};

		/// <summary>
		/// Directionに変換する.
		///
		/// 距離が１でない場合は、Noneを返す
		/// </summary>
		public Direction ToDir ()
		{
			if (y >= -1 && y <= 1 && x >= -1 && x <= 1)
			{
				return pos2dir [(y + 1) * 3 + x + 1];
			}
			return Direction.None;
		}

		/// <summary>
		/// pへの方向を返す
		/// </summary>
		public Direction ToDir(Point p)
		{
			return (p - this).Normalized().ToDir();
		}

		#if UNITY_5_5_OR_NEWER
		public UnityEngine.Vector2 ToVector2()
		{
			return new UnityEngine.Vector2(x, y);
		}
		#endif

		public float Length ()
		{
			return (float)System.Math.Sqrt(x * x + y * y);
		}

		/// <summary>
		/// 縦横斜めの移動で測った距離.
		///
		/// 斜めへの移動は、１として換算される。
		/// </summary>
		/// <returns>長さ</returns>
		public int GridLength ()
		{
			return Math.Max(Math.Abs(x), Math.Abs(y));
		}

		/// <summary>
		/// -1〜1の間に正規化する.
		///
		/// X,Yそれぞれに、0なら0、正の値なら1, 負の値なら-1 と正規化する
		/// </summary>
		public Point Normalized ()
		{
			var x_ = (x == 0) ? 0 : (x > 0 ? 1 : -1);
			var y_ = (y == 0) ? 0 : (y > 0 ? 1 : -1);
			return new Point(x_, y_);
		}
	}

}

