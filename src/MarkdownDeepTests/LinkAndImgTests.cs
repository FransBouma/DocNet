using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkdownDeep;
using NUnit.Framework;

namespace MarkdownDeepTests
{
	[TestFixture]
	class LinkAndImgTests
	{
		SpanFormatter _formatter;


		[SetUp]
		public void SetUp()
		{
			_formatter = new SpanFormatter(GetSetupMarkdown());
		}


		[Test]
		public void InlineLinkWithoutTitleWithSpecialAttributes()
		{
			var m = GetSetupMarkdown();
			m.ExtraMode = true;
			var s = new SpanFormatter(m);
			Assert.AreEqual("pre <a href=\"url.com\" id=\"foo\" class=\"a b cl\" lang=\"nl\">link text</a> post",
			s.Format("pre [link text](url.com){#foo .a .b .cl lang=nl} post"));
		}


		[Test]
		public void InlineImgWithoutTitleWithSpecialAttributes()
		{
			var m = GetSetupMarkdown();
			m.ExtraMode = true;
			var s = new SpanFormatter(m);
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"alt text\" id=\"foo\" class=\"a b cl\" lang=\"nl\" /> post",
					s.Format("pre ![alt text](url.com/image.png){#foo .a .b .cl lang=nl} post"));
		}

		[Test]
		public void ReferenceLinkWithTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\" title=\"title\">link text</a> post",
					_formatter.Format("pre [link text][link1] post"));
		}

		[Test]
		public void ReferenceLinkIdsAreCaseInsensitive()
		{
			Assert.AreEqual("pre <a href=\"url.com\" title=\"title\">link text</a> post",
					_formatter.Format("pre [link text][LINK1] post"));
		}

		[Test]
		public void ImplicitReferenceLinkWithoutTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\">link2</a> post",
					_formatter.Format("pre [link2] post"));
			Assert.AreEqual("pre <a href=\"url.com\">link2</a> post",
					_formatter.Format("pre [link2][] post"));
		}

		[Test]
		public void ImplicitReferenceLinkWithTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\" title=\"title\">link1</a> post",
					_formatter.Format("pre [link1] post"));
			Assert.AreEqual("pre <a href=\"url.com\" title=\"title\">link1</a> post",
					_formatter.Format("pre [link1][] post"));
		}

		[Test]
		public void ReferenceLinkWithoutTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\">link text</a> post",
					_formatter.Format("pre [link text][link2] post"));
		}

		[Test]
		public void MissingReferenceLink()
		{
			Assert.AreEqual("pre [link text][missing] post",
					_formatter.Format("pre [link text][missing] post"));
		}

		[Test]
		public void InlineLinkWithTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\" title=\"title\">link text</a> post",
					_formatter.Format("pre [link text](url.com \"title\") post"));
		}

		[Test]
		public void InlineLinkWithoutTitle()
		{
			Assert.AreEqual("pre <a href=\"url.com\">link text</a> post",
					_formatter.Format("pre [link text](url.com) post"));
		}

		[Test]
		public void Boundaries()
		{
			Assert.AreEqual("<a href=\"url.com\">link text</a>",
					_formatter.Format("[link text](url.com)"));
			Assert.AreEqual("<a href=\"url.com\" title=\"title\">link text</a>",
					_formatter.Format("[link text][link1]"));
		}


		[Test]
		public void ReferenceImgWithTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"alt text\" title=\"title\" /> post",
					_formatter.Format("pre ![alt text][img1] post"));
		}

		[Test]
		public void ImplicitReferenceImgWithoutTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"img2\" /> post",
					_formatter.Format("pre ![img2] post"));
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"img2\" /> post",
					_formatter.Format("pre ![img2][] post"));
		}

		[Test]
		public void ImplicitReferenceImgWithTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"img1\" title=\"title\" /> post",
					_formatter.Format("pre ![img1] post"));
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"img1\" title=\"title\" /> post",
					_formatter.Format("pre ![img1][] post"));
		}

		[Test]
		public void ReferenceImgWithoutTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"alt text\" /> post",
					_formatter.Format("pre ![alt text][img2] post"));
		}

		[Test]
		public void MissingReferenceImg()
		{
			Assert.AreEqual("pre ![alt text][missing] post",
					_formatter.Format("pre ![alt text][missing] post"));
		}

		[Test]
		public void InlineImgWithTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"alt text\" title=\"title\" /> post",
					_formatter.Format("pre ![alt text](url.com/image.png \"title\") post"));
		}

		[Test]
		public void InlineImgWithoutTitle()
		{
			Assert.AreEqual("pre <img src=\"url.com/image.png\" alt=\"alt text\" /> post",
					_formatter.Format("pre ![alt text](url.com/image.png) post"));
		}


		[Test]
		public void ImageLink() 
		{
			Assert.AreEqual("pre <a href=\"url.com\"><img src=\"url.com/image.png\" alt=\"alt text\" /></a> post",
					_formatter.Format("pre [![alt text](url.com/image.png)](url.com) post"));
		}



		private Markdown GetSetupMarkdown()
		{
			var toReturn = new Markdown();
			toReturn.AddLinkDefinition(new LinkDefinition("link1", "url.com", "title"));
			toReturn.AddLinkDefinition(new LinkDefinition("link2", "url.com"));
			toReturn.AddLinkDefinition(new LinkDefinition("img1", "url.com/image.png", "title"));
			toReturn.AddLinkDefinition(new LinkDefinition("img2", "url.com/image.png"));
			return toReturn;
		}
	}
}
