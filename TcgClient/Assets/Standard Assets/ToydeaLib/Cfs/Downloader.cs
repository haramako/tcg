using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Cfs;
using UnityEngine.Profiling;

namespace Cfs
{
	public class Downloader : MonoSingleton<Downloader>
	{
		public static bool LogEnabled = true;

		public enum CfsState
		{
			None = 0,
			DownloadingIndex,
			DownloadedIndex,
			DownloadingContents,
			Ready,
		}

		[System.Diagnostics.Conditional("DEBUG")]
		static void log(object msg)
		{
			if (LogEnabled)
			{
				Debug.Log(msg);
			}
		}

		public CfsState State { get; private set; }
		public Cfs Cfs { get; private set; }

		public void Init(Cfs cfs)
		{
			Cfs = cfs;
			State = CfsState.None;
		}

		public YieldInstruction DownloadIndex(Action callback)
		{
			return StartCoroutine (downloadIndexCoroutine(callback));
		}

		IEnumerator downloadIndexCoroutine(Action callback)
		{
			log ("Cfs: start download index");
			if (State != CfsState.None)
			{
				throw new InvalidOperationException ("Invalid state " + State);
			}

			State = CfsState.DownloadingIndex;

			// タグをハッシュに変更する
			var bucketHashes = new List<string>();
			foreach (var bucketPath in Cfs.BucketPath)
			{
				if (bucketPath.Length != 32)
				{
					Debug.Log(Cfs.BaseUri);
					var indexWww = new WWW((Cfs.BaseUri + bucketPath).ToString());
					yield return indexWww;
					if (indexWww.error != null)
					{
						Debug.LogError("Cfs: error" + indexWww.error + " on " + indexWww.url );
						yield break;
					}
					bucketHashes.Add(indexWww.text.Trim());
				}
				else
				{
					bucketHashes.Add(bucketPath);
				}
			}

			if (Cfs.UseStreamingAssets)
			{
				Cfs.LoadBucketFromStreamingAssets(bucketHashes[0]);
				State = CfsState.DownloadedIndex;
			}
			else
			{
				foreach (var hash in bucketHashes)
				{
					var www = new WWW(Cfs.urlFromHash(hash).ToString());
					yield return www;
					if (www.error != null)
					{
						Debug.LogError("Cfs: error " + www.error);
					}
					Cfs.writeBucket(hash, www.bytes);
				}

				foreach (var bucketPath in Cfs.BucketPath)
				{
					string hash;
					if (bucketPath.Length != 32)
					{
						var indexWww = new WWW((Cfs.BaseUri + bucketPath).ToString());
						yield return indexWww;
						if (indexWww.error != null)
						{
							Debug.LogError("Cfs: error " + indexWww.error + " on " + indexWww.url);
							yield break;
						}
						hash = indexWww.text.Trim();
					}
					else
					{
						hash = bucketPath;
					}

					var www = new WWW(Cfs.urlFromHash(hash).ToString());
					yield return www;
					if (www.error != null)
					{
						Debug.LogError("Cfs: error" + www.error);
					}
					Cfs.writeBucket(hash, www.bytes);
				}

			}
			State = CfsState.DownloadedIndex;
			callback();
		}

		/// <summary>
		/// ファイルをダウンロードする
		/// </summary>
		/// <param name="files">Files.</param>
		public void Download( IEnumerable<string> files, Action callback = null, Action<FileInfo> callbackEveryFile = null )
		{
			if( Cfs.UseStreamingAssets )
			{
				callback();
				return;
			}

			log ("start downloading ");

			var stat = new DownloadingStat
			{
				Callback = callback,
				CallbackEveryFile = callbackEveryFile,
			};

			foreach ( var file_ in files )
			{
				var file = file_;
				stat.All++;
				Profiler.BeginSample("Downloader.Download: Check file exists");
				if (Cfs.Exists (file))
				{
					stat.Cached++;
					continue;
				}
				Profiler.EndSample();
				var url = Cfs.UrlFromFile(file).ToString();
				stat.Download++;

				tasks_.Enqueue(downloadCo(url, file, stat));
			}
			for (int i = 0; i < 1; i++)
			{
				StartCoroutine(worker());
			}

			StartCoroutine(finisher(stat));
		}

		IEnumerator finisher(DownloadingStat stat)
		{
			yield return new WaitWhile(() => stat.Downloaded < stat.Download);
			log("Finish download");
			stat.Callback();
		}

		Queue<IEnumerator> tasks_ = new Queue<IEnumerator>();

		IEnumerator worker()
		{
			while (tasks_.Count() > 0)
			{
				var task = tasks_.Dequeue();
				yield return task;
			}
		}

		internal class DownloadingStat
		{
			internal int All;
			internal int Downloaded;
			internal int Download;
			internal int Cached;
			internal Action Callback;
			internal Action<FileInfo> CallbackEveryFile;
		}

		IEnumerator downloadCo(string url, string file, DownloadingStat stat)
		{
			WWW www;
			while (true)
			{
				int retryCount = 0;
				www = new WWW (url);
				yield return www;
				if (www.error != null)
				{
					if (retryCount >= 3)
					{
						break;
					}
					else
					{
						yield return new WaitForSeconds (1.0f);
						www.Dispose ();
						continue;
					}
				}
				else
				{
					break;
				}
			}

			if( www.error != null )
			{
				Debug.LogError ("error downloading" + file + " from " + url + " ," + www.error);
				www.Dispose();
				yield break;
			}

			log ("finish downloading " + file + " from " + url);
			Profiler.BeginSample("Cfs.Downloader: write");
			Cfs.writeFile (file, www.bytes);
			Profiler.EndSample();

			if( stat.CallbackEveryFile != null )
			{
				stat.CallbackEveryFile(Cfs.Bucket.Files[file]);
			}

			stat.Downloaded++;

			www.Dispose ();
		}
	}
}
