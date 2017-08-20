using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MarkdownDeep;

namespace Docnet
{
	public class NotFoundNavigationElement : SimpleNavigationElement
	{
		public NotFoundNavigationElement()
		{
			this.Name = "Page not found";
			this.IsIndexElement = false;
			this.ContentProducerFunc = GenerateContent;
		}

		private string GenerateContent(SimpleNavigationElement element, Config config, NavigationContext navigationContext)
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("# Page not found (404)");
			stringBuilder.AppendLine();

			stringBuilder.AppendLine("Unfortunately the page you were looking for does not exist. In order to help you out ");
			stringBuilder.AppendLine("in the best way possible, below is the table of contents:");
			stringBuilder.AppendLine();

			foreach (var sibling in this.ParentContainer.Value)
			{
				if (sibling == this)
				{
					continue;
				}

				stringBuilder.AppendFormat("* [{0}]({1}{2}){3}", sibling.Name, "/" /* pathRelativeToRoot */,
					sibling.GetFinalTargetUrl(navigationContext), Environment.NewLine);
			}

			var markdownContent = stringBuilder.ToString();

			var destinationFile = Utils.MakeAbsolutePath(config.Destination, this.GetTargetURL(navigationContext));

			var htmlContent = Utils.ConvertMarkdownToHtml(markdownContent, Path.GetDirectoryName(destinationFile), config.Destination, 
				string.Empty, new List<Heading>(), config.ConvertLocalLinks, 
				new NavigationContext(config.PathSpecification, config.UrlFormatting, config.MaxLevelInToC, config.StripIndexHtm));
			return htmlContent;
		}

		public override string GenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot, NavigationContext navigationContext)
		{
			// Skip
			return string.Empty;
		}

		public override void CollectSearchIndexEntries(List<SearchIndexEntry> collectedEntries, NavigatedPath activePath, NavigationContext navigationContext)
		{
			// Skip
		}

		public override string GetTargetURL(NavigationContext navigationContext)
		{
			return "404.htm";
		}

		public override bool IsIndexElement { get; set; }
	}
}