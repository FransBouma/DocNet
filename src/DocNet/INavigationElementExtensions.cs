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
		/// <param name="pathSpecification">The path specification.</param>
		/// <returns></returns>
		public static string GetFinalTargetUrl(this INavigationElement navigationElement, PathSpecification pathSpecification)
		{
			var targetUrl = navigationElement.GetTargetURL(pathSpecification);
			var link = HttpUtility.UrlPathEncode(targetUrl);

            // Disabled for now as discussed in #65 (https://github.com/FransBouma/DocNet/pull/65), but
            // is required for #44
            //if (pathSpecification == PathSpecification.RelativeAsFolder)
            //{
            //    if (link.Length > IndexHtmFileName.Length &&
            //        link.EndsWith(IndexHtmFileName, StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        link = link.Substring(0, link.Length - IndexHtmFileName.Length);
            //    }
            //}

            return link;
		}
	}
}