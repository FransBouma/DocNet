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

namespace Docnet
{
	public class SimpleNavigationElement : NavigationElement<string>
	{
		#region Members
		private string _targetURLForHTML;
		private List<Tuple<string, string>> _relativeH2LinksOnPage;		// first element in Tuple is anchor name, second is name for ToC.
		#endregion


		public SimpleNavigationElement()
		{
			_relativeH2LinksOnPage = new List<Tuple<string, string>>();
		}


		/// <summary>
		/// Generates the output for this navigation element
		/// </summary>
		/// <param name="activeConfig">The active configuration to use for the output.</param>
		/// <param name="activePath">The active path navigated through the ToC to reach this element.</param>
		public override void GenerateOutput(Config activeConfig, NavigatedPath activePath)
		{
			// if we're the __index element, we're not pushing ourselves on the path, as we're representing the container we're in, which is already on the path.
			if(!this.IsIndexElement)
			{
				activePath.Push(this);
			}
			_relativeH2LinksOnPage.Clear();
			var sourceFile = Utils.MakeAbsolutePath(activeConfig.Source, this.Value);
			var destinationFile = Utils.MakeAbsolutePath(activeConfig.Destination, this.TargetURL);
			var sb = new StringBuilder(activeConfig.PageTemplateContents.Length + 2048);
			var content = string.Empty;
			this.MarkdownFromFile = string.Empty;
			if(File.Exists(sourceFile))
			{
				this.MarkdownFromFile = File.ReadAllText(sourceFile);
				content = Utils.ConvertMarkdownToHtml(this.MarkdownFromFile, Path.GetDirectoryName(destinationFile), activeConfig.Destination, _relativeH2LinksOnPage);
			}
			else
			{
				// if we're not the index element, the file is missing and potentially it's an error in the config page. 
				// Otherwise we can simply assume we are a missing index page and we'll generate default markdown so the user has something to look at.
				if(this.IsIndexElement)
				{
					// replace with default markdown snippet. This is the name of our container and links to the elements in that container as we are the index page that's not
					// specified / existend. 
					var defaultMarkdown = new StringBuilder();
					defaultMarkdown.AppendFormat("# {0}{1}{1}", this.ParentContainer.Name, Environment.NewLine);
					defaultMarkdown.AppendFormat("Please select one of the topic in this section:{0}{0}", Environment.NewLine);
					foreach(var sibling in this.ParentContainer.Value)
					{
						if(sibling == this)
						{
							continue;
						}
						defaultMarkdown.AppendFormat("* [{0}]({1}){2}", sibling.Name, HttpUtility.UrlPathEncode(sibling.TargetURL), Environment.NewLine);
					}
					defaultMarkdown.Append(Environment.NewLine);
					content = Utils.ConvertMarkdownToHtml(defaultMarkdown.ToString(), Path.GetDirectoryName(destinationFile), activeConfig.Destination, _relativeH2LinksOnPage);
				}
				else
				{
					// target not found. See if there's a content producer func to produce html for us. If not, we can only conclude an error in the config file.
					if(this.ContentProducerFunc == null)
					{
						throw new FileNotFoundException(string.Format("The specified markdown file '{0}' couldn't be found. Aborting", sourceFile));
					}
					content = this.ContentProducerFunc(this);
				}
			}
			sb.Append(activeConfig.PageTemplateContents);
			sb.Replace("{{Name}}", activeConfig.Name);
			sb.Replace("{{Footer}}", activeConfig.Footer);
			sb.Replace("{{TopicTitle}}", this.Name);
			var relativePathToRoot = Utils.MakeRelativePath(Path.GetDirectoryName(destinationFile), activeConfig.Destination).Replace(@"\", @"/");
			sb.Replace("{{Path}}", relativePathToRoot);
			sb.Replace("{{Breadcrumbs}}", activePath.CreateBreadCrumbsHTML(relativePathToRoot));
			sb.Replace("{{ToC}}", activePath.CreateToCHTML(relativePathToRoot));
			sb.Replace("{{ExtraScript}}", (this.ExtraScriptProducerFunc == null) ? string.Empty : this.ExtraScriptProducerFunc(this));
            
            // Check if the content contains @@include tag
            content = Utils.IncludeProcessor(content, Path.Combine(activeConfig.Source, "_partials"));
            
            // the last action has to be replacing the content marker, so markers in the content which we have in the template as well aren't replaced 
            sb.Replace("{{Content}}", content);
			Utils.CreateFoldersIfRequired(destinationFile);
			File.WriteAllText(destinationFile, sb.ToString());
			if(!this.IsIndexElement)
			{
				activePath.Pop();
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
			// simply convert ourselves into an entry if we're not an index
			if(!this.IsIndexElement)
			{
				var toAdd = new SearchIndexEntry();
				toAdd.Fill(this.MarkdownFromFile, this.TargetURL, this.Name, activePath);
				collectedEntries.Add(toAdd);
			}
			activePath.Pop();
		}


		/// <summary>
		/// Generates the ToC fragment for this element, which can either be a simple line or a full expanded menu.
		/// </summary>
		/// <param name="navigatedPath">The navigated path to the current element, which doesn't necessarily have to be this element.</param>
		/// <param name="relativePathToRoot">The relative path back to the URL root, e.g. ../.., so it can be used for links to elements in this path.</param>
		/// <returns></returns>
		public override string GenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot)
		{
			// index elements are rendered in the parent container.
			if(this.IsIndexElement)
			{
				return string.Empty;
			}

			return PerformGenerateToCFragment(navigatedPath, relativePathToRoot);
		}


		/// <summary>
		/// The actual implementation of GenerateToCFragment. This is factored out to be able to re-use the fragment HTML product logic for the root index element
		/// which would otherwise be skipped as it's an index element.
		/// </summary>
		/// <param name="navigatedPath">The navigated path.</param>
		/// <param name="relativePathToRoot">The relative path to root.</param>
		/// <returns></returns>
		public string PerformGenerateToCFragment(NavigatedPath navigatedPath, string relativePathToRoot)
		{
			// we can't navigate deeper from here. If we are the element being navigated to, we are the current and will have to emit any additional relative URLs too.
			bool isCurrent = navigatedPath.Contains(this);
			var fragments = new List<string>();
			var liClass = "tocentry";
			var aClass = string.Empty;
			if(isCurrent)
			{
				liClass = "tocentry current";
				aClass = "current";
			}
			fragments.Add(string.Format("<li{0}><a{1} href=\"{2}{3}\">{4}</a>",
										string.IsNullOrWhiteSpace(liClass) ? string.Empty : string.Format(" class=\"{0}\"", liClass),
										string.IsNullOrWhiteSpace(aClass) ? string.Empty : string.Format(" class=\"{0}\"", aClass),
										relativePathToRoot,
										HttpUtility.UrlPathEncode(this.TargetURL),
										this.Name));
			if(isCurrent && _relativeH2LinksOnPage.Any())
			{
				// generate relative links
				fragments.Add(string.Format("<ul class=\"{0}\">", this.ParentContainer.IsRoot ? "currentrelativeroot" : "currentrelative"));
				foreach(var p in _relativeH2LinksOnPage)
				{
					fragments.Add(string.Format("<li class=\"tocentry\"><a href=\"#{0}\">{1}</a></li>", p.Item1, p.Item2));
				}
				fragments.Add("</ul>");
			}
			else
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
				if(_targetURLForHTML==null)
				{
					_targetURLForHTML = (this.Value ?? string.Empty);
					if(_targetURLForHTML.ToLowerInvariant().EndsWith(".md"))
					{
						_targetURLForHTML = _targetURLForHTML.Substring(0, _targetURLForHTML.Length-3) + ".html";
					}
					_targetURLForHTML = _targetURLForHTML.Replace("\\", "/");
				}
				return _targetURLForHTML;
			}
		}


		/// <summary>
		/// Gets / sets a value indicating whether this element is the __index element
		/// </summary>
		public override bool IsIndexElement { get; set; }

		/// <summary>
		/// Gets or sets the content producer function, which is used if the target file specified isn't available and default markdown isn't appropriate.
		/// If null, and the target file isn't found, the engine will throw an error as there's no content. If set, it's utilized over the default markdown producer.
		/// Use this to produce in-line HTML for specific pages which aren't specified, like the search page.
		/// </summary>
		public Func<SimpleNavigationElement, string> ContentProducerFunc { get; set; }
		/// <summary>
		/// Gets or sets the extra script producer function, which, if set, produces HTML to be embedded at the extra script marker
		/// </summary>
		public Func<SimpleNavigationElement, string> ExtraScriptProducerFunc { get; set; }

		/// <summary>
		/// Gets the loaded markdown text from the file.
		/// </summary>
		public string MarkdownFromFile
		{
			get;
			private set;
		}
		#endregion
	}
}
