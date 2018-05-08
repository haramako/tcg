using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// リソースのリファレンスカウントを自動で増減させるBehaviour
/// </summary>
public class ResourceRefcountBehaviour : MonoBehaviour
{
	ResourceCache.Resource targetResource;

	public void SetTargetResource(ResourceCache.Resource target)
	{
		if (targetResource != null)
		{
			target.DecRef ();
		}
		targetResource = target;
		if (targetResource != null)
		{
			target.IncRef ();
		}
	}

	void OnDestroy()
	{
		if (targetResource != null)
		{
			targetResource.DecRef ();
		}
	}
}

