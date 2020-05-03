// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="StringParser" />
	/// </summary>
	public static class StringWordCount {
		/// <summary>
		/// The ParseIntegerList
		/// </summary>
		/// <param name="integerList">The <see cref="string"/></param>
		/// <param name="seperator">The <see cref="char"/></param>
		/// <returns>The <see cref="List{int}"/></returns>
		public static int WordCount(this string text) {
			if (string.IsNullOrEmpty(text)) {
				return 0;
			}
			int wordCount = 0, index = 0;

			// skip whitespace until first word
			while (index < text.Length && char.IsWhiteSpace(text[index])) {
				index++;
			}
			while (index < text.Length) {
				// check if current char is part of a word
				while (index < text.Length && !char.IsWhiteSpace(text[index])) {
					index++;
				}
				wordCount++;

				// skip whitespace until next word
				while (index < text.Length && char.IsWhiteSpace(text[index])) {
					index++;
				}
			}
			return wordCount;
		}


	}
}
