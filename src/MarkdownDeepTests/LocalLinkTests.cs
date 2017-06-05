using System.Collections.Generic;
using NUnit.Framework;

namespace MarkdownDeepTests
{
    [TestFixture]
    class LocalLinkTests
    {
        public static IEnumerable<TestCaseData> GetTests_locallinks_enabled()
        {
            return Utils.GetTests("locallinks_enabled");
        }


        [Test, TestCaseSource("GetTests_locallinks_enabled")]
        public void Test_locallinks_enabled(string resourceName)
        {
            Utils.RunResourceTest(resourceName);
        }

        public static IEnumerable<TestCaseData> GetTests_locallinks_disabled()
        {
            return Utils.GetTests("locallinks_disabled");
        }


        [Test, TestCaseSource("GetTests_locallinks_disabled")]
        public void Test_locallinks_disabled(string resourceName)
        {
            Utils.RunResourceTest(resourceName);
        }
    }
}