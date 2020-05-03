// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="SqlTypeMap" />
	/// </summary>
	public class SqlTypeMap {
		/// <summary>
		/// Defines the SqlType
		/// </summary>
		public string SqlType;

		/// <summary>
		/// Defines the DotNetType
		/// </summary>
		public Type DotNetType;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTypeMap"/> class.
		/// </summary>
		/// <param name="dotNet">The <see cref="Type"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		public SqlTypeMap(Type dotNet, string sql) {
			DotNetType = dotNet;
			SqlType = sql;
		}

		/// <summary>
		/// The UnderscoreCase
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string UnderscoreCase(string name) {
			// Check to see if its all caps, because if it is then
			// we dont want to do anything with it.
			if (name.Where(c => char.IsLower(c)).Count() == 0) {
				return name;
			}



			var renamed = new StringBuilder();
			for (var i = 0; i < name.Length; i++) {
				if (i > 0 && char.IsUpper(name[i])) {
					renamed.Append("_");
				}
				renamed.Append(char.ToLower(name[i]));
			}
			return renamed.ToString();
		}
	}
}
