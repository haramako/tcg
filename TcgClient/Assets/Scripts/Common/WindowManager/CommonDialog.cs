using UnityEngine;
using UnityEngine.UI;
using RSG;

public class CommonDialog : MonoBehaviour
{
	public Text MessageText;
	public Button[] Buttons;

	Promise<int> onClose_ = new Promise<int>();

	/// <summary>
	/// エラーダイアログ
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public static IPromise<int> Open(string message)
	{
		return ResourceCache.Create<GameObject>("CommonDialog")
			   .WithScreenLock()
			   .Then(obj =>
		{
			var dialog = obj.GetComponent<CommonDialog>();
			dialog.MessageText.text = message;
			ScreenLocker.Modal(obj);
			return (IPromise<int>)dialog.onClose_;
		});
	}

	public void OnButonClick(GameObject target)
	{
		var id = target.GetId();
		onClose_.Resolve(id);
		ScreenLocker.Unmodal(gameObject);
	}
}
