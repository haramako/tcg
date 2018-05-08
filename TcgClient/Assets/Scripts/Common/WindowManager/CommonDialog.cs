using UnityEngine;
using RSG;

public class CommonDialog
{
	/// <summary>
	/// エラーダイアログ
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public static IPromise<int> Open(string message)
	{
		// @todo 仮コード
		Debug.LogError(message);
		return Promise<int>.Resolved(0);
	}
}
