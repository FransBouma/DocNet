using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Docnet
{
	/// <summary>
	/// Contains the elements currently navigated to get to the current location. 
	/// </summary>
	public class NavigatedPath : Stack<INavigationElement>
	{
		/// <summary>
		/// Creates the bread crumbs HTML of the elements in this path, delimited by '/' characters. 
		/// </summary>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <returns></returns>
		public string CreateBreadCrumbsHTML(string relativePathToRoot)
		{
			var fragments = new List<string>();
			// we enumerate a stack, which enumerates from top to bottom, so we have to reverse things first. 
			foreach(var element in this.Reverse())
			{
				var targetURL = element.TargetURL;
				if(string.IsNullOrWhiteSpace(targetURL))
				{
					fragments.Add(string.Format("<li>{0}</li>", element.Name));
				}
				else
				{
					fragments.Add(string.Format("<li><a href=\"{0}{1}\">{2}</a></li>", relativePathToRoot, HttpUtility.UrlEncode(targetURL), element.Name));
				}
			}
			return string.Format("<ul>{0}</ul>{1}", string.Join(" / ", fragments.ToArray()), Environment.NewLine);
		}


		public string CreateBreadCrumbsText(string relativePathToRoot)
		{
			var fragments = new List<string>();
			// we enumerate a stack, which enumerates from top to bottom, so we have to reverse things first. 
			foreach(var element in this.Reverse())
			{
				fragments.Add(element.Name);
			}
			return string.Join(" / ", fragments.ToArray());
		}


		/// <summary>
		/// Creates the ToC HTML for the element reached by the elements in this path. All containers in this path are expanded, all elements inside these containers which
		/// aren't, are not expanded. 
		/// </summary>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <returns></returns>
		public string CreateToCHTML(string relativePathToRoot)
		{
			// the root container is the bottom element of this path. We use that container to build the root and navigate any node open along the navigated path. 
			var rootContainer = this.Reverse().FirstOrDefault() as NavigationLevel;
			if(rootContainer == null)
			{
				// no root container, no TOC
				return string.Empty;
			}
			return rootContainer.GenerateToCFragment(this, relativePathToRoot);
		}
	}
}
