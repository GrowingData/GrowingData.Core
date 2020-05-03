// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Text;

	/// <summary>
	/// Defines the <see cref="UrlString" />
	/// </summary>
	public static class UrlString {
		private static string SafeCharacters = "-_";
		/// <summary>
		/// Take a string, and exclude any character that is not a Letter or Digit
		/// and replace " " with "-"
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string UrlSafeString(this string title) {
			var sb = new StringBuilder();

			foreach (var c in title) {
				if (c == ' ') {
					sb.Append('-');
				} else {
					if (char.IsLetterOrDigit(c)
					|| SafeCharacters.Contains(c)) {
						sb.Append(c);
					}
				}
			}

			return sb.ToString();
		}
	}
}
