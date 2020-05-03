// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// Defines the <see cref="StringLineReader" />
	/// </summary>
	public static class StringLineReader {
		/// <summary>
		/// The Lines
		/// </summary>
		/// <param name="text">The <see cref="string"/></param>
		/// <returns>The <see cref="IEnumerable{string}"/></returns>
		public static IEnumerable<string> Lines(this string text) {
			using (var sr = new StringReader(text)) {
				string line;
				while ((line = sr.ReadLine()) != null) {
					yield return line;
				}
			}
		}
	}
}
