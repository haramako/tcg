using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using Cfs;

public class CfsTest
{

	//[Test]
	//public void NewPlayModeTestSimplePasses() {
	//}

	[UnityTest]
	public IEnumerator TestDownload()
	{
		var root = Path.Combine(Application.persistentDataPath, "Cfs");
		Directory.Delete(root, true);

		var cfs = new Cfs.Cfs(
			root,
			new System.Uri("file://" + Application.streamingAssetsPath + "/cfs/"),
			new string[] { "tag/toydealib-test" });

		var manObj = new GameObject("CfsManager");
		var man = manObj.AddComponent<Downloader>();

		man.Init(cfs);

		bool finished = false;
		man.DownloadIndex(() => finished = true);
		yield return new WaitUntil(() => finished);

		finished = false;
		man.Download(cfs.Bucket.Files.Keys, () => finished = true);
		yield return new WaitUntil(() => finished);

		Assert.AreEqual( "hoge\n", cfs.GetString("hoge.txt"));

		Assert.AreEqual( "fuga\n", cfs.GetString("fuga.txt"));

		Assert.AreEqual( "", cfs.GetString("empty.txt"));

		Object.Destroy(manObj);
	}

	[UnityTest]
	public IEnumerator TestUseSreamingAssets()
	{
		var cfs = new Cfs.Cfs(
			Path.Combine(Application.streamingAssetsPath, "Cfs"),
			new string[] { "tag/toydealib-test" });

		var manObj = new GameObject("CfsManager");
		var man = manObj.AddComponent<Downloader>();

		man.Init(cfs);

		bool finished = false;
		man.DownloadIndex(() => finished = true);
		yield return new WaitUntil(() => finished);

		finished = false;
		man.Download(cfs.Bucket.Files.Keys, () => finished = true);
		yield return new WaitUntil(() => finished);

		Assert.AreEqual( "hoge\n", cfs.GetString("hoge.txt"));

		Assert.AreEqual( "fuga\n", cfs.GetString("fuga.txt"));

		Assert.AreEqual( "", cfs.GetString("empty.txt"));

		Object.Destroy(manObj);
	}
}
