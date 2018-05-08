using System.Collections.Generic;
using System.Linq;
using Game;

public static class EnumerableExtension
{
	/// <summary>
	/// Enumerableをランダムにシャッフルする
	/// </summary>
	/// <param name="rand">ランダムジェネレータに与えるシード</param>
	/// <returns></returns>
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, int seed)
	{
		return Shuffle(source, new RandomXS(seed));
	}

	/// <summary>
	/// Enumerableをランダムにシャッフルする
	///
	/// See: https://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle-algorithm
	/// </summary>
	/// <param name="rand">ランダムジェネレータ</param>
	/// <returns></returns>
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, RandomBase rand)
	{
		T[] elements = source.ToArray();
		// Note i > 0 to avoid final pointless iteration
		for (int i = elements.Length - 1; i > 0; i--)
		{
			// Swap element "i" with a random earlier element it (or itself)
			var swapIndex = rand.RangeInt(0, i + 1);
			T tmp = elements[i];
			elements[i] = elements[swapIndex];
			elements[swapIndex] = tmp;
		}
		// Lazily yield (avoiding aliasing issues etc)
		foreach (T element in elements)
		{
			yield return element;
		}
	}
}
