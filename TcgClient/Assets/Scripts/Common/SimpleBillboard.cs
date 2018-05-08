using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SimpleBillboard : MonoBehaviour
{

	Quaternion backupQuaternion;

	public void OnWillRenderObject()
	{
		var camera = Camera.current;
		if (camera == null) return;
		backupQuaternion = transform.localRotation;

		var cameraForward = camera.transform.TransformVector (new Vector3 (0, 0, -1));
		var cameraUp = camera.transform.TransformVector (new Vector3 (0, 1, 0));
		transform.LookAt (transform.position - cameraForward, cameraUp);
	}

	public void OnPostRender()
	{
		transform.localRotation = backupQuaternion;
	}

}
