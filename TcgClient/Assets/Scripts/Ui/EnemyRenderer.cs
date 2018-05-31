using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using RSG;

public class EnemyRenderer : MonoBehaviour
{

	public Image MainImage;
	public Image ShadowImage;

	Sprite mainSprite_;
	ResourceCache.Resource res_;

	public IPromise LoadResource(Character c)
	{
		return ResourceCache.LoadBundle(string.Format("Enemy{0:D4}", c.ImageId)).Then(res =>
		{
			res.IncRef();
			res_ = res;
			return Promise.Resolved();
		});
	}

	public void Redraw(Character c)
	{
		mainSprite_ = res_.LoadAsset<Sprite>(string.Format("Enemy{0:D4}", c.ImageId));
		redraw();
	}

	public void redraw()
	{
		MainImage.sprite = mainSprite_;
		ShadowImage.sprite = mainSprite_;
	}

}
