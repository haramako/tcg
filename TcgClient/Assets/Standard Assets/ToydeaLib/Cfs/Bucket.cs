using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Profiling;

namespace Cfs
{
	public class Bucket
	{
		public Dictionary<string, FileInfo> Files = new Dictionary<string, FileInfo>();

		/// <summary>
		/// 空のバケットを生成する
		/// </summary>
		public Bucket()
		{
		}

		/// <summary>
		/// 文字列からバケットを生成する
		/// </summary>
		/// <param name="src">バケットファイル</param>
		/// <param name="filter">
		///   ファイル名によるフィルタ（帰り値が真ならそのファイルを使用する）.
		///   nullを指定した場合はすべてのファイルを使用する
		/// </param>
		public Bucket(string src, Predicate<string> filter = null)
		{
			Profiler.BeginSample("Bucket.new");
			foreach (var line_ in src.Split('\n'))
			{
				var line = line_.Trim();
				if (line == "") continue;
				var elements = line.Split ('\t');
				var hash = elements [0];
				var filename = elements [1];
				if (filter != null && !filter (filename)) continue;
				var size = Int32.Parse(elements [2]);
				var origHash = elements [4];
				var origSize = Int32.Parse(elements [5]);
				var attr = (ContentAttr)Int32.Parse(elements [6]);
				Files [filename] = new FileInfo (hash, filename, size, origHash, origSize, attr);
			}
			Profiler.EndSample();
		}

		/// <summary>
		/// 他のBucketをマージする.
		///
		/// thisにmergedBucketの内容をマージして上書きする。
		/// 両方に同じファイル名のものが存在した場合は、meregedBucket の方に存在するものが優先される。
		/// また、ファイルの追加のみで、ファイルが消されることはない。
		/// </summary>
		/// <param name="merge">Merge.</param>
		public void Merge(Bucket mergedBucket)
		{
			foreach (var kv in mergedBucket.Files)
			{
				Files[kv.Key] = kv.Value;
			}
		}
	}

	[Flags]
	public enum ContentAttr
	{
		None = 0,
		Compressed = 1,
		Crypted = 2,
		All = 3,
	}

	public class FileInfo
	{
		public readonly string Filename;
		public readonly int Size;
		public readonly string Hash;
		public readonly int OrigSize;
		public readonly string OrigHash;
		public readonly ContentAttr Attr;
		public FileInfo(string hash, string filename, int size, string origHash, int origSize, ContentAttr attr)
		{
			Hash = hash;
			Filename = filename;
			Size = size;
			OrigHash = origHash;
			OrigSize = origSize;
			Attr = attr;
		}
	}

}
