using UnityEngine;
using System.Collections;
using System;
using RSG;

public class AnimationExtension : MonoBehaviour
{
	[Tooltip("アニメーションが終わったら削除する")] // NO-TRANSLATE
	public bool DestroyOnFinish = false;

	public string[] SoundNames = new string[0];

	bool playing_ = true;

	Animation animation_;

	[NonSerialized]
	[HideInInspector]
	public UnityEngine.Events.UnityEvent OnFinished = new UnityEngine.Events.UnityEvent ();

	public IPromise Finished()
	{
		playing_ = true;
		var promise = new Promise();
		UnityEngine.Events.UnityAction onFinishEvent = () =>
		{
			promise.Resolve();
			OnFinished.RemoveAllListeners();
		};
		OnFinished.AddListener(onFinishEvent);
		return promise;
	}

	void Awake()
	{
		animation_ = GetComponent<Animation>();
	}

	void Update()
	{
		if (animation_ != null)
		{
			if (!(animation_.isPlaying))
			{
				if (playing_)
				{
					playing_ = false;
					OnFinished.Invoke();
				}
				if (DestroyOnFinish)
				{
					Destroy(gameObject);
				}
			}
		}
	}

	public void PlaySound(int n)
	{
		if (n >= SoundNames.Length)
		{
			throw new IndexOutOfRangeException(string.Format("アニメーション {0} の音番号{1}は範囲外です", gameObject.name, n));
		}
		//AudioPlay.SeWithAssetBundleName(SoundNames[n]);
	}

	public void PlaySound0() { PlaySound(0); }
	public void PlaySound1() { PlaySound(1); }
	public void PlaySound2() { PlaySound(2); }
	public void PlaySound3() { PlaySound(3); }
	public void PlaySound4() { PlaySound(4); }
	public void PlaySound5() { PlaySound(5); }
	public void FinishEvent() {	playing_ = false; OnFinished.Invoke(); }

}
