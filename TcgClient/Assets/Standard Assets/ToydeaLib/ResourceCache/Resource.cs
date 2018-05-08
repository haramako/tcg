using UnityEngine;
using System.Collections.Generic;
using System;
using RSG;

using Object = UnityEngine.Object;

public partial class ResourceCache : MonoSingleton<ResourceCache>
{
	public abstract class Resource : IDisposable
	{
		/// <summary>
		/// 名前（アセバンのファイル名から拡張子をぬいたもの）
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// リファレンスカウントの数
		/// </summary>
		public int RefCount { get; private set; }

		/// <summary>
		/// 読み込みが完了しているかどうか
		/// </summary>
		public bool IsLoaded { get; protected set; }

		/// <summary>
		/// 最後に使用された時刻(UnityEngine.Time.timeの値)
		/// </summary>
		public float LastLoadTime;

		/// <summary>
		/// 読み込みが終わったことを通知するPromise
		/// </summary>
		public IPromise<Resource> OnLoaded;

		public abstract Object LoadAsset(string obj, Type type);

		public T LoadAsset<T>(string obj) where T : Object
		{
			return (T)LoadAsset(obj, typeof(T));
		}

		public virtual Object[] LoadAssetWithSubAssets(string objname)
		{
			return null;
		}

		public abstract void Dispose();

		protected Resource(string name)
		{
			Name = name;
		}

		// リファレンスカウントを増やす
		public void IncRef()
		{
			RefCount++;
			//ResourceCache.Log ("Increment RefCount " + Name + " to " + RefCount);
		}

		// リファレンスカウントを減らす
		public void DecRef()
		{
			RefCount--;
			//ResourceCache.Log ("Decrement RefCount " + Name + " to " + RefCount);
			if (RefCount < 0)
			{
				ResourceCache.Log("Invalid RefCount " + Name);
			}
		}
	}
}
