using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using RSG;
using Object = UnityEngine.Object;
using UnityEngine.Profiling;

public partial class ResourceCache: MonoSingleton<ResourceCache>
{

	public class FileInfo
	{
		public string Path;
		public bool UseWww;
		public long Hash;
	}

	/// <summary>
	/// アセットバンドルから読み込む
	/// </summary>
	public class AssetBundleResource : Resource
	{
		/// <summary>
		/// ロード完了を通知するPromise
		/// </summary>
		Promise<Resource> onLoadedPromise;

		/// <summary>
		///
		/// </summary>
		AssetBundle bundle_;

#pragma warning disable 414
		Object[] assets_;
#pragma warning restore 414

		/// <summary>
		/// ロードにかかった時間[ミリ秒]
		/// </summary>
		float loadTime_;

		/// <summary>
		/// ロード開始時間(Time.realtimeSinceStartup)
		/// </summary>
		float startAt_;

		static public bool TryCreate(string filename, out Resource res)
		{
			res = new AssetBundleResource (filename);
			return true;
		}

		public static Func<string, FileInfo> LocalPathFromFile;

		public AssetBundleResource(string name) : base(name)
		{

			var abname = name.ToLowerInvariant() + ResourceCache.Extension;
			FileInfo file;
			try
			{
				file = LocalPathFromFile(abname);
			}
			catch( FileNotFoundException ex)
			{
				// ファイルが見つからなかった
				OnLoaded = Promise<Resource>.Rejected(ex);
				return;
			}

			IncRef();

			startAt_ = Time.realtimeSinceStartup;

			ResourceCache.Instance.AddTask(loadCoroutine(file));

			onLoadedPromise = new Promise<Resource>();
			OnLoaded = onLoadedPromise;
		}

		IEnumerator loadCoroutine(FileInfo file)
		{
			if (file.UseWww)
			{
				var op = WWW.LoadFromCacheOrDownload(file.Path, (int)file.Hash);

				yield return op;

				bundle_ = op.assetBundle;
			}
			else
			{
				var op = AssetBundle.LoadFromFileAsync(file.Path);

				yield return op;

				bundle_ = op.assetBundle;
			}

			yield return bundle_.LoadAllAssetsAsync();

			Profiler.BeginSample("LoadAllAssets");
			assets_ = bundle_.LoadAllAssets();
			Profiler.EndSample();

			yield return null;

			loadTime_ = Time.realtimeSinceStartup - startAt_;
			Log(string.Format("Load {0} with {1:N3} msec", Name, loadTime_ * 1000));
			DecRef();

			IsLoaded = true;
			onLoadedPromise.Resolve(this);
		}

		public override Object LoadAsset (string objname, Type type)
		{
			LastLoadTime = Time.time;
			return bundle_.LoadAsset(objname, type);
		}

		public override Object[] LoadAssetWithSubAssets(string objname)
		{
			return bundle_.LoadAssetWithSubAssets(objname);
		}

		public override void Dispose()
		{
			if (bundle_ != null) bundle_.Unload (true);
		}
	}
}
