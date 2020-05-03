// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Text.RegularExpressions;

	/// <summary>
	/// Defines the <see cref="StringLike" />
	/// </summary>
	public static class StringLike {
		/// <summary>
		/// Compares the string against a given pattern.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
		/// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
		public static bool Like(this string str, string pattern) {
			return new Regex(
				"^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
				RegexOptions.IgnoreCase | RegexOptions.Singleline
			).IsMatch(str);
		}

		/// <summary>
		/// Removes the text between two characters.
		///		E.g "Some text(not text)".RemoveBetween('(', ')')
		///			=> "Some text "
		/// </summary>
		/// <param name="s"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public static string RemoveBetween(this string s, char begin, char end) {
			var regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
			return regex.Replace(s, string.Empty);
		}
	}
}
