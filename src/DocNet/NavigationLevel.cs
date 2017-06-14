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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Docnet
{
	public class NavigationLevel : NavigationElement<List<INavigationElement>>
	{
		public NavigationLevel() : base()
		{
			this.Value = new List<INavigationElement>();
		}


		public void Load(JObject dataFromFile)
		{
			foreach(KeyValuePair<string, JToken> child in dataFromFile)
			{
				INavigationElement toAdd;
				if(child.Value.Type == JTokenType.String)
				{
					var nameToUse = child.Key;
					var isIndexElement = child.Key == "__index";
					if(isIndexElement)
					{
						nameToUse = this.Name;
					}
					toAdd = new SimpleNavigationElement() { Name = nameToUse, Value = child.Value.ToObject<string>(), IsIndexElement = isIndexElement};
				}
				else
				{
					var subLevel = new NavigationLevel() { Name = child.Key, IsRoot = false};
					subLevel.Load((JObject)child.Value);
					toAdd = subLevel;
				}
				toAdd.ParentContainer = this;
				this.Value.Add(toAdd);
			}
		}


		/// <summary>
		/// Collects the search index entries. These are created from simple navigation elements found in this container, which aren't index element.
		/// </summary>
		/// <param name="collectedEntries">The collected entries.</param>
		/// <param name="activePath">The active path currently navigated.</param>
		public override void CollectSearchIndexEntries(List<SearchIndexEntry> collectedEntries, NavigatedPath activePath)
		{
			activePath.Push(this);
			foreach(var element in this.Value)
			{
				element.CollectSearchIndexEntries(collectedEntries, activePath);
			}
			activePath.Pop();
		}


		/// <summary>
		/// Generates the output for this navigation element
		/// </summary>
		/// <param name="activeConfig">The active configuration to use for the output.</param>
		/// <param name="activePath">The active path navigated through the ToC to reach this element.</param>
		public override void GenerateOutput(Config activeConfig, NavigatedPath activePath)
		{
			activePath.Push(this);
			int i = 0;
			while(i<this.Value.Count)
			{
				var element = this.Value[i];
				element.GenerateOutput(activeConfig, activePath);
				i++;
			}
			activePath.Pop();
		}


		/// <summary>
		/// Generates the ToC fragment for this element, which can either be a simple line or a full expanded menu.
		/// </summary>
		/// <param name="navigatedPath">The navigated path to the current element, which doesn't necessarily have to be this element.</param>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <param name="maxLevel">The maximum level.</param>
		/// <returns></returns>
		public override string GenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot, int maxLevel)
		{
			var fragments = new List<string>();
			if(!this.IsRoot)
			{
				fragments.Add("<li class=\"tocentry\">");
			}
			if(navigatedPath.Contains(this))
			{
				// we're expanded. If we're not root and on the top of the navigated path stack, our index page is the page we're currently generating the ToC for, so 
				// we have to mark the entry as 'current'
				if(navigatedPath.Peek() == this && !this.IsRoot)
				{
					fragments.Add("<ul class=\"current\">");
				}
				else
				{
					fragments.Add("<ul>");
				}

				// first render the level header, which is the index element, if present or a label. The root always has an __index element otherwise we'd have stopped at load.
				var elementStartTag = "<li><span class=\"navigationgroup\"><i class=\"fa fa-caret-down\"></i> ";
				var indexElement = this.IndexElement;
				if(indexElement == null)
				{
					fragments.Add(string.Format("{0}{1}</span></li>", elementStartTag, this.Name));
				}
				else
				{
					if(this.IsRoot)
					{
						fragments.Add(indexElement.PerformGenerateToCFragment(navigatedPath, relativePathToRoot, maxLevel));
					}
					else
					{
						fragments.Add(string.Format("{0}<a href=\"{1}{2}\">{3}</a></span></li>", elementStartTag, relativePathToRoot, HttpUtility.UrlPathEncode(indexElement.TargetURL), 
													this.Name));
					}
				}
				// then the elements in the container. Index elements are skipped here.
				foreach(var element in this.Value)
				{
					fragments.Add(element.GenerateToCFragment(navigatedPath, relativePathToRoot, maxLevel));
				}
				fragments.Add("</ul>");
			}
			else
			{
				// just a link
				fragments.Add(string.Format("<span class=\"navigationgroup\"><i class=\"fa fa-caret-right\"></i> <a href=\"{0}{1}\">{2}</a></span>", 
											relativePathToRoot, HttpUtility.UrlPathEncode(this.TargetURL), this.Name));
			}
			if(!this.IsRoot)
			{
				fragments.Add("</li>");
			}
			return string.Join(Environment.NewLine, fragments.ToArray());
		}


		#region Properties
		public override string TargetURL
		{
			get
			{
				var defaultElement = this.IndexElement;
				if(defaultElement == null)
				{
					return string.Empty;
				}
				return defaultElement.TargetURL ?? string.Empty;
			}
		}


		public SimpleNavigationElement IndexElement
		{
			get
			{
				var toReturn = this.Value.FirstOrDefault(e => e.IsIndexElement) as SimpleNavigationElement;
				if(toReturn == null)
				{
					// no index element, add an artificial one.
					var path = string.Empty;
					if(this.ParentContainer != null)
					{
						path = Path.GetDirectoryName(this.ParentContainer.TargetURL);
					}
					var nameToUse = this.Name.Replace(".", "").Replace('/', '_').Replace("\\", "_").Replace(":", "").Replace(" ", "");
					if(string.IsNullOrWhiteSpace(nameToUse))
					{
						return null;
					}
					toReturn = new SimpleNavigationElement() {ParentContainer = this, Value = string.Format("{0}{1}.md", path, nameToUse), Name = this.Name, IsIndexElement = true};
					this.Value.Add(toReturn);
				}

				return toReturn;
			}
		}


		/// <summary>
		/// Gets / sets a value indicating whether this element is the __index element
		/// </summary>
		public override bool IsIndexElement
		{
			// never an index
			get { return false; }
			set {  
				// nop;
			}
		}


		public bool IsRoot { get; set; }
		#endregion
	}
}
