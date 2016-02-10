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
			GeneratePages();
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
			if(config.Pages.IndexElement == null)
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
		private void GeneratePages()
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
			_loadedConfig.Pages.GenerateOutput(_loadedConfig, new NavigatedPath());
			Console.WriteLine("Generating search index");
			_loadedConfig.GenerateSearchData();
			Console.WriteLine("Done!");
		}
	}
}