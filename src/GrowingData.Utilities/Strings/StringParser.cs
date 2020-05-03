// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="StringParser" />
	/// </summary>
	public static class StringParser {
		/// <summary>
		/// The ParseIntegerList
		/// </summary>
		/// <param name="integerList">The <see cref="string"/></param>
		/// <param name="seperator">The <see cref="char"/></param>
		/// <returns>The <see cref="List{int}"/></returns>
		public static List<int> ParseIntegerList(this string integerList, char seperator = ',') {
			if (string.IsNullOrEmpty(integerList)) {
				return new List<int>();
			}
			return integerList.Split(seperator).Select(x => int.Parse(x)).ToList();
		}

		/// <summary>
		/// The ParseDoubleList
		/// </summary>
		/// <param name="integerList">The <see cref="string"/></param>
		/// <param name="seperator">The <see cref="char"/></param>
		/// <returns>The <see cref="List{double}"/></returns>
		public static List<double> ParseDoubleList(this string integerList, char seperator = ',') {
			if (string.IsNullOrEmpty(integerList)) {
				return new List<double>();
			}
			return integerList.Split(seperator).Select(x => double.Parse(x)).ToList();
		}


		public static string CleanNonAscii(this string input, string skipchars) {
			var cleaned = input
				.Where(c => (int)c < 128)
				.Where(c => !skipchars.Contains(c))
				.ToArray();

			return new string(cleaned);

		}
	}
}
