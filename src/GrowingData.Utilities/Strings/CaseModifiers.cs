// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="CaseModifiers" />
	/// </summary>
	public static class CaseModifiers {
		/// <summary>
		/// The ToUnderscoreCase
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string ToDnsSafeLabel(this string name) {
			if (name == null) {
				return null;
			}
			// Check to see if its all caps, because if it is then
			// we dont want to do anything with it.
			if (name.Where(c => char.IsLetter(c)).Where(c => char.IsLower(c)).Count() == 0) {
				return name.ToLower();
			}

			var renamed = new StringBuilder();
			for (var i = 0; i < name.Length; i++) {
				if (name[i] == ' ' || name[i] == '_' || name[i] == '-') {
					renamed.Append("-");
					continue;
				}
				if (!char.IsLetterOrDigit(name[i])) {
					// Ignore invalid characters
					continue;
				}
				if (i > 0 && char.IsUpper(name[i])) {
					// Add a space where we have a change inCase
					//	e.g. "helloWorld" => "hello_world"
					renamed.Append("_");
				}

				renamed.Append(char.ToLower(name[i]));
			}
			return renamed.ToString();
		}

		/// <summary>
		/// The ToUnderscoreCase
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string ToDatabaseSafeLabel(this string name) {
			if (name == null) {
				return null;
			}
			return ToDnsSafeLabel(name).Replace("-", "_");
		}
	}
}
