// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;

	/// <summary>
	/// Defines the <see cref="SqlTableAttribute" />
	/// </summary>
	public class SqlTableAttribute : Attribute {
		/// <summary>
		/// Defines the TableName
		/// </summary>
		public string TableName;

		/// <summary>
		/// Defines the SchemaName
		/// </summary>
		public string SchemaName;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTableAttribute"/> class.
		/// </summary>
		/// <param name="schema">The <see cref="string"/></param>
		/// <param name="table">The <see cref="string"/></param>
		public SqlTableAttribute(string schema, string table) {
			SchemaName = schema;
			TableName = table;
		}
	}
}
