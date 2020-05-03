// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

using System;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities;
using Newtonsoft.Json;

namespace GrowingData.Data {

	/// <summary>
	/// Defines the <see cref="SqlTable" />
	/// </summary>
	public class SqlTable {
		/// <summary>
		/// Defines the TableName
		/// </summary>
		[JsonProperty]
		public virtual string TableId { get; private set; }

		/// <summary>
		/// Defines the SchemaName
		/// </summary>
		[JsonProperty]
		public virtual string DatasetId { get; private set; }

		/// <summary>
		/// Gets the Columns
		/// </summary>
		[JsonProperty]
		public virtual List<SqlColumn> Columns { get; private set; }

		/// <summary>
		/// Gets the Columns
		/// </summary>
		[JsonProperty]
		public virtual List<string> PrimaryKeys { get; private set; }

		/// <summary>
		/// The AddColumn
		/// </summary>
		/// <param name="col">The <see cref="SqlColumn"/></param>
		public void AddColumn(SqlColumn col) {
			Columns.Add(col);
		}

		public void SetPrimaryKeys(List<string> keys) {
			// Make sure that all the keys exists
			foreach (var k in keys) {
				if (!Columns.Exists(x => x.ColumnName == k)) {
					throw new Exception($"Unable to create primary key for: {k}, it does not exist in table {DatasetId}.{TableId}");
				}
			}

			PrimaryKeys = keys.ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTable"/> class.
		/// </summary>
		public SqlTable() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTable"/> class.
		/// </summary>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="schemaName">The <see cref="string"/></param>
		public SqlTable(string tableName, string schemaName) {
			TableId = tableName;
			DatasetId = schemaName;
			Columns = new List<SqlColumn>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTable"/> class.
		/// </summary>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="columns">The <see cref="SqlColumn[]"/></param>
		public SqlTable(string tableName, string schemaName, params SqlColumn[] columns) {
			TableId = tableName;
			DatasetId = schemaName;
			Columns = columns.ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlTable"/> class.
		/// </summary>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="columns">The <see cref="List{SqlColumn}"/></param>
		public SqlTable(string tableName, string schemaName, List<SqlColumn> columns) {
			TableId = tableName;
			DatasetId = schemaName;
			Columns = columns;
		}

		/// <summary>
		/// Clones an SqlTable
		/// </summary>
		/// <param name="table"></param>
		public SqlTable(SqlTable table) {
			TableId = table.TableId;
			DatasetId = table.DatasetId;
			Columns = new List<SqlColumn>();
			PrimaryKeys = new List<string>();
			foreach (var c in table.Columns) {
				Columns.Add(new SqlColumn(c.ColumnName, c.DotNetType));
			}
			foreach (var k in table.PrimaryKeys) {
				PrimaryKeys.Add(k);
			}
		}

		/// <summary>
		/// The AddColumn
		/// </summary>
		/// <param name="columnName">The <see cref="string"/></param>
		/// <param name="type">The <see cref="Type"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public SqlTable AddColumn(string columnName, Type type) {
			Columns.Add(new SqlColumn(columnName, type));
			return this;
		}

		public SqlTable Rename(string datasetId, string tableId) {
			return new SqlTable(tableId, datasetId, Columns);
		}

		/// <summary>
		/// The GetSchemaHash
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public string GetSchemaHash() {
			return GetSchemaHash(Columns);
		}
		/// <summary>
		/// The GetSchemaHash
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public static string GetSchemaHash(List<SqlColumn> columns) {
			var columnStrings = columns.Select(c => $"{c.ColumnName}|{c.DotNetType.Name}");
			var schemaString = string.Join(",", columnStrings);
			return schemaString.HashStringMD5();
		}
	}
}
