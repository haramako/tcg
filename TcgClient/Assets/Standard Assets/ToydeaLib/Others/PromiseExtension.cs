using UnityEngine;
using System;
using System.Collections;
#if USE_DOTWEEN
using DG.Tweening;
#endif

namespace RSG
{

	/// <summary>
	/// コルーチン実行のためのシングルトン
	/// </summary>
	class Worker : MonoBehaviour
	{
		static Worker instance_;

		public static Worker Instance
		{
			get
			{
				if (instance_ == null)
				{
					var obj = new GameObject ("<<PromiseWorker>>");
					GameObject.DontDestroyOnLoad (obj);
					instance_ = obj.AddComponent<Worker> ();
				}
				return instance_;
			}
		}
	}

	public static class PromiseEx
	{
		public static IPromise Delay(float sec)
		{
			var promise = new Promise();
			Worker.Instance.StartCoroutine (delayCoroutine(promise, sec));
			return promise;
		}

		static IEnumerator delayCoroutine(Promise promise, float sec)
		{
			yield return new WaitForSeconds (sec);
			promise.Resolve ();
		}

		/// <summary>
		/// 指定したフレームだけ待つ
		///
		/// 0を指定した場合は、1フレームも待たずに実行する
		/// </summary>
		/// <param name="frame">フレーム数</param>
		/// <returns></returns>
		public static IPromise DelayFrame(int frame)
		{
			var promise = new Promise();
			Worker.Instance.StartCoroutine (delayFrameCoroutine(promise, frame));
			return promise;
		}

		static IEnumerator delayFrameCoroutine(Promise promise, int frame)
		{
			while (frame > 0)
			{
				yield return null;
				frame--;
			}
			promise.Resolve ();
		}

		public static IPromise Delay(this IPromise promise, float sec)
		{
			return promise.Then(() => Delay(sec));
		}

		public static IPromise<T> Delay<T>(this IPromise<T> promise, float sec)
		{
			return promise.Then(x => Delay(sec).Then(() => Promise<T>.Resolved(x)));
		}

		public static IPromise DelayFrame(this IPromise promise, int frame)
		{
			return promise.Then(() => DelayFrame(frame));
		}

		public static IPromise<T> DelayFrame<T>(this IPromise<T> promise, int frame)
		{
			return promise.Then(x => DelayFrame(frame).Then(() => Promise<T>.Resolved(x)));
		}

		public static IPromise<WWW> StartWWW(string url)
		{
			var promise = new Promise<WWW> ();
			Worker.Instance.StartCoroutine (WWWToPromiseCoroutine(promise, new WWW(url)));
			return promise;
		}

		public static IPromise<WWW> StartWWW(WWW www)
		{
			var promise = new Promise<WWW> ();
			Worker.Instance.StartCoroutine (WWWToPromiseCoroutine(promise, www));
			return promise;
		}

		static IEnumerator WWWToPromiseCoroutine(Promise<WWW> promise, WWW www)
		{
			yield return www;
			if (www.error != null)
			{
				promise.Reject (new Exception (www.error));
			}
			else
			{
				promise.Resolve (www);
			}
		}

		/// <summary>
		/// 対象のYieldInstruction(コルーチン)をPromise<object>に変換する.
		///
		/// 使用例:
		///    var wait = new WaitForSeconds(3.0f);
		///    wait.AsPromise().Done(_=>{ Debug.Log("finish!"); });
		/// </summary>
		public static IPromise<WWW> AsPromise(this WWW www)
		{
			var promise = new Promise<WWW>();
			Worker.Instance.StartCoroutine(asPromiseCoroutineWWW(promise, www));
			return promise;
		}

		static IEnumerator asPromiseCoroutineWWW(Promise<WWW> promise, WWW www)
		{
			while (!www.isDone)
			{
				yield return null;
			}
			if (!string.IsNullOrEmpty(www.error))
			{
				promise.Reject(new Exception("www access failed, ur=" + www.url + ", error=" + www.error));
			}
			else
			{
				promise.Resolve(www);
			}
		}

		/// <summary>
		/// 対象のIEnumrator(コルーチン)をIPromiseに変換する.
		///
		/// コルーチンは、Promise用のワーカーオブジェクトのSingletonで実行される。
		/// この Singleton はシーン変更などで削除されないので注意すること
		///
		/// 使用例:
		///    IEnumerator someCoroutine(){
		///        yield return new WaitForSeconds(3.0f);
		///    }
		///    ...
		///    var wait = someCroutine();
		///    wait.AsPromise().Done(_=>{ Debug.Log("finish!"); });
		/// </summary>
		public static IPromise AsPromise(this IEnumerator coro)
		{
			return Worker.Instance.StartCoroutine(coro).AsPromise().Then(() => Promise.Resolved());
		}

		/// <summary>
		/// 対象のYieldInstruction(コルーチン)をPromise<object>に変換する.
		///
		/// 使用例:
		///    var wait = new WaitForSeconds(3.0f);
		///    wait.AsPromise().Done(_=>{ Debug.Log("finish!"); });
		/// </summary>
		public static IPromise AsPromise<T>(this T ao) where T: YieldInstruction
		{
			var promise = new Promise();
			Worker.Instance.StartCoroutine(asPromiseCoroutine(promise, ao));
			return promise;
		}

		static IEnumerator asPromiseCoroutine<T>(Promise promise, T ao) where T: YieldInstruction
		{
			yield return ao;
			promise.Resolve ();
		}

		public static IPromise Resolved()
		{
			var promise = new Promise();
			promise.Resolve();
			return promise;
		}

		public static IPromise<T> Resolved<T>(T val)
		{
			var promise = new Promise<T>();
			promise.Resolve(val);
			return promise;
		}

		/// <summary>
		/// PromiseをIEnumerator(コルーチン)に変換する.
		///
		/// 使用例:
		///     ...コルーチン内で...
		///     yield return SomeSlowPromise().AsCoroutine(); // Promiseが終わるまで待つ
		///
		///     // 返り値を取得する場合は別途保存する
		///     int result = 0;
		///     yield return OtherSlowPromise().Then(v=>{ result = v; }).AsCoroutine();
		/// </summary>
		public static IEnumerator AsCoroutine<T>(this IPromise<T> promise)
		{
			bool finished = false;
			promise.Done((_) => { finished = true; });
			while (!finished)
			{
				yield return null;
			}
		}

		public static IEnumerator AsCoroutine(this IPromise promise)
		{
			bool finished = false;
			promise.Done(() => { finished = true; });
			while (!finished)
			{
				yield return null;
			}
		}

		#if USE_DOTWEEN
		/// <summary>
		/// Tweenの終了を通知するPromiseを返す
		/// </summary>
		public static IPromise AsPromise(this DG.Tweening.Tween self)
		{
			var promise = new Promise();
			self.OnComplete(() => promise.Resolve());
			return promise;
		}
		#endif
	}
}
