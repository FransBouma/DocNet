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


		// Make sure you copy the AutoHeaderIDTests.cs file to c:\temp, as it's used in one of the tests, which uses an absolute path for the snippet feature
		// a relative path would have been preferable but the tests are run in a temp folder so they don't know the path of the source. 
		[Test, TestCaseSource("GetTests")]
		public void Test(string resourceName)
		{
			Utils.RunResourceTest(resourceName);
		}
	}
}
