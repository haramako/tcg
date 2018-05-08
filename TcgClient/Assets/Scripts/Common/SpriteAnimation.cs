using UnityEngine;
using System.Collections;

public class SpriteAnimation : MonoBehaviour
{

	public Sprite[] Sprites;
	public float AnimationSpeed = 1;
	public float InitialAnimation;

	float curTime;

	// Use this for initialization
	void Start ()
	{
		curTime = InitialAnimation;
	}

	// Update is called once per frame
	void Update ()
	{
		curTime += Time.deltaTime * AnimationSpeed * Sprites.Length;
		var anim = ((int)curTime) % Sprites.Length;
		GetComponent<SpriteRenderer> ().sprite = Sprites [anim];
	}
}
