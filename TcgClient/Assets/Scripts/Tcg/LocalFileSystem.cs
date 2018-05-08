using System;
using System.Collections.Generic;
using System.IO;
using Game;
using System.Linq;

namespace Game
{
	public class LocalFileSystem : IFileSystem
	{
		public string RootPath { get; private set; }

		public LocalFileSystem(string rootPath)
		{
			RootPath = rootPath;
		}

		IEnumerable<string> IFileSystem.GetFiles()
		{
			return new DirectoryInfo(RootPath).GetFiles().Select(f => f.Name);
		}

		Stream IFileSystem.OpenRead(string path)
		{
			return File.OpenRead(Path.Combine(RootPath, path));
		}

		byte[] IFileSystem.ReadAllBytes(string path)
		{
			return File.ReadAllBytes(Path.Combine(RootPath, path));
		}

		bool IFileSystem.Exists(string path)
		{
			return File.Exists(Path.Combine(RootPath, path));
		}
	}
}
