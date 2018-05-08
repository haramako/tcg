using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Ionic.Zlib;
using System.Security.Cryptography;
using RSG;
using UnityEngine.Profiling;

namespace Cfs
{

	public enum CfsState
	{
		None = 0,
		DownloadingIndex,
		DownloadedIndex,
		DownloadingContents,
		Ready,
	}

	public class Cfs
	{
		public readonly string LocalRoot;
		public readonly Uri BaseUri;
		public readonly string[] BucketPath;
		public readonly string CryptoKey = "";//"aiRue7ooouNee0IeooneeN2eel9Aifie";
		public readonly string CryptoIv = "";//"Yee9zoogoow3Geiz";

		/// <summary>
		/// ファイル名によるフィルタ
		/// </summary>
		public Predicate<string> Filter;

		/// <summary>
		/// 現在の状態
		/// </summary>
		public CfsState State { get; private set; }

		/// <summary>
		/// バケット
		/// </summary>
		public Bucket Bucket { get; private set; }

		public bool Canceling { get; private set; }

		/// <summary>
		/// ストリーミングアセットを使うかどうか
		/// </summary>
		/// <value><c>true</c> if use streaming assets; otherwise, <c>false</c>.</value>
		public bool UseStreamingAssets { get; private set; }

		/// <summary>
		/// Web版のコンストラクタ
		/// </summary>
		/// <param name="localRoot">Local root.</param>
		/// <param name="baseUri">Base URI.</param>
		/// <param name="bucketPath">バケットのパスの配列（タグ名か、３２文字のMD5ハッシュ)</param>
		public Cfs(string localRoot, Uri baseUri, string[] bucketPath)
		{
			LocalRoot = localRoot.TrimEnd(Path.DirectorySeparatorChar);
			Bucket = new Bucket();
			BaseUri = baseUri;
			BucketPath = bucketPath;
			State = CfsState.None;
			prepare ();
		}

		/// <summary>
		/// StreamingAssetsを使用する
		/// </summary>
		/// <param name="localRoot">Local root.</param>
		/// <param name="baseUri">Base URI.</param>
		/// <param name="bucketPath">Bucket path.</param>
		public Cfs(string localRoot, string[] bucketPath)
		{
			LocalRoot = localRoot.TrimEnd(Path.DirectorySeparatorChar);
			Bucket = new Bucket();
			BaseUri = new Uri("file://" + LocalRoot + "/");
			BucketPath = bucketPath;
			UseStreamingAssets = true;
			State = CfsState.None;
			prepare ();
		}

		void prepare()
		{
			if (UseStreamingAssets) return;

			// ディレクトリを作成する
			string[] pathParts = LocalRoot.Split(Path.DirectorySeparatorChar);
			for (int i = 0; i < pathParts.Length; i++)
			{
				if (i == 0 && pathParts [i] == "") pathParts [i] = "/";
				if (i > 0) pathParts [i] = Path.Combine (pathParts [i - 1], pathParts [i]);
				if (pathParts[i] != "" && !Directory.Exists (pathParts [i])) Directory.CreateDirectory (pathParts [i]);
			}
		}

		public void LoadBucketFromStreamingAssets(string hash)
		{
			var newBucket = new Bucket (getStringFromHash (hash, ContentAttr.None), Filter);
			Bucket.Merge(newBucket);
		}

		internal void writeBucket(string hash, byte[] data)
		{
			writeFile (hash, new MemoryStream (data), ContentAttr.None);
			var newBucket = new Bucket (getStringFromHash (hash, ContentAttr.None), Filter);
			Profiler.BeginSample("Bucket.Merge");
			Bucket.Merge(newBucket);
			Profiler.EndSample();
		}

		internal void writeFile(string filename, byte[] data)
		{
			var hash = Bucket.Files [filename].Hash;
			writeFile (hash, new MemoryStream(data), ContentAttr.None);
		}

		internal Uri urlFromHash(string hash)
		{
			return new Uri(BaseUri + "data/" + hash.Substring(0, 2) + "/" + hash.Substring(2));
		}


		//========================================================================
		// Hashを使ったアクセス関数
		//========================================================================

		string localPathFromHash(string hash)
		{
			if( UseStreamingAssets )
			{
				var path = hash;
				var path2 = Path.Combine(path.Substring(0, 2), path.Substring(2));
				return Path.Combine(Path.Combine(LocalRoot, "data"), path2);
			}
			else
			{
				return Path.Combine (LocalRoot, hash);
			}
		}

		Stream getStreamFromHash(string hash, ContentAttr attr)
		{
			var stream = File.OpenRead (localPathFromHash (hash));
			return decode(stream, attr);
		}

		internal string getStringFromHash(string hash, ContentAttr attr)
		{
			using (var stream = getStreamFromHash (hash, attr))
			{
				return new StreamReader (stream).ReadToEnd ();
			}
		}

		internal void writeFile(string hash, Stream stream, ContentAttr attr)
		{
			var path = localPathFromHash (hash);

			// テンポラリファイルに書き込む
			var tmpPath = tempPath();
			using (var tmp = open (tmpPath, FileMode.Create))
			{
				var buf = new byte[8192];
				int len = 0;
				int read;
				while ((read = stream.Read (buf, 0, buf.Length)) > 0)
				{
					tmp.Write (buf, 0, read);
					len += read;
				}
				// パディングをする(AESがブロックサイズでしか復号できないため）
				if ((attr & ContentAttr.Crypted) != 0 && len % 16 != 0)
				{
					var pad = new byte[16];
					tmp.Write (pad, 0, 16 - len % 16);
				}
			}
			// アトミックにするために、Mone/Replaceを使用している
			if (File.Exists (path))
			{
				File.Replace (tmpPath, path, null);
			}
			else
			{
				File.Move (tmpPath, path);
			}
		}

		Stream decode(Stream srcStream, ContentAttr attr)
		{
			var result = srcStream;
			if ((attr & ContentAttr.Crypted) != 0 && !string.IsNullOrEmpty(CryptoKey))
			{
				var cipher = new AesManaged ();
				cipher.Padding = PaddingMode.None;
				cipher.Mode = CipherMode.CFB;
				cipher.KeySize = 256;
				cipher.BlockSize = 128;
				cipher.Key = System.Text.Encoding.UTF8.GetBytes (CryptoKey);
				cipher.IV = System.Text.Encoding.UTF8.GetBytes (CryptoIv);

				result = new CryptoStream (result, cipher.CreateDecryptor (), CryptoStreamMode.Read);
			}

			if( (attr & ContentAttr.Compressed) != 0 )
			{
				// ZLibのヘッダーを2byteを読み飛ばす
				// See: http://wiz.came.ac/blog/2009/09/zlibdll-zlibnet-deflatestream.html
				result.ReadByte ();
				result.ReadByte ();
				result = new DeflateStream (result, CompressionMode.Decompress);
			}

			return result;
		}

		string tempPath()
		{
			return Path.Combine (LocalRoot, "cfstmpfile");
		}

		//========================================================================
		// ファイルシステム層の抽象化
		//========================================================================

		Stream open(string path, FileMode mode)
		{
			if (UseStreamingAssets)
			{
				throw new NotSupportedException("cant write to streaming assets");
			}
			else
			{
				return File.Open(path, mode);
			}
		}

		bool exists(string path)
		{
			if (UseStreamingAssets)
			{
				//var path2 = Path.Combine(path.Substring(0, 2), path.Substring(2));
				//return File.Exists(Path.Combine(StreamingAssetsDataPath, path2));
				return File.Exists(path);
			}
			else
			{
				return File.Exists(path);
			}
		}

		//========================================================================
		// ファイル名を使ったアクセス関数
		//========================================================================

		public Uri UrlFromFile(string filename)
		{
			var hash = Bucket.Files [filename].Hash;
			return new Uri(BaseUri + "data/" + hash.Substring(0, 2) + "/" + hash.Substring(2));
		}

		public string LocalPathFromFile(string filename)
		{
			FileInfo file;
			if (Bucket.Files.TryGetValue (filename, out file))
			{
				return localPathFromHash(file.Hash);
			}
			else
			{
				throw new FileNotFoundException (string.Format ("<color=red><b>cfs file '{0}' is not found.</b></color>", filename));
			}
		}

		public byte[] GetBytes(string filename)
		{
			//Configure.Log ("CfsLog", "open file " + filename);
			var file = Bucket.Files [filename];
			var buf = new byte[file.OrigSize];
			using (var stream = decode (File.OpenRead (LocalPathFromFile (filename)), Bucket.Files [filename].Attr))
			{
				stream.Read (buf, 0, buf.Length);
				return buf;
			}
		}

		public Stream GetStream(string filename)
		{
			//Configure.Log ("CfsLog", "open file " + filename);
			return decode(File.OpenRead(LocalPathFromFile (filename)), Bucket.Files[filename].Attr);
		}

		public string GetString(string filename)
		{
			using (var stream = GetStream (filename))
			{
				return new StreamReader (stream).ReadToEnd ();
			}
		}

		public bool ExistsInBucket(string filename)
		{
			return Bucket.Files.ContainsKey (filename);
		}

		public bool Exists(string filename)
		{
			try
			{
				return exists(LocalPathFromFile(filename));
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// キャッシュをすべてクリアする
		/// </summary>
		/// <returns>The cache.</returns>
		public void ClearCache()
		{
			try
			{
				Directory.Delete (LocalRoot, true);
				CancelDownload();
			}
			catch(Exception ex)
			{
				Debug.LogException (ex);
			}
		}

		public void CancelDownload()
		{
			Canceling = true;
		}

		/// <summary>
		/// ファイルの状態をチェックする
		/// </summary>
		/// <param name="fullCheck">フルチェック（Hashの比較を行う）ならtrue</param>
		public CheckResult Check(bool fullCheck)
		{
			var result = new CheckResult ();

			var filelist = Directory.GetFiles (LocalRoot).Select(f => Path.GetFileName(f)).ToList();
			result.AllFileCount = filelist.Count;

			// バケットファイルを調査
			foreach (var bucketPath in BucketPath)
			{
				if (filelist.Contains(bucketPath))
				{
					result.DownloadedFileCount++;
					filelist.Remove(bucketPath);
				}

				// バケット内のファイルを調査
				foreach (var file in Bucket.Files.Values)
				{
					if (filelist.Contains(file.Hash))
					{
						result.DownloadedFileCount++;
						filelist.Remove(file.Hash);

						// fullCheckならhashを比較する
						if (fullCheck)
						{
							var hash = getFileHash(localPathFromHash(file.Hash), file.Size);
							if (hash != file.Hash)
							{
								Debug.LogError("invalid file hash '" + file.Filename + "' expect " + file.Hash + " but " + hash);
								result.ErrorCount++;
								try
								{
									File.Delete(localPathFromHash(file.Hash));
								}
								catch (Exception ex)
								{
									Debug.LogError("cannot delete error file " + file.Hash + ", " + ex);
								}
							}
						}
					}
				}
			}

			// 不要になったファイルを削除する
			foreach (var f in filelist)
			{
				try
				{
					File.Delete(Path.Combine(LocalRoot, f));
				}
				catch(Exception ex)
				{
					Debug.LogError ("cannot delete file " + f + ", " + ex);
					result.ErrorCount++;
				}
				result.RemovedFileCount++;
			}

			return result;
		}

		string getFileHash(string path, int size)
		{
			using (var f = File.OpenRead (path))
			{
				var md5 = MD5.Create ();
				var buf = new byte[size];
				f.Read (buf, 0, size); // padが入ってるかもしれないので、長さを制限する
				return BitConverter.ToString (md5.ComputeHash (buf)).ToLowerInvariant ().Replace ("-", "");
			}
		}
	}

	public class CheckResult
	{
		public int AllFileCount;
		public int DownloadedFileCount;
		public int RemovedFileCount;
		public int ErrorCount;
	}

}