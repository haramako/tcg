using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using RSG;


/// <summary>
/// 地上の全体管理
/// </summary>
public partial class Browser : MonoSingleton<Browser>
{
	public static float FadeSpeed = 1.0f;

	public interface IPageListener
	{
		void BeforeChange (PageOption opt);
		IEnumerator Fadeout(PageOption opt);
		IEnumerator Fadein(PageOption opt);
		IEnumerator BeforeLoadPage(PageOption opt);
		IEnumerator AfterLoadPage(PageOption opt);
		IEnumerator AfterActivate(PageOption opt);
	}

	public GameObject Listener;
	public IPageListener listener_;
	public GameObject PageRoot;
	public GameObject PageTop;
	public Fader Fader;
	public Camera UiCamera;

	List<PageOption> history_ = new List<PageOption>();
	GameObject currentPage_;

	protected override void Awake()
	{
		listener_ = Listener.GetComponent<IPageListener>();
		Fader.gameObject.SetActive(true);
		//GameObject.DontDestroyOnLoad(UiCamera.gameObject);
		base.Awake();
	}

	/// <summary>
	/// 新規画面に移行
	/// </summary>
	/// <param name="path">移行先path</param>
	/// <param name="isBack">戻る為の遷移か</param>
	/// <returns></returns>
	IPromise DoGoTo(PageOption opt)
	{
		//Debug.Log(Game.Marker.D("ページを移動 {0} Back={1}").Format(opt.Uri.OriginalString, opt.IsBack).Text);
		PageOption prevFrame = null;
		if (history_.Count >= 1)
		{
			prevFrame = history_[history_.Count - 1];
		}

		return StartCoroutine(goToCoroutine(prevFrame, opt, false, null)).AsPromise();
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="prev">スタック上の前のフレーム</param>
	/// <param name="post">スタック上のあとのフレーム</param>
	/// <param name="isBack">バックキーでの移動かどうか</param>
	/// <param name="isBack">バックの場合のリザルト</param>
	/// <returns></returns>
	IEnumerator goToCoroutine(PageOption prev, PageOption post, bool isBack, object result)
	{
		//Debug.Log("Begin GrondMain.changeFrame()");

		ScreenLocker.Lock();

		PageOption after; // 移動後のフレームオプション
		PageOption before; // 移動前のフレームオプション
		string path;
		if (isBack)
		{
			after = prev;
			before = post;
			path = prev.Uri.LocalPath.Substring(1); // "/"を排除
			after.IsBack = true;
			after.BackResult = result;
		}
		else
		{
			after = post;
			before = prev;
			path = post.Uri.LocalPath.Substring(1); // "/"を排除
			after.IsBack = false;
			after.BackResult = null;
		}

		// フェードアウト
		if (post.Fade)
		{
			var opt = new Fader.Option
			{
				Color = post.FadeColor,
			};
			yield return Browser.FadeOut(1.0f, opt).AsCoroutine();
		}


		if(after.DelayTime > 0f)
		{
			yield return new WaitForSeconds(after.DelayTime);
		}

		ScreenLocker.UnlockAll();
		//ScreenLocker.Lock();

		//yield return null; // TODO: 一時的に無効化

		if (!post.IsPushState)
		{
			if (currentPage_ != null)
			{
				var beforePage = currentPage_.GetComponentSafe<Page>();
				var deactivating = beforePage.OnDeactivatePage();
				if( deactivating != null)
				{
					yield return deactivating;
				}
				Destroy(currentPage_);
			}
		}

		// ヒストリの変更
		if (!isBack)
		{
			if (after.ClearHistory)
			{
				history_.Clear();
			}
			history_.Add(post);
		}
		else
		{
			history_.RemoveAt(history_.Count - 1);
		}

		listener_.BeforeChange (after);

		// SaveStateを行う
		if (currentPage_ != null)
		{
			var beforePage = currentPage_.GetComponent<Page>();
			beforePage.OnSaveState(before);
		}

		#if false // ファング表示などのカクつきを解消
		ResourceCache.ReleaseAll(0);
		#endif

		if (!post.IsPushState)
		{
			// PushState() でない場合、新しいフレームを作成
			var path2 = path.Split('/');
			GameObject obj = null;

			yield return ResourceCache
						 .Create<GameObject>("" + path2.Last())
						 .Then(o =>
			{
				obj = o;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.SetParent(PageRoot.transform, false);
			}).AsCoroutine();

			currentPage_ = obj;

			var newPage = obj.GetComponent<Page>();
			if (newPage != null)
			{
				var loading = newPage.OnLoadPage(after);
				if( loading != null)
				{
					yield return loading;
				}

				newPage.OnStartPage(after);

				//yield return listener_.BeforeLoadPage(); // TODO: 一時的に無効化

				// yield return newPage.OnLoadPage(after); // TODO: 一時的に無効化

				newPage.OnChangeState(after);

				//yield return listener_.AfterLoadPage(); // TODO: 一時的に無効化

			}

			if( obj != null)
			{
				obj.SetActive(true);
			}

			if (newPage != null)
			{
				newPage.OnActivatePage(after);
			}
		}
		else
		{
			//yield return listener_.BeforeLoadPage(); // TODO: 一時的に無効化

			// PushState() の場合
			var afterPage = currentPage_.GetComponent<Page>();
			afterPage.OnChangeState(after);

			//yield return listener_.AfterLoadPage();// TODO: 一時的に無効化
		}

		// フェードイン
		if (post.Fade && !post.NoFadein)
		{
			yield return Browser.FadeIn(1.0f).AsCoroutine();
		}

		ScreenLocker.Unlock();

		if (currentPage_ != null)
		{
			var co = currentPage_.GetComponent<Page>().AfterActivate(after);
			if (co != null)
			{
				yield return co;
			}
		}

		var afterActivate = listener_.AfterActivate(after);
		if (afterActivate != null)
		{
			yield return afterActivate;
		}

		//Debug.Log("Finish GrondMain.changePage()");
	}

	public Page GetCurrentWindow()
	{
		if (currentPage_ == null)
		{
			return null;
		}
		else
		{
			return currentPage_.GetComponent<Page>();
		}
	}

	/// <summary>
	/// 1個前のフレームに戻る
	/// </summary>
	/// <returns>戻ったか</returns>
	public bool DoBack(object result)
	{
		int count = history_.Count;
		if (history_.Count > 1)
		{
			StartCoroutine(goToCoroutine(history_[count - 2], history_[count - 1], true, result));
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// URIのQueryStringを分解する
	/// </summary>
	/// <param name="query"></param>
	/// <returns></returns>
	public static Dictionary<string, string> ParseQuery(string query)
	{
		return query
			   .Split('?', '&')
			   .Where(i => string.IsNullOrEmpty(i) == false)
			   .Select(i => i.Split('='))
			   .ToDictionary(
				   i => Uri.UnescapeDataString(i[0]),
				   i => ((i.Length > 1) ? Uri.UnescapeDataString(i[1]) : ""));
	}

	public static void PushState(string path, PageOption option = null)
	{
		if (option == null)
		{
			option = new PageOption();
		}
		option.IsPushState = true;
		GoTo(path, option);
	}

	public static IPromise GoTo(string path, PageOption option = null)
	{
		var uri = new Uri(new Uri("dfz:///"), path);
		var query = ParseQuery(uri.Query);
		if (option == null)
		{
			option = new PageOption();
		}
		option.Uri = uri;
		option.UriParam = query;

		#if UNITY_EDITOR
		// 現在のURLをStartupWindow用に保存する
		PlayerPrefs.SetString("StartupWindow.CurrentUrl", uri.ToString());
		#endif

		return Instance.DoGoTo(option);
	}

	//
	/// <summary>
	/// 特定のURLまでのヒストリーを削除する
	/// 現在のPageを除いて、末尾から探し、最初の指定されたURLを含まないヒストリーを削除する。
	/// また、指定されたURLが存在しない場合はすべてのヒストリーを削除する。
	///
	/// A => B => C => D => E
	/// から path に B を指定した場合、ヒストリーは
	/// A => B => E
	/// の状態になる。
	/// その直後に Back() を呼ぶことで、特定のURLまで戻る挙動となる
	///
	/// </summary>
	/// <param name="path"></param>
	public void RemoveHistorySincePath(string path)
	{
		if (path[0] != '/') path = "/" + path; // 先頭の/を足す

		while (true)
		{
			// すべて削除したら終わり
			if (history_.Count <= 2)
			{
				break;
			}

			var page = history_[history_.Count - 2];
			Debug.Log(page.Uri.LocalPath);
			if ( page.Uri.LocalPath == path )
			{
				// 指定されたURLまで戻ったら終わり
				break;
			}
			history_.RemoveAt(history_.Count - 2);
		}
	}

	public static void BackToPage(string path)
	{
		Instance.RemoveHistorySincePath(path);
		Back();
	}

	/// <summary>
	/// ひとつ前の画面に戻る
	/// </summary>
	public static void Back(object result = null)
	{
		Instance.DoBack(result);
	}


	public static IPromise FadeIn(float duration = 0.5f, Fader.Option opt = null)
	{
		return Instance.Fader.FadeIn(duration / FadeSpeed, opt);
	}

	public static IPromise FadeOut(float duration = 0.5f, Fader.Option opt = null)
	{
		return Instance.Fader.FadeOut(duration / FadeSpeed, opt);
	}

}
