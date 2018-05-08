using UnityEngine;
using System.IO;

public partial class AssetBundleCreator
{
	static AssetBundleCreator()
	{
		RootPaths.Add("Assets/Gardens");
		Rules.Add(new PathRule("Assets/Gardens/", 1));
		AssetBundlePath = Path.Combine(Application.dataPath, "StreamingAssets");
	}
}
