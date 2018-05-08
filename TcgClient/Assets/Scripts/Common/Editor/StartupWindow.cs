using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class StartupWindow : EditorWindow
{


	StartupWindowSetting setting_;

	void OnEnable()
	{
		setting_ = AssetDatabase.LoadAssetAtPath<StartupWindowSetting> ("Assets/StartupWindow.asset");
		if (setting_ == null)
		{
			setting_ = ScriptableObject.CreateInstance<StartupWindowSetting> ();
			AssetDatabase.CreateAsset (setting_, "Assets/StartupWindow.asset");
		}
	}

	void OnGUI()
	{
		StartupWindowSetting s = setting_;

		GUILayout.BeginVertical ();

		s.Enable = GUILayout.Toggle (s.Enable, "有効");
		s.Url = GUILayout.TextField (s.Url);

		if (s.Enable)
		{
			UrlSchemeReceiver.SetUrlInEditor (s.Url);
			EditorUtility.SetDirty (s);
		}
		else
		{
			UrlSchemeReceiver.SetUrlInEditor ("");
			EditorUtility.SetDirty (s);
		}

		GUILayout.EndVertical ();
	}

	[MenuItem("Tools/スタートアップウィンドウ")]
	public static void Open()
	{
		EditorWindow.GetWindow<StartupWindow> ();
	}

}
