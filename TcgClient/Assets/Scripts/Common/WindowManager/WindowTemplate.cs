using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Windowオブジェクトの親オブジェクトとなるもの.
///
/// 常に画面と同じサイズになるので、これを基準に配置していく
/// </summary>
[ExecuteInEditMode]
public class WindowTemplate : MonoBehaviour
{
	#if UNITY_EDITOR
	void OnEnable ()
	{
		if (!Application.isPlaying)
		{
			var rt = GetComponent<RectTransform>();
			var srt = new SerializedObject(rt);

			srt.Update();

			var sizeDelta = srt.FindProperty("m_SizeDelta");
			sizeDelta.vector2Value = new Vector2(1280, 720);

			srt.ApplyModifiedPropertiesWithoutUndo();
		}
	}
	#endif

	void Awake()
	{
		if (Application.isPlaying && ResourceCache.HasInstance)
		{
			try
			{
				while (transform.childCount > 0)
				{
					var obj = transform.GetChild(0).gameObject;
					ResourceCache.PreloadResource.Register(obj.name, obj);
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	// 常時シーンに白い枠組みを表示する
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.6f, 0.8f, 0.85f);

		var size = GetComponent<RectTransform>().sizeDelta;
		var bottomRight = new Vector3(transform.localPosition.x + size.x, transform.localPosition.y);
		var topRight = new Vector3(transform.localPosition.x + size.x, transform.localPosition.y + size.y);
		var topLeft = new Vector3(transform.localPosition.x, transform.localPosition.y + size.y);

		// 線引き図
		// * -----> *
		// ^        ^
		// |        |
		// * -----> *
		Gizmos.DrawLine (transform.localPosition, bottomRight);
		Gizmos.DrawLine (topLeft, topRight);
		Gizmos.DrawLine (transform.localPosition, topLeft);
		Gizmos.DrawLine (bottomRight, topRight);
	}
}