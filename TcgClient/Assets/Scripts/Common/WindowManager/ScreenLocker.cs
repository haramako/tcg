using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using RSG;
using UnityEngine.EventSystems;

/// <summary>
/// モーダルな表示を行うためのクラス
///
/// 使い方
/// <pre>
///
/// // 画面全体の操作を無効にする
/// ScreenLocker.Lock();
/// ...画面ロック中の処理...
/// ScreenLocker.Unlock();
///
///
/// // 特定のオブジェクトを全面に表示する
/// var modalToken = ScreenLocker.Modal(gameObject);
/// ...画面ロック中の処理...
/// ScreenLocker.Unmocal(modalToken);
///
/// </pre>
///
/// <h2>バックキーの制御</h2>
///
/// バックキーの制御は、下記の優先順位で行われる。
/// (高)
/// 1. 「完全ロック」中は、BackKeyが効かないようになる
/// 2. IModalEventHandler を実装している モーダル表示中オブジェクトの一番上のもの  ( ScreenLocker.Lock() により管理される )
/// 3. 登録された BackKeyハンドラ の最後に登録されたもの (PushBackKeyHandler()/ PopBackKeyHandler() により管理される）
/// (低)
///
/// であるので、モーダルなダイアログに関しては、IModalEventHandler を実装し、
/// それ以外は、PushBackKeyHandler()/PopBackKeyHandler() により BackKeyハンドラ を管理してください。
///
/// IModalEventHandler の実装については、 CommonDialog を参考にしてください。
/// BackKeyHandler の実装については、GameBaseMain.OnBackKeyPushed() を参考にしてください。
/// もしくは、GameBaseMainを継承しているものなら、OnBackKeyPushed()を override するだけで対応可能です。
///
/// </summary>
public class ScreenLocker : MonoSingleton<ScreenLocker>
{
	/// <summary>
	/// ログを有効化
	/// </summary>
	public static bool LogEnabled;

	public interface IModalEventHandler : IEventSystemHandler
	{
		void OnModalOpen();
		void OnModalClose();
		void OnModalBackKeyPushed();
	}

	public class Option
	{
		public bool FullModal;
		public Color FilterColor;
		public bool Animation;
		public Transform PlacementPlannedSite;  // 配置予定地

		public static Option None { get { return new Option() { FullModal = true, FilterColor = Color.clear }; } }
		public static Option Modal { get { return new Option() { FilterColor = new Color(0, 0, 0, 0.75f) }; } }
	}

	public class ModalInfo
	{
		public Option Option;
		public GameObject Obj;
		public GameObject Target;
		public Promise<ModalInfo> OnClose;
		public object Result;
	}

	/// <summary>
	/// トップウィンドウ
	/// </summary>
	public GameObject WindowTop;

	// ロック中の数
	int lockCount_;

	// 生成したモーダル
	Stack<ModalInfo> modals_ = new Stack<ModalInfo>();

	// モーダルの数
	public int ModelCount { get { return modals_.Count; } }

	// バックキーハンドラー
	List<Action> backKeyHandlers_ = new List<Action>();

	// グローバルなイベントシステム
	EventSystem eventSystem_;

	protected override void Awake()
	{
		base.Awake();
		eventSystem_ = GameObject.FindObjectOfType<EventSystem>();
	}

	// モーダル表示を行う
	public void DoLock()
	{
		if (lockCount_ <= 0)
		{
			eventSystem_.enabled = false;
		}
		lockCount_++;
	}

	// モーダル表示を行う
	public void DoUnlock()
	{
		lockCount_--;
		if(lockCount_ <= 0)
		{
			eventSystem_.enabled = true;
		}
	}

	// モーダル表示を行う
	public RSG.IPromise<ModalInfo> DoModal(GameObject target, Option opt)
	{
		GameObject parent;
		if (opt.FullModal )
		{
			parent = WindowTop;
		}
		else
		{
			if( WindowTop != null )
			{
				parent = WindowTop;
			}
			else
			{
				parent = gameObject;
			}
		}

		var obj = new GameObject("ScreenBarrier");
		var image = obj.AddComponent<Image>();
		var rect = obj.GetComponent<RectTransform>();
		image.color = opt.FilterColor;
		rect.sizeDelta = new Vector2(9999, 9999);
		obj.transform.SetParent(parent.transform, false);
		obj.transform.SetAsLastSibling();
		var modalInfo = new ModalInfo()
		{
			Obj = obj,
			Target = target,
			Option = opt,
			OnClose = new Promise<ModalInfo>(),
		};
		modals_.Push(modalInfo);

		if (target != null)
		{
			target.transform.SetParent(parent.transform, false);
			if (opt.PlacementPlannedSite != null)
			{
				target.transform.position = opt.PlacementPlannedSite.position;
			}
			else
			{
				target.transform.localPosition = Vector3.zero;
			}
			target.transform.SetAsLastSibling();
			target.SetActive(true);

			var handler = target.GetComponent<IModalEventHandler>();
			if (handler != null)
			{
				handler.OnModalOpen();
			}

		}

		return modalInfo.OnClose;
	}

	// モーダル表示を解除する
	public void DoUnmodal(GameObject target = null)
	{
		//Debug.Log("Unmodal " + target);
		if (modals_.Count > 0)
		{
			//HidController.Instance.ClearButtonPushed();

			var modal = modals_.Pop();
			if( modal.Target != null)
			{
				if(target != null && modal.Target != target)
				{
					Debug.LogWarning("Invalid Unmodal " + modal.Target + " " + target);
				}

				var handler = modal.Target.GetComponent<IModalEventHandler>();
				if( handler != null)
				{
					handler.OnModalClose();
				}
				GameObject.Destroy(modal.Target);
			}
			modal.OnClose.Resolve(modal);
			GameObject.Destroy(modal.Obj);

			if( modals_.Count == 0)
			{
			}
		}
		else
		{
			// TODO:一時的にコメントアウトしてる
			Debug.LogError("too much ScreenLocker.Unlock() called.");
		}
	}

	public void DoUnlockAll()
	{
		//lockCount_ = 0;
		//eventSystem_.enabled = true;

		while (modals_.Count > 0)
		{
			var locker = modals_.Pop ();
			if (locker.Target != null)
			{
				GameObject.Destroy (locker.Target);
			}
			GameObject.Destroy (locker.Obj);
		}
	}

	void doPushBackKeyHandler(Action callback)
	{
		backKeyHandlers_.Add(callback);
	}

	void doPopBackKeyHandler(Action callback)
	{
		if (backKeyHandlers_.Count <= 0)
		{
			throw new InvalidOperationException ("too many pops back key handler");
		}
		else
		{
			if (!backKeyHandlers_.Remove(callback))
			{
				Debug.LogError("not match back key handler");
			}
		}
	}

	void Update()
	{
		if (eventSystem_ == null)
		{
			eventSystem_ = GameObject.FindObjectOfType<EventSystem>();
		}

		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			if (IsLocked())
			{
				// 完全ロック中は反応しない
			}
			else if ( modals_.Count > 0)
			{
				// バックキーが押されたことを通知する
				var locker = modals_.Peek();
				if (locker.Target != null)
				{
					var handler = locker.Target.GetComponent<IModalEventHandler>();
					if (handler != null)
					{
						handler.OnModalBackKeyPushed();
					}
				}
			}
			else if (backKeyHandlers_.Count > 0)
			{
				// それ以外のバックキー対応
				backKeyHandlers_[backKeyHandlers_.Count - 1]();
			}
			else
			{
				Debug.Log("no back key handler registered in ScreenLocker");
			}
		}
	}

	public bool IsLocked()
	{
		if (lockCount_ > 0)
		{
			return true;
		}
		else if (modals_.Count > 0)
		{
			return modals_.Peek().Target == null;
		}
		else
		{
			return false;
		}

	}

	public static void Lock()
	{
		if( LogEnabled )
		{
			Debug.Log("ScreenLocker +Lock" + +Instance.lockCount_);
		}
		Instance.DoLock();
	}

	public static void Unlock()
	{
		if( LogEnabled)
		{
			Debug.Log("ScreenLocker -Unlock " + (Instance.lockCount_ - 1));
		}
		Instance.DoUnlock();
	}

	public static RSG.IPromise<ModalInfo> Modal(GameObject obj = null, Option opt = null)
	{
		if (opt == null)
		{
			if (obj == null)
			{
				opt = Option.None;
			}
			else
			{
				opt = Option.Modal;
			}
		}
		return Instance.DoModal(obj, opt);
	}

	public static void Unmodal(GameObject target = null)
	{
		Instance.DoUnmodal(target);
	}

	public static void UnlockAll()
	{
		Instance.DoUnlockAll();
	}

	/// <summary>
	/// タップされるまで待つ.
	///
	/// 例：
	/// ScreenLocker.WaitForTap().Done(_ => { Debug.Log("Closed!"); });
	///
	/// </summary>
	/// <returns>モーダル情報</returns>
	public static RSG.IPromise<ModalInfo> WaitForTap()
	{
		return ResourceCache.Create<GameObject>("system/WaitForTap").Then(obj => ScreenLocker.Modal(obj, Option.None));
	}

	/// <summary>
	/// バックキーのハンドラの登録を行う.
	/// </summary>
	/// <param name="callback">バックキーが押されたときに呼ばれるコールバック</param>
	public static void PushBackKeyHandler(Action callback)
	{
		Instance.doPushBackKeyHandler(callback);
	}


	/// <summary>
	/// バックキーのハンドラの解除を行う.
	///
	/// 現在アクティブなハンドラじゃなくても解除できるように、どのハンドラを解除するか指定する必要がある
	/// </summary>
	/// <param name="callback">PushBackKeyHandler()で登録したコールバック</param>
	public static void PopBackKeyHandler(Action callback)
	{
		Instance.doPopBackKeyHandler(callback);
	}

}

public static class ScreenLockerPromiseExtension
{
	public static IPromise WithScreenLock(this IPromise promise)
	{
		ScreenLocker.Lock();
		return promise.Then(() => ScreenLocker.Unlock());
	}

	public static IPromise<T> WithScreenLock<T>(this IPromise<T> promise)
	{
		ScreenLocker.Lock();
		return promise.Then(x =>
		{
			ScreenLocker.Unlock(); return x;
		});
	}
}

