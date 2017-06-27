using System;
using System.Web;

namespace Docnet
{
	public static class INavigationElementExtensions
	{
		/// <summary>
		/// Gets the final URL by encoding the path and by removing the filename if it equals <c>index.htm</c>.
		/// </summary>
		/// <param name="navigationElement">The navigation element.</param>
		/// <param name="pathSpecification">The path specification.</param>
		/// <returns></returns>
		public static string GetFinalTargetUrl(this INavigationElement navigationElement, PathSpecification pathSpecification)
		{
			var targetUrl = navigationElement.GetTargetURL(pathSpecification);
			var link = HttpUtility.UrlPathEncode(targetUrl);

			if (link.EndsWith("index.htm", StringComparison.InvariantCultureIgnoreCase))
			{
				link = link.Substring(0, link.Length - "index.htm".Length);
			}

			return link;
		}
	}
}