// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Data.Common;
	using System.IO;
	using GrowingData.Data;

	/// <summary>
	/// Defines the <see cref="DbBulkInsert" />
	/// </summary>
	public abstract class DbBulkInsert {
		/// <summary>
		/// The CreateTable
		/// </summary>
		/// <param name="tbl">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public abstract bool CreateTable(SqlTable tbl);

		/// <summary>
		/// The CreateSchemaIfRequired
		/// </summary>
		/// <param name="targetSchema">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public abstract bool CreateSchemaIfRequired(string targetSchema);

		/// <summary>
		/// The BulkInsert
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="CsvDbDataReader"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool BulkInsert(SqlTable schema, CsvDbDataReader reader) {
			return BulkInsert(schema, reader, null);
		}

		/// <summary>
		/// The BulkInsert
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <param name="notify">The <see cref="Action{DbDataReader}"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public abstract bool BulkInsert(SqlTable schema, DbDataReader reader, Action<DbDataReader> notify);

		/// <summary>
		/// The ModifySchema
		/// </summary>
		/// <param name="oldSchema">The <see cref="SqlTable"/></param>
		/// <param name="newSchema">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public abstract bool ModifySchema(SqlTable oldSchema, SqlTable newSchema);

		/// <summary>
		/// The BulkInsert
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool BulkInsert(SqlTable schema, DbDataReader reader) {
			return BulkInsert(schema, reader, null);
		}

		/// <summary>
		/// The GetDbSchema
		/// </summary>
		/// <param name="schema">The <see cref="string"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public abstract SqlTable GetDbSchema(string schema, string table);

		/// <summary>
		/// Initializes a new instance of the <see cref="DbBulkInsert"/> class.
		/// </summary>
		protected DbBulkInsert() {
		}


	}
}
