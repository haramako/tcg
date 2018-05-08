using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.ProtocolBuffers;

public class PbFile
{
	public static byte[] buf = new byte[1024 * 1024];

	public static IEnumerable<T> ReadPbList<T>(Func<T> constructor, byte[] buf, int offset = 0, int len = -1) where T: Message
	{
		//var len = s.Read (buf, 0, buf.Length);
		if (len < 0)
		{
			len = buf.Length - offset;
		}
		var cis = CodedInputStream.CreateInstance (buf, offset, len);
		var magic = cis.ReadRawByte ();
		if( magic != 'C' ) throw new ArgumentException("Invalid stream header");
		var headerSize = cis.ReadRawByte ();
		cis.SkipRawBytes (headerSize); // 読み飛ばすだけ

		uint size = 0;
		while( !cis.IsAtEnd )
		{
			if (!cis.ReadFixed32 (ref size))
			{
				break;
			}
			// TODO: サイズ０（すべてデフォルト値）の時に問題がおこるので対策を案が得ること
			if (size <= 0) break; // 0 パディングされている場合
			var oldLimit = cis.PushLimit((int)size);
			T message = constructor ();
			message.MergeFrom(cis);
			cis.PopLimit (oldLimit);
			yield return message;
		}
	}

}
