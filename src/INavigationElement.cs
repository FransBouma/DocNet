using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docnet
{
	public interface INavigationElement
	{
		/// <summary>
		/// Generates the output for this navigation element
		/// </summary>
		/// <param name="activeConfig">The active configuration to use for the output.</param>
		/// <param name="activePath">The active path navigated through the ToC to reach this element.</param>
		void GenerateOutput(Config activeConfig, NavigatedPath activePath);


		/// <summary>
		/// Generates the ToC fragment for this element, which can either be a simple line or a full expanded menu.
		/// </summary>
		/// <param name="navigatedPath">The navigated path to the current element, which doesn't necessarily have to be this element.</param>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <returns></returns>
		string GenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot);
		/// <summary>
		/// Collects the search index entries. These are created from simple navigation elements found in this container, which aren't index element.
		/// </summary>
		/// <param name="collectedEntries">The collected entries.</param>
		/// <param name="activePath">The active path currently navigated.</param>
		void CollectSearchIndexEntries(List<SearchIndexEntry> collectedEntries, NavigatedPath activePath);


		/// <summary>
		/// Gets a value indicating whether this element is the __index element
		/// </summary>
		bool IsIndexElement { get; set; }
		string Name { get; set; }
		object Value { get; set; }
		string TargetURL { get; }
		NavigationLevel ParentContainer { get; set; }
	}
}
