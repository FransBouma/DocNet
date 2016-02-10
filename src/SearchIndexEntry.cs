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
