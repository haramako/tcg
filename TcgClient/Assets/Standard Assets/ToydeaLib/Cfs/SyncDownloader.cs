#if (UNITY_EDITOR)

	using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Linq;
using System.IO;

namespace Cfs
{
	public class SyncDownloader
	{

		public Cfs cfs_;

		public SyncDownloader(Cfs cfs)
		{
			cfs_ = cfs;
		}

		public void DownloadIndexSync()
		{
			foreach( var path in cfs_.BucketPath)
			{
				fetchSync (path, true);
				var newBucket = new Bucket(cfs_.getStringFromHash (path, ContentAttr.All));
				cfs_.Bucket.Merge(newBucket);
			}

		}

		public void DownloadSync( IEnumerable<FileInfo> files)
		{
			foreach( var file in files.Where (f => !File.Exists(cfs_.LocalPathFromFile(f.Filename))))
			{
				//Debug.Log ("downloading " + file.Filename);
				fetchSync (file.Hash, true);
			}
		}

		void fetchSync(string hash, bool padded)
		{
			var url = cfs_.BaseUri + "data/" + hash.Substring(0, 2) + "/" + hash.Substring(2);
			var request = (HttpWebRequest)WebRequest.Create (url);
			var res = request.GetResponse ();
			using (var stream = res.GetResponseStream ())
			{
				cfs_.writeFile (hash, stream, ContentAttr.All);
			}
		}

	}
}
#endif