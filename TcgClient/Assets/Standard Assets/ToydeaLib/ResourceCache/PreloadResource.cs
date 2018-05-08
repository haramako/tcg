using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSG;
using System;

using Object = UnityEngine.Object;

public partial class ResourceCache: MonoSingleton<ResourceCache>
{

	/// <summary>
	/// アセットバンドルから読み込む
	/// </summary>
	public class PreloadResource : Resource
	{
		public GameObject Obj;
		public static Dictionary<string, PreloadResource> cache_ = new Dictionary<string, PreloadResource>();

		static public bool TryCreate(string filename, out Resource res)
		{
			PreloadResource preloadResource;
			if( cache_.TryGetValue(filename, out preloadResource))
			{
				res = preloadResource;
				return true;
			}
			else
			{
				res = null;
				return false;
			}
		}

		public PreloadResource(string name, GameObject obj) : base(name)
		{
			Obj = obj;
			OnLoaded = RSG.PromiseEx.Resolved((Resource)this);
			IncRef();
		}

		public static void Register(string name, GameObject obj)
		{
			// まだ準備ができていないので帰る
			if(PreloadResourceHolder.Instance == null)
			{
				return;
			}

			if( obj.name.StartsWith("_"))
			{
				// _ではじまるものは登録しない
				obj.transform.SetParent(null, false);
				Destroy(obj);
				return;
			}

			var lowerName = name.ToLowerInvariant();
			if (cache_.ContainsKey(lowerName))
			{
				// すでに同じ名前のものが登録されている
				Debug.LogError("Already register " + obj.name);
				obj.transform.SetParent(null, false);
				Destroy(obj);
				return;
			}

			Log("PreloadResource " + name + " registered");
			cache_.Add(lowerName, new PreloadResource(name, obj));
			obj.transform.SetParent(PreloadResourceHolder.Instance.gameObject.transform, false);
			obj.SetActive(false);
		}

		public override Object LoadAsset (string objname, Type type)
		{
			LastLoadTime = Time.time;
			return Obj;
		}

		public override void Dispose()
		{
			if (Obj != null)
			{
				cache_.Remove(this.Name);
				GameObject.Destroy(Obj);
				Obj = null;
			}
		}
	}

}
