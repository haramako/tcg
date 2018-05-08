using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if false
using Ionic.Zlib;
#endif

#if false

public class PbxFile
{

	public string Filename { get; private set; }
	Dictionary<int, int> intIndex;
	Dictionary<string, int> stringIndex;
	int dataOrigin;

	public PbxFile(string filename)
	{
		Filename = filename;
		readIndex ();
	}

	private void readIndex()
	{
		using (var file = File.OpenRead (Filename))
		{
			var r = new BinaryReader (file);
			var magic = r.ReadByte ();
			if( magic != 'C' ) throw new ArgumentException("Invalid stream header");
			var headerSize = r.ReadByte ();
			r.ReadBytes (headerSize); // 読み飛ばすだけ
			var indexSize = r.ReadInt32();
			var pbx = Master.PbxHeader.ParseFrom (new PbFile.LimitedInputStream (file, indexSize));
			intIndex = pbx.IntIndex.ToDictionary (i => i.Key, i => i.Value);
			stringIndex = pbx.StringIndex.ToDictionary (i => i.Key, i => i.Value);
			dataOrigin = 1 + 1 + headerSize + 4 + indexSize; // magic(1byte) + headerSize(1byte) + header + indexSize(4byte) + index
		}
	}

	public bool Exists(int key)
	{
		return intIndex.ContainsKey(key);
	}

	public bool Exists(string key)
	{
		return stringIndex.ContainsKey(key);
	}

	public Stream GetStream(int key)
	{
		return getStreamByPosition( intIndex [key] );
	}

	public Stream GetStream(string key)
	{
		return getStreamByPosition( stringIndex [key] );
	}

	Stream getStreamByPosition(int pos)
	{
		var file = File.OpenRead (Filename);
		var r = new BinaryReader (file);
		file.Seek (dataOrigin + pos, SeekOrigin.Begin);
		var len = r.ReadInt32 ();
		var stream = new PbFile.LimitedInputStream (file, len);
		// ZLibのヘッダーを2byteを読み飛ばす
		// See: http://wiz.came.ac/blog/2009/09/zlibdll-zlibnet-deflatestream.html
		stream.ReadByte();
		stream.ReadByte ();
		return new DeflateStream (stream, CompressionMode.Decompress);
	}

}
#endif