using System.Collections.Generic;
using MarkdownDeep;
using NUnit.Framework;

namespace MarkdownDeepTests
{
	[TestFixture]
	public class ExtensionsTests
	{
		[TestCase]
		public void ConvertsHeadingsHierarchy()
		{
			var headings = new List<Heading>();
			headings.Add(new Heading { Level = 1, Name = "1" });
			headings.Add(new Heading { Level = 2, Name = "1.1" });
			headings.Add(new Heading { Level = 3, Name = "1.1.1" });
			headings.Add(new Heading { Level = 2, Name = "1.2" });
			headings.Add(new Heading { Level = 4, Name = "1.2.1.1" });
			headings.Add(new Heading { Level = 2, Name = "1.3" });
			headings.Add(new Heading { Level = 1, Name = "2" });
			headings.Add(new Heading { Level = 3, Name = "2.1.1" });
			headings.Add(new Heading { Level = 2, Name = "2.2" });

			var hierarchy = headings.ConvertToHierarchy();

			Assert.AreEqual(2, hierarchy.Count);

			var heading1 = hierarchy[0];
			Assert.AreEqual("1", heading1.Name);
			Assert.AreEqual(3, heading1.Children.Count);

			var heading1_1 = heading1.Children[0];
			Assert.AreEqual("1.1", heading1_1.Name);
			Assert.AreEqual(1, heading1_1.Children.Count);

			var heading1_1_1 = heading1_1.Children[0];
			Assert.AreEqual("1.1.1", heading1_1_1.Name);
			Assert.AreEqual(0, heading1_1_1.Children.Count);

			var heading1_2 = heading1.Children[1];
			Assert.AreEqual("1.2", heading1_2.Name);
			Assert.AreEqual(1, heading1_2.Children.Count);

			var heading1_2_1_1 = heading1_2.Children[0];
			Assert.AreEqual("1.2.1.1", heading1_2_1_1.Name);
			Assert.AreEqual(0, heading1_2_1_1.Children.Count);

			var heading1_3 = heading1.Children[2];
			Assert.AreEqual("1.3", heading1_3.Name);
			Assert.AreEqual(0, heading1_3.Children.Count);

			var heading2 = hierarchy[1];
			Assert.AreEqual("2", heading2.Name);
			Assert.AreEqual(2, heading2.Children.Count);

			var heading2_1_1 = heading2.Children[0];
			Assert.AreEqual("2.1.1", heading2_1_1.Name);
			Assert.AreEqual(0, heading2_1_1.Children.Count);

			var heading2_2 = heading2.Children[1];
			Assert.AreEqual("2.2", heading2_2.Name);
			Assert.AreEqual(0, heading2_2.Children.Count);
		}
	}
}