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
using Newtonsoft.Json.Linq;

namespace Docnet
{
	public class Config
	{
		#region Members
		private string _configFileSourcePath, _templateContents, _docsSourcePath, _destinationPath, _themeFolder;
		private dynamic _configData;
		private NavigationLevel _pages;
		#endregion
		
		internal bool Load(string configFile)
		{
			_configFileSourcePath = Path.GetDirectoryName(configFile);

			var configData = File.ReadAllText(configFile, Encoding.UTF8);
			if(string.IsNullOrWhiteSpace(configData))
			{
				Console.WriteLine("[ERROR] '{0}' is empty.", configFile);
				return false;
			}
			_configData = JObject.Parse(configData);
			if(_configData == null)
			{
				Console.WriteLine("[ERROR] Parsing '{0}' failed!", configFile);
				return false;
			}
			if(string.IsNullOrWhiteSpace(this.ThemeFolder) || !Directory.Exists(this.ThemeFolder))
			{
				Console.WriteLine("[ERROR] Theme '{0}' or Themes folder not found.", this.ThemeFolder);
				return false;
			}
			_templateContents = File.ReadAllText(this.PageTemplateFile, Encoding.UTF8);
			if(string.IsNullOrWhiteSpace(_templateContents))
			{
				Console.WriteLine("[ERROR] Page template '{0}' is empty.", _configData.PageTemplate);
				return false;
			}
			return true;
		}


		/// <summary>
		/// Generates the search data, which is the json file called 'search_index.json' with search data of all pages as well as the docnet_search.htm file in the output.
		/// The search index is written to the root of the output folder.
		/// </summary>
		/// <param name="navigationContext">The navigation context.</param>
		internal void GenerateSearchData(NavigationContext navigationContext)
		{
			GenerateSearchPage(navigationContext);
			GenerateSearchDataIndex(navigationContext);
		}

		internal void CopyThemeToDestination()
		{
			var sourceFolder = Path.Combine(this.ThemeFolder, "Destination");
			if(!Directory.Exists(sourceFolder))
			{
				Console.WriteLine("[WARNING] No theme content found! 'Destination' folder in theme folder '{0}' is missing.", this.ThemeFolder);
				return;
			}
			Utils.DirectoryCopy(sourceFolder, this.Destination, copySubFolders:true);
		}

		internal void CopySourceFoldersToCopy()
		{
			var foldersToCopy = this.SourceFoldersToCopy;
			foreach(var folder in foldersToCopy)
			{
				if(string.IsNullOrWhiteSpace(folder))
				{
					continue;
				}
				var sourceFolderName = Path.Combine(this.Source, folder);
				if(!Directory.Exists(sourceFolderName))
				{
					continue;
				}
				var destinationFolderName = Path.Combine(this.Destination, folder);
				Console.WriteLine("... copying '{0}' to {1}", sourceFolderName, destinationFolderName);
				Utils.DirectoryCopy(sourceFolderName, destinationFolderName, copySubFolders:true);
			}
		}

		internal void ClearDestinationFolder()
		{
			if(string.IsNullOrWhiteSpace(this.Destination))
			{
				Console.WriteLine("[WARNING] Destination is empty. No folder to clear.");
				return;
			}
			if(!Directory.Exists(this.Destination))
			{
				return;
			}
			Directory.Delete(this.Destination, true);
			Directory.CreateDirectory(this.Destination);
		}


		/// <summary>
		/// Generates the index of the search data. this is a json file with per page which has markdown a couple of data elements.
		/// </summary>
		private void GenerateSearchDataIndex(NavigationContext navigationContext)
		{
			var collectedSearchEntries = new List<SearchIndexEntry>();
			this.Pages.CollectSearchIndexEntries(collectedSearchEntries, new NavigatedPath(), navigationContext);
			JObject searchIndex = new JObject(new JProperty("docs",
															new JArray(
																collectedSearchEntries.Select(e=>new JObject(
																	new JProperty("location", e.Location),
																	new JProperty("breadcrumbs", e.BreadCrumbs),
																	new JProperty("keywords", e.Keywords),
																	new JProperty("title", e.Title)))
																)
															));
			File.WriteAllText(Utils.MakeAbsolutePath(this.Destination, "search_index.json"), searchIndex.ToString());
		}


		private void GenerateSearchPage(NavigationContext navigationContext)
		{
			var activePath = new NavigatedPath();
			activePath.Push(this.Pages);
			var searchSimpleElement = new SimpleNavigationElement() {Name = "Search", Value = "Docnet_search.htm", IsIndexElement = false, ParentContainer = this.Pages};
			searchSimpleElement.ContentProducerFunc = e=>@"
					<h1 id=""search"">Search Results</h1>
					<p>
					<form id=""content_search"" action=""docnet_search.htm"">
						<span role= ""status"" aria-live=""polite"" class=""ui-helper-hidden-accessible""></span>
						<input name=""q"" id=""search-query"" type=""text"" class=""search_input search-query ui-autocomplete-input"" placeholder=""Search the Docs"" autocomplete=""off"" autofocus/>
					</form>
					</p>
					<div id=""search-results"">
					<p>Sorry, page not found.</p>
					</div>";
			searchSimpleElement.ExtraScriptProducerFunc = e=> @"
	<script>var base_url = '.';</script>
	<script data-main=""js/search.js"" src=""js/require.js""></script>";
			searchSimpleElement.GenerateOutput(this, activePath, navigationContext);
			activePath.Pop();
		}


		#region Properties
		public string Name
		{
			get { return _configData.Name ?? string.Empty; }
		}

		public string Source
		{
			get
			{
				if(string.IsNullOrWhiteSpace(_docsSourcePath))
				{
					_docsSourcePath = Utils.MakeAbsolutePath(_configFileSourcePath, (string)_configData.Source) ?? ".";
				}
				return _docsSourcePath;
			}
		}

		public string Destination
		{
			get
			{
				if(string.IsNullOrWhiteSpace(_destinationPath))
				{
					_destinationPath = Utils.MakeAbsolutePath(_configFileSourcePath, (string)_configData.Destination) ?? ".";
				}
				return _destinationPath;
			}
		}

		public string IncludeFolder
		{
			get
			{
				string rawIncludeFolder = _configData.IncludeSource;
				return string.IsNullOrWhiteSpace(rawIncludeFolder) ? "Includes" : rawIncludeFolder;
			}
		}

	    public bool ConvertLocalLinks
	    {
	        get
	        {
	            return _configData.ConvertLocalLinks ?? false;
	        }
        }

		public int MaxLevelInToC
		{
			get
			{
				return _configData.MaxLevelInToC ?? 2;
			}
		}

		public bool StripIndexHtm
		{
			get
			{
				return _configData.StripIndexHtm ?? false;
			}
		}

		public string ThemeName
		{
			get
			{
				string rawThemeName = _configData.Theme;
				return string.IsNullOrWhiteSpace(rawThemeName) ? "Default" : rawThemeName;
			}
		}
		
		public string ThemeFolder
		{
			get
			{
				if(string.IsNullOrWhiteSpace(_themeFolder))
				{
					var exeRawFolderUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
					if(exeRawFolderUri == null)
					{
						return string.Empty;	// will end up as an error as themes now aren't found.
					}
					var exeFolder = new Uri(exeRawFolderUri).LocalPath;
					_themeFolder = Path.Combine(Path.Combine(exeFolder, "Themes"), this.ThemeName);
				}
				return _themeFolder;
			}
		}
		
		public string PageTemplateFile
		{
			get
			{
				return Path.Combine(this.ThemeFolder, "PageTemplate.htm");
			}
		}

		public string PageTemplateContents
		{
			get { return _templateContents ?? string.Empty; }
		}

	    public PathSpecification PathSpecification
	    {
	        get
	        {
	            var pathSpecification = PathSpecification.Full;

                var pathSpecificationAsString = (string)_configData.PathSpecification;
	            if (!string.IsNullOrWhiteSpace(pathSpecificationAsString))
	            {
	                if (!Enum.TryParse(pathSpecificationAsString, true, out pathSpecification))
	                {
	                    pathSpecification = PathSpecification.Full;
	                }
	            }

	            return pathSpecification;
	        }
        }

		public NavigationLevel Pages
		{
			get
			{
				if(_pages == null)
				{
					JObject rawPages = _configData.Pages;
					_pages = new NavigationLevel(Source) {Name = "Home", IsRoot = true};
					_pages.Load(rawPages);
				}
				return _pages;
			}
		}

		public string Footer
		{
			get { return _configData.Footer ?? string.Empty; }
		}

		public List<string> SourceFoldersToCopy
		{
			get
			{
				JArray rawFolderNames = _configData.SourceFoldersToCopy;
				if(rawFolderNames == null)
				{
					return new List<string>();
				}
				return rawFolderNames.HasValues ? rawFolderNames.Values<string>().ToList() : new List<string>();
			}
		} 
		#endregion
	}
}