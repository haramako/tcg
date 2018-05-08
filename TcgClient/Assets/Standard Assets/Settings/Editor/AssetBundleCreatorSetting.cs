using UnityEngine;
using System.IO;

public partial class AssetBundleCreator
{
	static AssetBundleCreator()
	{
		RootPaths.Add("Assets/AssetBundles");
		Rules.Add(new PathRule("Assets/AssetBundles/", 1));
		AssetBundlePath = Path.Combine(Application.dataPath, "StreamingAssets");
	}
}
