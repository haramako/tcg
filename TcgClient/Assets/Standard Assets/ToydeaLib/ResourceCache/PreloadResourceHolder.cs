using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PreloadResourceHolder : MonoBehaviour
{
	static PreloadResourceHolder instance_;

	public static void Init()
	{
		var obj = new GameObject("PreloadResourceHolder");
		Object.DontDestroyOnLoad(obj);
		//obj.hideFlags = HideFlags.HideAndDontSave; // MEMO: シーンにゴミとして残って、保存されてしまう可能性があるので、保存されないようにする
		instance_ = obj.AddComponent<PreloadResourceHolder>();
	}

	public static PreloadResourceHolder Instance
	{
		get
		{
			if( instance_ == null)
			{
				Debug.LogError("ResourceCache not initialized");
			}
			return instance_;
		}
	}
}
