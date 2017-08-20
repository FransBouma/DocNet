using System;
using System.Text.RegularExpressions;

namespace Docnet
{
	internal static class StringExtensions
	{
		public static string ApplyUrlFormatting(this string value, UrlFormatting urlFormatting)
		{
			var finalValue = string.Empty;
			string replacementValue = null;

			switch (urlFormatting)
			{
				case UrlFormatting.None:
					finalValue = value;
					break;

				case UrlFormatting.Strip:
					replacementValue = string.Empty;
					break;

				case UrlFormatting.Dashes:
					replacementValue = "-";
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(urlFormatting), urlFormatting, null);
			}

			if (replacementValue != null)
			{
				var doubleReplacementValue = replacementValue + replacementValue;
				var regEx = new Regex("[^a-zA-Z0-9 -]");

				var splitted = value.Split(new[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
				for(var i = 0; i < splitted.Length; i++)
				{
					var splittedValue = splitted[i];
					if (string.Equals(splittedValue, ".") || string.Equals(splittedValue, ".."))
					{
						continue;
					}

					splittedValue = regEx.Replace(splittedValue, replacementValue).Replace(" ", replacementValue);

					if (!string.IsNullOrEmpty(replacementValue))
					{
						while (splittedValue.Contains(doubleReplacementValue))
						{
							splittedValue = splittedValue.Replace(doubleReplacementValue, replacementValue);
						}
					}
					splitted[i] = splittedValue.ToLower();
				}

				finalValue = string.Join("/", splitted);
			}
			return finalValue;
		}
	}
}