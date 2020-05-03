// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;

	/// <summary>
	/// Defines the <see cref="SqlPKAttribute" />
	/// </summary>
	public class SqlPKAttribute : Attribute {
		/// <summary>
		/// Defines the Length
		/// </summary>
		public int Length = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlPKAttribute"/> class.
		/// </summary>
		public SqlPKAttribute() {
		}
	}

	/// <summary>
	/// Defines the <see cref="SqlLongTextAttribute" />
	/// </summary>
	public class SqlLongTextAttribute : Attribute {
		/// <summary>
		/// Defines the Length
		/// </summary>
		public int Length = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlLongTextAttribute"/> class.
		/// </summary>
		public SqlLongTextAttribute() {
		}
	}
}
