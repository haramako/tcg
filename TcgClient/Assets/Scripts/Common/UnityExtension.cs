using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;
using System.Linq;

public static class Util
{

	/// <summary>
	/// 対象のゲームオブジェクトの子孫から、名前が完全一致するGameObjectのComponentを検索する.
	///
	/// GameObject.Find() や Tansform.Find() と違うのは、非アクティブのものや、直接の子供ではなく孫やそれ以下のものも検索できること
	/// </summary>
	/// <returns>見つかったGameObject.見つからない場合は、nullを返す</returns>
	/// <param name="obj">対象のGameObject</param>
	/// <param name="name">名前</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T FindByName<T>(this GameObject obj, string name) where T : Component
	{
		return obj
			   .EachChildren ()
			   .Where (o => (o.name == name))
			   .Select (o => o.GetComponent<T>())
			   .Where (c => (c != null))
			   .FirstOrDefault ();
	}

	/// <summary>
	/// 対象のゲームオブジェクトの子孫から、名前が完全一致するGameObjectを検索する.
	///
	/// GameObject.Find() や Tansform.Find() と違うのは、非アクティブのものや、直接の子供ではなく孫やそれ以下のものも検索できること
	/// </summary>
	/// <returns>見つかったGameObject.見つからない場合は、nullを返す</returns>
	/// <param name="obj">対象のGameObject</param>
	/// <param name="name">名前</param>
	public static GameObject FindByName(this GameObject obj, string name)
	{
		return obj
			   .EachChildren ()
			   .Where (o => (o.name == name))
			   .FirstOrDefault ();
	}

	public static IEnumerable<GameObject> EachChildren(this GameObject obj)
	{
		foreach (Transform child in obj.transform)
		{
			yield return child.gameObject;
			foreach (var x in child.gameObject.EachChildren())
			{
				yield return x;
			}
		}
	}

	/// <summary>
	/// 自分の親を名前で検索する
	/// </summary>
	/// <returns>The ancestors.</returns>
	/// <param name="obj">Object.</param>
	public static T FindAncestorByName<T>(this GameObject obj, string name) where T: Component
	{
		return obj
			   .EachAncestors ()
			   .Where (o => (o.name == name))
			   .Select (o => o.GetComponent<T>())
			   .Where (c => (c != null))
			   .FirstOrDefault ();
	}

	public static GameObject FindAncestorByName(this GameObject obj, string name)
	{
		return obj
			   .EachAncestors ()
			   .Where (o => (o.name == name))
			   .FirstOrDefault ();
	}

	/// <summary>
	/// 自分の親をたどる
	/// </summary>
	/// <returns>先祖オブジェクトのリスト（自分、親、親の親、、、の順に列挙される）</returns>
	/// <param name="includeSelf">自分を含むか</param>
	public static IEnumerable<GameObject> EachAncestors(this GameObject obj, bool includeSelf = false)
	{
		Transform cur = (includeSelf ?  obj.transform : obj.transform.parent);
		while (cur != null)
		{
			yield return cur.gameObject;
			cur = cur.parent;
		}
	}

	/// <summary>
	/// GetComponent()の安全版、コンポーネントがない場合は、例外を投げる
	/// </summary>
	public static T GetComponentSafe<T>(this GameObject obj) where T: Component
	{
		T result = obj.GetComponent<T>();
		if( result == null)
		{
			throw new NullReferenceException("Cannot find component " + typeof(T) + " from " + obj);
		}
		return result;
	}

	/// <summary>
	/// GetComponent()の安全版、コンポーネントがない場合は、例外を投げる
	/// </summary>
	public static T GetComponentSafe<T>(this MonoBehaviour obj) where T : Component
	{
		return obj.gameObject.GetComponentSafe<T>();
	}

	/// <summary>
	/// 名前で指定されたIDを取得する
	///
	/// ":"で区切られた後半部分がIDとして使用される
	/// "SomeComponent:999" という名前のオブジェクトなら、999がIDとなる。
	/// </summary>
	/// <returns>ID</returns>
	public static int GetId(this GameObject obj)
	{
		return int.Parse (obj.GetStringId ());
	}

	/// <summary>
	/// 名前で指定されたIDを取得する
	///
	/// ":"で区切られた後半部分がIDとして使用される
	/// "SomeComponent:999" という名前のオブジェクトなら、999がIDとなる。
	/// </summary>
	/// <returns>ID</returns>
	public static int GetId(this Component c)
	{
		return c.gameObject.GetId ();
	}

	/// <summary>
	/// 名前で指定されたIDを取得する
	///
	/// ":"で区切られた後半部分がIDとして使用される
	/// "SomeComponent:Hoge" という名前のオブジェクトなら、"Hoge"がIDとなる。
	/// </summary>
	/// <returns>ID</returns>
	public static string GetStringId(this GameObject obj)
	{
		return obj.EachAncestors (true).Where (o => o.name.Contains(":")).Select (o => o.name.Split (':') [1]).FirstOrDefault ();
	}

	/// <summary>
	/// 名前で指定されたIDを取得する
	///
	/// ":"で区切られた後半部分がIDとして使用される
	/// "SomeComponent:Hoge" という名前のオブジェクトなら、"Hoge"がIDとなる。
	/// </summary>
	/// <returns>ID</returns>
	public static string GetStringId(this Component c)
	{
		return c.gameObject.GetStringId ();
	}

	/// <summary>
	/// HTML形式の色の文字列から Color を取得する
	/// </summary>
	/// <param name="col"></param>
	/// <returns></returns>
	public static Color ColorFromString(string col)
	{
		if( col.Length != 6 && col.Length != 8)
		{
			throw new ArgumentException("Invalid color string " + col);
		}

		var r = Convert.ToInt32(col.Substring(0, 2), 16) / 255.0f;
		var g = Convert.ToInt32(col.Substring(2, 2), 16) / 255.0f;
		var b = Convert.ToInt32(col.Substring(4, 2), 16) / 255.0f;
		var a = 1.0f;
		if ( col.Length == 8)
		{
			a = Convert.ToInt32(col.Substring(6, 2), 16) / 255.0f;
		}
		return new Color(r, g, b, a);
	}
}

