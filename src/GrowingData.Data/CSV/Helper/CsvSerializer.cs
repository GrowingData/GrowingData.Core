// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="CsvSerializer" />
	/// </summary>
	public static class CsvSerializer {
		/// <summary>
		/// The Serialize
		/// </summary>
		/// <param name="o">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string Serialize(object o) {
			if (o == null) {
				return string.Empty;
			}
			var type = o.GetType();

			if (type == typeof(string)) {

				// Always double quoted
				var unescaped = (string)o;
				var escapedBuffer = new StringBuilder();
				escapedBuffer.Append('\"');
				foreach (var c in unescaped) {
					var isSpecial = false;
					if (c == '\"') {
						// The CSV RFC (http://tools.ietf.org/html/rfc4180) says that a double 
						// quote appearing between double quotes should be proceeded
						// by a double quote "", rather than \"
						escapedBuffer.Append("\"\"");
						isSpecial = true;
					}
					//if (c == '\t') {
					//	// Replace tabs with \t
					//	escapedBuffer.Append("\\t");
					//	isSpecial = true;
					//}
					//if (c == '\r') {
					//	// Replace CarriageReturn with \r
					//	escapedBuffer.Append("\\r");
					//	isSpecial = true;
					//}
					//if (c == '\n') {
					//	// Replace Newline with \t
					//	escapedBuffer.Append("\\n");
					//	isSpecial = true;
					//}
					if (!isSpecial) {
						escapedBuffer.Append(c);
					}
				}
				escapedBuffer.Append('\"');
				return escapedBuffer.ToString();

			}
			if (type == typeof(DateTime)) {
				return ((DateTime)o).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
				//return ((DateTime)o).ToString("yyyy-MM-dd hh:mm:ss");
			}

			if (type == typeof(bool)) {
				var boolean = (bool)o;
				return boolean ? "1" : "0";
			}

			return Convert.ToString(o);
		}
	}
}
