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
using System.IO;

namespace Docnet
{
	public class Engine
	{
		#region Members
		private CliInput _input;
		private Config _loadedConfig;
		#endregion

		public Engine(CliInput input)
		{
			_input = input;
		}

		public int DoWork()
		{
			_loadedConfig = LoadConfig();
			if(_loadedConfig == null)
			{
				return 1;
			}

			var navigationContext = new NavigationContext
			{
				MaxLevel = _loadedConfig.MaxLevelInToC,
				PathSpecification = _loadedConfig.PathSpecification,
				StripIndexHtm = _loadedConfig.StripIndexHtm
			};

			GeneratePages(navigationContext);
			return 0;
		}


		public Config LoadConfig()
		{
			if(string.IsNullOrWhiteSpace(_input.StartFolder))
			{
				Console.WriteLine("[ERROR] Nothing to do, no start folder specified");
				return null;
			}

			var configFile = Path.Combine(_input.StartFolder, "docnet.json");
			if(!File.Exists(configFile))
			{
				Console.WriteLine("[ERROR] {0} not found.", configFile);
				return null;
			}

			var config = new Config();
			if(!config.Load(configFile))
			{
				Console.WriteLine("Errors occurred, can't continue!");
				return null;
			}

			var navigationContext = new NavigationContext(config.PathSpecification, config.MaxLevelInToC, config.StripIndexHtm);

			var indexElement = config.Pages.GetIndexElement(navigationContext);
			if(indexElement == null)
			{
				Console.WriteLine("[ERROR] Root __index not found. The root navigationlevel is required to have an __index element");
				return null;
			}

			return config;
		}


		/// <summary>
		/// Generates the pages from the md files in the source, using the page template loaded and the loaded config.
		/// </summary>
		/// <returns>true if everything went ok, false otherwise</returns>
		private void GeneratePages(NavigationContext navigationContext)
		{
			if(_input.ClearDestinationFolder)
			{
				Console.WriteLine("Clearing destination folder '{0}'", _loadedConfig.Destination);
				_loadedConfig.ClearDestinationFolder();
			}
			Console.WriteLine("Copying theme '{0}'", _loadedConfig.ThemeName);
			_loadedConfig.CopyThemeToDestination();
			Console.WriteLine("Copying source folders to copy.");
			_loadedConfig.CopySourceFoldersToCopy();
			Console.WriteLine("Generating pages in '{0}'", _loadedConfig.Destination);
			_loadedConfig.Pages.GenerateOutput(_loadedConfig, new NavigatedPath(), navigationContext);
			Console.WriteLine("Generating search index");
			_loadedConfig.GenerateSearchData(navigationContext);
			Console.WriteLine("Done!");
		}
	}
}