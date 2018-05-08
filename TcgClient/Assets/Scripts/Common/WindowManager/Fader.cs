using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RSG;

/// <summary>
/// フェードアウト/フェードインの表示
/// </summary>
public class Fader : MonoBehaviour
{

	public class Option
	{
		public Color Color = Color.clear;
	}

	public Image Board;

	float alphaTo_ = 0f;
	float currentAlpha_ = 0f;
	float speed_;
	CanvasGroup canvasGroup_;

	public bool IsFading
	{
		get
		{
			return alphaTo_ != currentAlpha_;
		}
	}

	void Awake()
	{
		canvasGroup_ = gameObject.GetComponentSafe<CanvasGroup>();
	}

	void Update ()
	{
		if (alphaTo_ != currentAlpha_)
		{
			if(currentAlpha_ > alphaTo_)
			{
				currentAlpha_ = currentAlpha_ - Time.deltaTime * speed_;
				if (currentAlpha_ < alphaTo_)
				{
					currentAlpha_ = alphaTo_;
				}
			}
			else
			{
				currentAlpha_ = currentAlpha_ + Time.deltaTime * speed_;
				if (currentAlpha_ > alphaTo_)
				{
					currentAlpha_ = alphaTo_;
				}
			}
			canvasGroup_.alpha = currentAlpha_;
		}
	}

	bool locking_;

	public IPromise FadeOut(float duration = 0.5f, Option option = null)
	{
		if( option == null)
		{
			option = new Option();
		}

		//Debug.Log("FadeOut");
		if (option.Color != Color.clear)
		{
			Board.color = option.Color;
		}
		else
		{
			Board.color = Color.black;
		}
		speed_ = 1.0f / Mathf.Max(duration, 0.001f);
		alphaTo_ = 1;
		if (!locking_)
		{
			locking_ = true;
			ScreenLocker.Lock();
		}
		return (new WaitWhile(() => IsFading) as IEnumerator).AsPromise();
	}

	public IPromise FadeIn(float duration = 0.5f, Option option = null)
	{
		if (option == null)
		{
			option = new Option();
		}

		//Debug.Log("FadeIn");
		if (option.Color != Color.clear)
		{
			//Board.color = option.Color;
		}
		speed_ = 1.0f / Mathf.Max(duration, 0.001f);
		alphaTo_ = 0;
		return (new WaitWhile(() => IsFading) as IEnumerator).AsPromise().Then(() =>
		{
			if (locking_)
			{
				locking_ = false;
				ScreenLocker.Unlock();
			}
		});
	}
}
