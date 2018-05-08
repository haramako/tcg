using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AppBrowser : MonoBehaviour, Browser.IPageListener
{

	public void BeforeChange (PageOption opt)
	{
	}

	public IEnumerator Fadeout(PageOption opt)
	{
		yield return null;
	}

	public IEnumerator Fadein(PageOption opt)
	{
		yield return null;
	}
	public IEnumerator BeforeLoadPage(PageOption opt)
	{
		yield return null;
	}
	public IEnumerator AfterLoadPage(PageOption opt)
	{
		yield return null;
	}
	public IEnumerator AfterActivate(PageOption opt)
	{
		return null;
	}

	public void OnBackClick()
	{
		Browser.Back();
	}

}
