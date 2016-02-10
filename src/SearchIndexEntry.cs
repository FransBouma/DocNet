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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Docnet
{
	/// <summary>
	/// Simple class which represents the entry of a page in the search index. 
	/// </summary>
	public class SearchIndexEntry
	{
		private static Regex _wordFinder = new Regex(@"\b[\w']*\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public void Fill(string markDownFromFile, string targetURL, string title, NavigatedPath tocLocaton)
		{
			this.Location = HttpUtility.UrlPathEncode(targetURL);
			this.Title = title;
			this.BreadCrumbs = tocLocaton.CreateBreadCrumbsText(string.Empty).Replace("\"", "").Replace("'", "");
			RetrieveWords(markDownFromFile);
		}


		private void RetrieveWords(string markDownFromFile)
		{
			if(string.IsNullOrWhiteSpace(markDownFromFile))
			{
				this.Keywords = string.Empty;
				return;
			}
			var allMatches = _wordFinder.Matches(markDownFromFile).Cast<Match>();
			var uniqueWords = new HashSet<string>(allMatches.Where(m=>!string.IsNullOrWhiteSpace(m.Value)).Select(m=>TrimSuffix(m.Value)));
			this.Keywords = string.Join(" ", uniqueWords.OrderBy(s=>s).ToArray());
		}

		private static string TrimSuffix(string word)
		{
			int apostropheLocation = word.IndexOf('\'');
			if(apostropheLocation != -1)
			{
				word = word.Substring(0, apostropheLocation);
			}
			return word;
		}


		#region Properties
		public string Location { get; set; }
		public string BreadCrumbs { get; set; }
		public string Keywords { get; set; }
		public string Title { get; set; }
		#endregion
	}
}
