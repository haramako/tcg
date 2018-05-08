using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using RSG;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

public partial class ResourceCache: MonoSingleton<ResourceCache>
{

	#if UNITY_EDITOR
	/// <summary>
	/// エディタのみで有効な、プロジェクトからのダイレクト読み込み
	/// 実機ではアセットバンドルとして読み込むはずのもの
	/// </summary>
	public class DirectResource : Resource
	{
		class CacheEntry
		{
			public string Name;
			public string Path;
			public Object[] Obj;
		}

		Dictionary<string, CacheEntry> cache_ = new Dictionary<string, CacheEntry>();

		static public bool TryCreate(string filename, out Resource res)
		{
			if (AssetDatabase.GetAssetPathsFromAssetBundle(filename.ToLowerInvariant() + ResourceCache.Extension).Length > 0)
			{
				res = new DirectResource(filename);
				return true;
			}
			else
			{
				res = null;
				return false;
			}
		}

		public DirectResource(string name) : base(name)
		{
			var def = new Promise<Resource>();
			OnLoaded = def;

			cache_ = AssetDatabase.GetAssetPathsFromAssetBundle(Name + Extension)
					 .Select(path => new CacheEntry
			{
				Path = path,
				Name = Path.GetFileNameWithoutExtension(path),
			}).ToDictionary(c => c.Name);

			PromiseEx.Delay(0.001f).Done(() =>
			{
				IsLoaded = true;
				def.Resolve(this);
			});
		}

		public override Object LoadAsset (string objname, Type type)
		{
			LastLoadTime = Time.time;

			CacheEntry found;
			if (!cache_.TryGetValue(objname, out found))
			{
				// アセバンの中にないよ
				return null;
			}

			if( found.Obj == null)
			{
				found.Obj = AssetDatabase.LoadAllAssetsAtPath(found.Path);
			}

			for( int i = 0; i < found.Obj.Length; i++)
			{
				if(found.Obj[i].name == objname && found.Obj[i].GetType() == type)
				{
					return found.Obj[i];
				}
			}

			return null;
		}

		public override Object[] LoadAssetWithSubAssets(string objname)
		{
			LastLoadTime = Time.time;

			CacheEntry found;
			if (!cache_.TryGetValue(objname, out found))
			{
				// アセバンの中にないよ
				return null;
			}

			if (found.Obj == null)
			{
				found.Obj = AssetDatabase.LoadAllAssetsAtPath(found.Path);
			}

			return found.Obj;
		}

		public override void Dispose()
		{
			cache_ = null;
		}
	}
	#endif
}
