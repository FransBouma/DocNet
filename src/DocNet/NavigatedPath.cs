//////////////////////////////////////////////////////////////////////////////////////////////
// DocNet is licensed under the MIT License (MIT)
// Copyright(c) 2016 Frans Bouma
// Get your copy at: https://github.com/FransBouma/DocNet 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies 
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////////////////
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
		/// <param name="navigationContext">The navigation context.</param>
		/// <returns></returns>
		public string CreateBreadCrumbsHTML(string relativePathToRoot, NavigationContext navigationContext)
		{
			var fragments = new List<string>();
			// we enumerate a stack, which enumerates from top to bottom, so we have to reverse things first. 
			foreach(var element in this.Reverse())
			{
				var targetURL = element.GetTargetURL(navigationContext);
				if(string.IsNullOrWhiteSpace(targetURL))
				{
					fragments.Add(string.Format("<li>{0}</li>", element.Name));
				}
				else
				{
					fragments.Add(string.Format("<li><a href=\"{0}{1}\">{2}</a></li>", relativePathToRoot, Uri.EscapeUriString(targetURL), element.Name));
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
		/// <param name="navigationContext">The navigation context.</param>
		/// <returns></returns>
		public string CreateToCHTML(string relativePathToRoot, NavigationContext navigationContext)
		{
			// the root container is the bottom element of this path. We use that container to build the root and navigate any node open along the navigated path. 
			var rootContainer = this.Reverse().FirstOrDefault() as NavigationLevel;
			if(rootContainer == null)
			{
				// no root container, no TOC
				return string.Empty;
			}
			return rootContainer.GenerateToCFragment(this, relativePathToRoot, navigationContext);
		}
	}
}
