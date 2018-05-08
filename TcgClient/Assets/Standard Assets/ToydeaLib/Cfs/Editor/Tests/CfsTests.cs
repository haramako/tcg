using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cfs
{
	[TestFixture]
	public class CfsTest
	{

	}

	[TestFixture]
	public class BucketTest
	{
		[TestCase]
		public void TestMerge()
		{
			var src1 =
				"1234567890123456789012\tf1\t10\t0\t1234567890123456789012\t10\t0\n" +
				"1234567890123456789012\tf2\t10\t0\t1234567890123456789012\t10\t0\n" +
				"1234567890123456789012\tf3\t10\t0\t1234567890123456789012\t10\t0\n";
			var src2 =
				"1234567890123456789012\tf1\t20\t0\t1234567890123456789012\t10\t0\n" +
				"1234567890123456789012\tf4\t30\t0\t1234567890123456789012\t10\t0\n";
			var b1 = new Bucket(src1);
			var b2 = new Bucket(src2);
			b1.Merge(b2);
			Assert.AreEqual(4, b1.Files.Count);
		}


	}
}
