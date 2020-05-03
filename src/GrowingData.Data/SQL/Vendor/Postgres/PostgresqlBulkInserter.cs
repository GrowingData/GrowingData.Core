// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.IO;
	using System.Linq;
	using Npgsql;
	using NpgsqlTypes;

	/// <summary>
	/// Defines the <see cref="PostgresqlBulkInserter" />
	/// </summary>
	public class PostgresqlBulkInserter : DbBulkInsert {
		/// <summary>
		/// Defines the _connectionFactory
		/// </summary>
		protected Func<NpgsqlConnection> _connectionFactory;

		/// <summary>
		/// Defines the _provider
		/// </summary>
		protected PostgresQueryProvider _provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="PostgresqlBulkInserter"/> class.
		/// </summary>
		/// <param name="targetConnection">The <see cref="Func{DbConnection}"/></param>
		public PostgresqlBulkInserter(Func<DbConnection> targetConnection)
			: base() {
			_connectionFactory = () => {
				return (NpgsqlConnection)targetConnection();
			};
			_provider = new PostgresQueryProvider();
		}

		/// <summary>
		/// The GetDbSchema
		/// </summary>
		/// <param name="schema">The <see cref="string"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public override SqlTable GetDbSchema(string schema, string table) {
			var schemaReader = new PostgresqlDbSchemaReader(_connectionFactory);
			return schemaReader.GetTables(schema, table).FirstOrDefault();
		}


		/// <summary>
		/// The BulkInsert
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <param name="callback">The <see cref="Action{DbDataReader}"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool BulkInsert(SqlTable schema, DbDataReader reader, Action<DbDataReader> callback) {
			// Read the first row to get the information on the schema

			if (reader.Read()) {

				var existingTable = GetDbSchema(schema.DatasetId, schema.TableId);

				var pgTypes = new List<NpgsqlDbType>();
				for (var i = 0; i < reader.FieldCount; i++) {
					var columnName = reader.GetName(i);
					var columnType = reader.GetFieldType(i);
					var column = new SqlColumn(columnName, columnType);
					schema.AddColumn(column);
					var type = PostgresqlTypeConverter.Get(column.SimpleType);

					if (type == null) {
						throw new Exception($"Unable to load Postgres type for type: {column.SimpleType.Code}, Column: {column.ColumnName}");
					}
					pgTypes.Add(type.PostgresqlDbType);
				}

				if (existingTable == null) {
					CreateTable(schema);
				} else {
					ModifySchema(existingTable, schema);
				}

				var table = GetDbSchema(schema.DatasetId, schema.TableId);



				var rowCount = 0L;
				using (var cn = _connectionFactory()) {
					using (var writer = cn.BeginBinaryImport(CopyBinaryCommand(table))) {
						do {

							writer.StartRow();
							var columns = table.Columns;
							for (var i = 0; i < columns.Count; i++) {
								var col = columns[i];
								var val = reader.GetValue(i);
								if (val == DBNull.Value) {
									writer.WriteNull();
								} else {
									writer.Write(val, pgTypes[i]);
								}
							}
							if (callback != null) {
								callback(reader);
							}
							rowCount++;
						} while (reader.Read());
					}
				}

			}
			return true;
		}

		/// <summary>
		/// The CopyBinaryCommand
		/// </summary>
		/// <param name="table">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string CopyBinaryCommand(SqlTable table) {
			var columns = string.Join(",\n\t", table.Columns.Select(x => $"\"{x.ColumnName}\""));
			return $"COPY \"{table.DatasetId}\".\"{table.TableId}\"(\n\t{columns}\n) FROM STDIN BINARY";
		}

		/// <summary>
		/// The CopyTextCommand
		/// </summary>
		/// <param name="table">The <see cref="SqlTable"/></param>
		/// <param name="seperator">The <see cref="char"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string CopyTextCommand(SqlTable table, char seperator) {
			var columns = string.Join(",\n\t", table.Columns.Select(x => $"\"{x.ColumnName.ToLower()}\""));
			return $"COPY \"{table.DatasetId}\".\"{table.TableId}\"(\n\t{columns}\n) FROM STDIN WITH (FORMAT CSV, DELIMITER '{seperator}', QUOTE '\"', NULL '')";
		}

		/// <summary>
		/// The CreateTable
		/// </summary>
		/// <param name="tbl">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool CreateTable(SqlTable tbl) {
			var ddl = _provider.CreateTableSql($"{tbl.DatasetId}.{tbl.TableId}", tbl.Columns, null);

			return ExecuteCommand(ddl.ToString());
		}

		/// <summary>
		/// The ExecuteCommand
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool ExecuteCommand(string sql) {
			using (var cn = _connectionFactory()) {
				using (var cmd = new NpgsqlCommand(sql, cn)) {
					cmd.ExecuteNonQuery();
				}
			}
			return true;
		}

		/// <summary>
		/// The SameColumn
		/// </summary>
		/// <param name="a">The <see cref="SqlColumn"/></param>
		/// <param name="b">The <see cref="SqlColumn"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool SameColumn(SqlColumn a, SqlColumn b) {
			return a.ColumnName.ToLowerInvariant() == b.ColumnName.ToLowerInvariant();
		}

		/// <summary>
		/// The CreateSchemaIfRequired
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool CreateSchemaIfRequired(string schemaName) {
			try {
				var cmd = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
				ExecuteCommand(cmd);
			} catch (Exception ex) {
				if (!ex.Message.Contains("pg_namespace_nspname_index")) {
					// This can break when there is a race condition in creating the schema
					// which means the schema has been cvreated by anotehr user so all is well.
					// https://stackoverflow.com/questions/29900845/create-schema-if-not-exists-raises-duplicate-key-error
					throw ex;
				}
			}
			return true;
		}

		/// <summary>
		/// The ModifySchema
		/// </summary>
		/// <param name="fromTbl">The <see cref="SqlTable"/></param>
		/// <param name="toTbl">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool ModifySchema(SqlTable fromTbl, SqlTable toTbl) {
			var toColumns = toTbl.Columns;
			var fromColumns = fromTbl.Columns;
			foreach (var c in toColumns) {
				var existing = fromColumns
					.FirstOrDefault(x => SameColumn(x, c));

				// Add the new column...
				if (existing == null) {
					fromTbl.AddColumn(c);
					var pgType = PostgresqlTypeConverter.Get(c.SimpleType);
					var ddl = $"ALTER TABLE \"{fromTbl.DatasetId}\".\"{fromTbl.TableId}\" ADD \"{c.ColumnName}\" {pgType.CreateColumnDefinition} NULL";
					ExecuteCommand(ddl);
				} else {
					if (c.SimpleType != existing.SimpleType) {
						var newType = SimpleDbType.Get(typeof(string));
						var pgType = PostgresqlTypeConverter.Get(newType);
						var ddl = $"ALTER TABLE \"{fromTbl.DatasetId}\".\"{fromTbl.TableId}\" ALTER COLUMN \"{c.ColumnName}\" TYPE {pgType.CreateColumnDefinition}";

						ExecuteCommand(ddl);

					}
				}
			}

			return true;
		}
	}
}
