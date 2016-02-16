using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MarkdownDeep;
using System.Reflection;

namespace MarkdownDeepTests
{
	[TestFixture]
	class DocNetModeTests
	{
		public static IEnumerable<TestCaseData> GetTests()
		{
			return Utils.GetTests("docnetmode");
		}


		[Test, TestCaseSource("GetTests")]
		public void Test(string resourceName)
		{
			Utils.RunResourceTest(resourceName);
		}
	}
}
