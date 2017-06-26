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

namespace Docnet
{
	public abstract class NavigationElement<T> : INavigationElement
		where T : class
	{
		/// <summary>
		/// Generates the output for this navigation element
		/// </summary>
		/// <param name="activeConfig">The active configuration to use for the output.</param>
		/// <param name="activePath">The active path navigated through the ToC to reach this element.</param>
		/// <param name="pathSpecification">The path specification.</param>
		public abstract void GenerateOutput(Config activeConfig, NavigatedPath activePath, PathSpecification pathSpecification);
		/// <summary>
		/// Generates the ToC fragment for this element, which can either be a simple line or a full expanded menu.
		/// </summary>
		/// <param name="navigatedPath">The navigated path to the current element, which doesn't necessarily have to be this element.</param>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <param name="pathSpecification">The path specification.</param>
		/// <returns></returns>
		public abstract string GenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot, PathSpecification pathSpecification);
		/// <summary>
		/// Collects the search index entries. These are created from simple navigation elements found in this container, which aren't index element.
		/// </summary>
		/// <param name="collectedEntries">The collected entries.</param>
		/// <param name="activePath">The active path currently navigated.</param>
		/// <param name="pathSpecification">The path specification.</param>
		public abstract void CollectSearchIndexEntries(List<SearchIndexEntry> collectedEntries, NavigatedPath activePath, PathSpecification pathSpecification);

		/// <summary>
		/// Gets the target URL with respect to the <see cref="PathSpecification"/>.
		/// </summary>
		/// <param name="pathSpecification">The path specification.</param>
		/// <returns></returns>
		public abstract string GetTargetURL(PathSpecification pathSpecification);

		#region Properties
		/// <summary>
		/// Gets / sets a value indicating whether this element is the __index element
		/// </summary>
		public abstract bool IsIndexElement { get; set; }

		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the value of this element, which can either be a string or a NavigationLevel
		/// </summary>
		public T Value { get; set; }

		object INavigationElement.Value
		{
			get { return this.Value; }
			set { this.Value = value as T; }
		}

		public NavigationLevel ParentContainer { get; set; }
		#endregion
	}
}
