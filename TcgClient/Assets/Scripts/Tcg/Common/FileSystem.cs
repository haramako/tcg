using System.Linq;
using System.Collections.Generic;
using System;

namespace Game
{
	/// <summary>
	/// ファイルの読み書きを抽象化するクラス
	/// </summary>
	public interface IFileSystem
	{
		IEnumerable<string> GetFiles();
		System.IO.Stream OpenRead(string path);
		byte[] ReadAllBytes(string path);
		bool Exists(string path);
	}

	public static class IFileSystemExtension
	{
		public static string ReadAllText(this IFileSystem fs, string path)
		{
			return new System.IO.StreamReader(fs.OpenRead(path)).ReadToEnd();
		}
	}

	/// <summary>
	/// 複数のファイルシステムを組み合わせて、一つのファイルシステムとして扱うクラス
	/// </summary>
	public class UnionFileSystem : IFileSystem
	{
		public IFileSystem[] FileSystems { get; private set; }

		public UnionFileSystem(params IFileSystem[] fileSystems)
		{
			FileSystems = fileSystems;
		}

		public IEnumerable<string> GetFiles()
		{
			return FileSystems.SelectMany(fs => fs.GetFiles()).Distinct().ToArray();
		}

		public System.IO.Stream OpenRead(string path)
		{
			foreach( var fs in FileSystems)
			{
				if( fs.Exists(path))
				{
					return fs.OpenRead(path);
				}
			}
			throw new System.IO.FileNotFoundException("File " + path + " not found.");
		}

		public byte[] ReadAllBytes(string path)
		{
			foreach (var fs in FileSystems)
			{
				if (fs.Exists(path))
				{
					return fs.ReadAllBytes(path);
				}
			}
			throw new System.IO.FileNotFoundException("File " + path + " not found.");
		}

		public bool Exists(string path)
		{
			foreach (var fs in FileSystems)
			{
				if (fs.Exists(path))
				{
					return true;
				}
			}
			return false;
		}

		public IFileSystem ExistsFromWhere(string path)
		{
			foreach (var fs in FileSystems)
			{
				if (fs.Exists(path))
				{
					return fs;
				}
			}
			return null;
		}
	}


}

