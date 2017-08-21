using System;
using System.Web;

namespace Docnet
{
	public static class INavigationElementExtensions
	{
	    private const string IndexHtmFileName = "index.htm";

		/// <summary>
		/// Gets the final URL by encoding the path and by removing the filename if it equals <c>index.htm</c>.
		/// </summary>
		/// <param name="navigationElement">The navigation element.</param>
		/// <param name="navigationContext">The navigation context.</param>
		/// <returns></returns>
		public static string GetFinalTargetUrl(this INavigationElement navigationElement, NavigationContext navigationContext)
		{
			var targetUrl = navigationElement.GetTargetURL(navigationContext);
			return GetFinalTargetUrl(targetUrl, navigationContext);
		}

		/// <summary>
		/// Gets the final URL by encoding the path and by removing the filename if it equals <c>index.htm</c>.
		/// </summary>
		/// <param name="targetUrl">The target URL.</param>
		/// <param name="navigationContext">The navigation context.</param>
		/// <returns></returns>
		public static string GetFinalTargetUrl(this string targetUrl, NavigationContext navigationContext)
		{
			var link = HttpUtility.UrlPathEncode(targetUrl);

			if (navigationContext.StripIndexHtm)
			{
				if (link.Length > IndexHtmFileName.Length &&
				    link.EndsWith(IndexHtmFileName, StringComparison.InvariantCultureIgnoreCase))
				{
					link = link.Substring(0, link.Length - IndexHtmFileName.Length);
				}
			}

			return link;
		}
	}
}