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

namespace Docnet
{
	public static class Utils
	{
		public static string ConvertMarkdownToHtml(string toConvert, List<Tuple<string, string>> createdAnchorCollector)
		{
			//var parser = new Markdown(new MarkdownOptions() { EmptyElementSuffix = ">"});
			//var toReturn = parser.Transform(toConvert);
			//if(createdAnchorCollector != null)
			//{
			//	createdAnchorCollector.AddRange(parser.CollectedH2AnchorNameTuples);
			//}

			var parser = new MarkdownDeep.Markdown();
			parser.ExtraMode = true;
			parser.GitHubCodeBlocks = true;
			var toReturn = parser.Transform(toConvert);
			return toReturn;
		}


		/// <summary>
		/// Copies directories and files, eventually recursively. From MSDN.
		/// </summary>
		/// <param name="sourceFolderName">Name of the source dir.</param>
		/// <param name="destinationFolderName">Name of the dest dir.</param>
		/// <param name="copySubFolders">if set to <c>true</c> it will recursively copy files/folders.</param>
		public static void DirectoryCopy(string sourceFolderName, string destinationFolderName, bool copySubFolders)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo sourceFolder = new DirectoryInfo(sourceFolderName);
			if(!sourceFolder.Exists)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceFolderName);
			}

			DirectoryInfo[] sourceFoldersToCopy = sourceFolder.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if(!Directory.Exists(destinationFolderName))
			{
				Directory.CreateDirectory(destinationFolderName);
			}

			// Get the files in the directory and copy them to the new location.
			foreach(FileInfo file in sourceFolder.GetFiles())
			{
				file.CopyTo(Path.Combine(destinationFolderName, file.Name), true);
			}
			if(copySubFolders)
			{
				foreach(DirectoryInfo subFolder in sourceFoldersToCopy)
				{
					Utils.DirectoryCopy(subFolder.FullName, Path.Combine(destinationFolderName, subFolder.Name), copySubFolders);
				}
			}
		}



		/// <summary>
		/// Makes toMakeAbsolute an absolute path, if it's not already a rooted path. If it's not a rooted path it's assumed it's relative to rootPath and is combined with that.
		/// </summary>
		/// <param name="rootPath">The root path.</param>
		/// <param name="toMakeAbsolute">To make absolute.</param>
		/// <returns></returns>
		public static string MakeAbsolutePath(string rootPath, string toMakeAbsolute)
		{
			if(string.IsNullOrWhiteSpace(toMakeAbsolute))
			{
				return rootPath;
			}
			if(Path.IsPathRooted(toMakeAbsolute))
			{
				return toMakeAbsolute;
			}
			var rawToReturn = Path.Combine(rootPath, toMakeAbsolute);
			return Path.GetFullPath(rawToReturn).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}


		/// <summary>
		/// Creates the folders in the path specified if they don't exist, recursively
		/// </summary>
		/// <param name="fullPath">The full path.</param>
		public static void CreateFoldersIfRequired(string fullPath)
		{
			string folderToCheck = Path.GetDirectoryName(fullPath);
			if(string.IsNullOrWhiteSpace(folderToCheck))
			{
				// nothing to do, no folder to emit
				return;
			}
			if(!Directory.Exists(folderToCheck))
			{
				Directory.CreateDirectory(folderToCheck);
			}
		}


		/// <summary>
		/// Creates a relative path to get from fromPath to toPath. If one of them is empty, the emptystring is returned. If there's no common path, toPath is returned.
		/// </summary>
		/// <param name="fromPath">From path.</param>
		/// <param name="toPath">To path.</param>
		/// <returns></returns>
		/// <remarks>Only works with file paths, which is ok, as it's used to create the {{Path}} macro.</remarks>
		public static string MakeRelativePath(string fromPath, string toPath)
		{
			var fromPathToUse = fromPath;
			if(string.IsNullOrEmpty(fromPathToUse))
			{
				return string.Empty;
			}
			var toPathToUse = toPath;
			if(string.IsNullOrEmpty(toPathToUse))
			{
				return string.Empty;
			}
			if(fromPathToUse.Last() != Path.DirectorySeparatorChar)
			{
				fromPathToUse += Path.DirectorySeparatorChar;
			}
			if(toPathToUse.Last() != Path.DirectorySeparatorChar)
			{
				toPathToUse += Path.DirectorySeparatorChar;
			}

			var fromUri = new Uri(Uri.UnescapeDataString(Path.GetFullPath(fromPathToUse)));
			var toUri = new Uri(Uri.UnescapeDataString(Path.GetFullPath(toPathToUse)));

			if(fromUri.Scheme != toUri.Scheme)
			{
				// path can't be made relative.
				return toPathToUse;
			}

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if(toUri.Scheme.ToUpperInvariant() == "FILE")
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}
	}
}
