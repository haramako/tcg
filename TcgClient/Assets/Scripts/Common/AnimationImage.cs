using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using RSG;

public class AnimationImage : MonoBehaviour
{
	public Sprite[] Sprites = new Sprite[0];
	public float Duration = 0.1f;

	//bool playing_ = true;
	Image image_ = null;
	int counter_ = 0;
	float starttime = 0;

	void Awake()
	{
		image_ = GetComponent<Image>();
		counter_ = 0;
		if (Sprites.Length > 0)
		{
			image_.sprite = Sprites[counter_];
		}
		starttime = Time.time;
	}

	void Update()
	{
		if (image_ != null)
		{
			if (Sprites.Length > 0)
			{
				if (starttime + Duration < Time.time)
				{
					counter_++;
					if (counter_ == Sprites.Length)
					{
						counter_ = 0;
					}
					image_.sprite = Sprites[counter_];
					starttime = Time.time;
				}
			}
		}
	}

}
