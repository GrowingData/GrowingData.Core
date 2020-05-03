// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Data.Common;

	/// <summary>
	/// Defines the <see cref="SqlServerSchemaReader" />
	/// </summary>
	public class SqlServerSchemaReader : DbSchemaReader {
		//private SqlConnection _cn;
		/// <summary>
		/// Initializes a new instance of the <see cref="SqlServerSchemaReader"/> class.
		/// </summary>
		/// <param name="cn">The <see cref="Func{DbConnection}"/></param>
		public SqlServerSchemaReader(Func<DbConnection> cn) : base(cn) {
		}

		/// <summary>
		/// Gets the SchemaSql
		/// </summary>
		public override string SchemaSql {
			get {
				return @"
					SELECT table_schema, table_name, column_name, data_type
					FROM information_schema.columns C
					WHERE	(table_schema = @Schema OR @Schema IS NULL)
					AND		(table_name = @Table OR @Table IS NULL)
					ORDER BY table_schema, ordinal_position
				";
			}
		}

		/// <summary>
		/// Gets the TypeConverter
		/// </summary>
		public override DbTypeConverter TypeConverter {
			get { return DbTypeConverter.SqlServer; }
		}
	}
}
