// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="DbSchemaReader" />
	/// </summary>
	public abstract class DbSchemaReader {
		/// <summary>
		/// Defines the _connectionFactory
		/// </summary>
		private Func<DbConnection> _connectionFactory;

		/// <summary>
		/// Gets the TypeConverter
		/// </summary>
		public abstract DbTypeConverter TypeConverter { get; }

		/// <summary>
		/// A query that returns the following columns:
		///		table_schema, table_name, column_name, data_type
		///	And the following parameters:
		///		@Schema, @Table
		///	
		/// Example (postgresql):
		///		SELECT table_schema, table_name, column_name, data_type
		///		FROM information_schema.columns C
		///		WHERE table_schema NOT IN ('information_schema', 'pg_catalog')
		///		AND (table_schema = @Schema OR @Schema IS NULL)
		///		AND (table_name = @Table OR @Table IS NULL)
		///		ORDER BY table_schema, ordinal_position
		/// 
		/// </summary>
		public abstract string SchemaSql { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DbSchemaReader"/> class.
		/// </summary>
		/// <param name="cn">The <see cref="Func{DbConnection}"/></param>
		public DbSchemaReader(Func<DbConnection> cn) {
			_connectionFactory = cn;
		}

		/// <summary>
		/// The GetTables
		/// </summary>
		/// <returns>The <see cref="List{SqlTable}"/></returns>
		public List<SqlTable> GetTables() {
			return GetTables(null, null);
		}

		/// <summary>
		/// The GetTables
		/// </summary>
		/// <param name="schema">The <see cref="string"/></param>
		/// <returns>The <see cref="List{SqlTable}"/></returns>
		public List<SqlTable> GetTables(string schema) {
			return GetTables(schema, null);
		}

		/// <summary>
		/// The GetTables
		/// </summary>
		/// <param name="schema">The <see cref="string"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="List{SqlTable}"/></returns>
		public List<SqlTable> GetTables(string schema, string table) {


			var tables = new Dictionary<string, SqlTable>();
			using (var cn = _connectionFactory()) {
				using (var cmd = cn.CreateCommand()) {
					cmd.CommandText = SchemaSql;

					cmd.Parameters.Add(GetParameter(cmd, "Schema", schema));
					cmd.Parameters.Add(GetParameter(cmd, "Table", table));

					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							var tableName = (string)reader["table_name"];
							var tableSchema = (string)reader["table_schema"];
							var columnName = (string)reader["column_name"];
							var columnType = (string)reader["data_type"];

							SqlTable tbl = null;

							if (!tables.ContainsKey(tableName)) {
								tbl = new SqlTable(tableName, tableSchema);
								tables[tableName] = tbl;
							} else {
								tbl = tables[tableName];
							}

							var type = SimpleDbType.Get(columnType);
							if (type != null) {

								tbl.AddColumn(new SqlColumn(columnName, type.DotNetType));
							}
						}
					}
				}
			}
			return tables.Values.OrderBy(t => t.DatasetId).ThenBy(t => t.TableId).ToList();
		}

		/// <summary>
		/// The GetParameter
		/// </summary>
		/// <param name="cmd">The <see cref="DbCommand"/></param>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="value">The <see cref="object"/></param>
		/// <returns>The <see cref="DbParameter"/></returns>
		public DbParameter GetParameter(DbCommand cmd, string name, object value) {
			var p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = value == null ? DBNull.Value : value;
			return p;
		}
	}
}
