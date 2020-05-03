// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Data.SqlClient;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="SqlBulkInsert" />
	/// </summary>
	public class SqlBulkInsert : DbBulkInsert {
		/// <summary>
		/// The GetPartitions
		/// </summary>
		/// <param name="yearStart">The <see cref="int"/></param>
		/// <param name="yearEnd">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string GetPartitions(int yearStart, int yearEnd) {
			var months = new int[12];
			var buffer = new List<string>();
			for (var y = yearStart; y < yearEnd; y++) {
				var line = string.Join(",",
					months
						.Select((x, i) => i + 1)
						.Select(m => y.ToString() + (m < 10 ? "0" + m.ToString() : m.ToString())));
				buffer.Add("						" + line);

			}
			return string.Join(",\r\n", buffer);
		}

		public int CallbackRows = 100;
		public int BatchSize = 1000;
		public int Timeout = 9999999;

		/// <summary>
		/// Defines the _connectionFactory
		/// </summary>
		protected Func<SqlConnection> _connectionFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlBulkInsert"/> class.
		/// </summary>
		/// <param name="targetConnection">The <see cref="Func{DbConnection}"/></param>
		public SqlBulkInsert(Func<DbConnection> targetConnection)
			: base() {

			_connectionFactory = () => {
				return targetConnection() as SqlConnection;
			};
		}

		/// <summary>
		/// The CreateSchemaIfRequired
		/// </summary>
		/// <param name="targetSchema">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool CreateSchemaIfRequired(string targetSchema) {
			var sql = $@"
				IF NOT EXISTS ( SELECT  * FROM sys.schemas WHERE name = N'{targetSchema}' ) 
					EXEC('CREATE SCHEMA [{targetSchema}] AUTHORIZATION [dbo]');";

			using (var cn = _connectionFactory()) {
				cn.ExecuteNonQuery(sql, null);
				return true;
			}
		}

		/// <summary>
		/// The GetDbSchema
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public override SqlTable GetDbSchema(string schemaName, string tableName) {

			try {
				var table = new SqlTable(tableName, schemaName);

				using (var cn = _connectionFactory()) {
					var sql = $"SELECT TOP 1 * FROM [{schemaName}].[{tableName}]";
					using (var reader = cn.ExecuteReader(sql, null)) {

						for (var i = 0; i < reader.FieldCount; i++) {
							var col = new SqlColumn(reader.GetName(i), reader.GetFieldType(i));
							table.AddColumn(col);
						}
					}

				}
				return table;
			} catch (Exception ex) {
				if (ex.Message.StartsWith("Invalid object name")) {
					// The table doesn't exist
					return null;
				}
				throw ex;
			}
		}

		/// <summary>
		/// The Copy_SqlRowsCopied
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="e">The <see cref="SqlRowsCopiedEventArgs"/></param>
		private void Copy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) {
		}

		/// <summary>
		/// The BulkInsert
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <param name="notify">The <see cref="Action{DbDataReader}"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool BulkInsert(SqlTable schema, DbDataReader reader, Action<DbDataReader> notify) {
			using (var cn = _connectionFactory()) {
				using (var copy = new SqlBulkCopy(cn)) {
					var existingTable = GetDbSchema(schema.DatasetId, schema.TableId);

					if (existingTable == null) {
						CreateTable(schema);
					} else {
						ModifySchema(existingTable, schema);
					}
					var dbTable = GetDbSchema(schema.DatasetId, schema.TableId);
					var destinationColumns = dbTable.Columns;


					for (var i = 0; i < reader.FieldCount; i++) {
						var column = reader.GetName(i);
						var columnType = reader.GetFieldType(i);
						if (columnType == null) {
							continue;
						}
						var sourceOrdinal = i;
						var destinationOrdinal = destinationColumns.FindIndex(x => x.ColumnName == column);

						if (destinationOrdinal == -1) {
							var msg = string.Format("Unable to resolve column mapping, column: {0} was not found in destination table {1}",
								column,
								dbTable.TableId
							);
							throw new Exception(msg);
						}
						copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, destinationOrdinal));
					}

					copy.DestinationTableName = string.Format("[{0}].[{1}]", dbTable.DatasetId, dbTable.TableId);

					copy.BatchSize = BatchSize;
					copy.NotifyAfter = CallbackRows;
					copy.SqlRowsCopied += (sender, e) => {
						notify(reader);
					};
					copy.BulkCopyTimeout = Timeout;

					copy.WriteToServer(reader);
				}

			}
			return true;
		}

		/// <summary>
		/// The SyncSchema
		/// </summary>
		/// <param name="schemaName">The <see cref="string"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public SqlTable SyncSchema(string schemaName, string tableName, DbDataReader reader) {
			var existingTable = GetDbSchema(schemaName, tableName);
			var readerTable = new SqlTable(tableName, schemaName);

			for (var i = 0; i < reader.FieldCount; i++) {
				var columnName = reader.GetName(i);
				var columnType = reader.GetFieldType(i);
				if (columnType == null) {
					continue;
				}
				var column = new SqlColumn(columnName, columnType);
				readerTable.AddColumn(column);

			}

			if (existingTable == null) {
				CreateTable(readerTable);
				existingTable = GetDbSchema(schemaName, tableName);
			}

			ModifySchema(existingTable, readerTable);
			var dbTable = GetDbSchema(schemaName, tableName);
			return dbTable;
		}

		/// <summary>
		/// The CreateTable
		/// </summary>
		/// <param name="table">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool CreateTable(SqlTable table) {

			var ddl = new System.Text.StringBuilder();
			ddl.Append($"CREATE TABLE [{table.DatasetId}].[{table.TableId}] (\r\n");


			foreach (var c in table.Columns) {
				var type = DbTypesSqlAzure.SqlServer.GetCreateColumnDefinition(c.SimpleType);

				if (c.ColumnName == "_id_") {
					ddl.Append($"	\"{c.ColumnName}\" CHAR(12) NOT NULL,\n");
					continue;
				}

				if (c.ColumnName == "_at_") {
					ddl.Append($"	\"{c.ColumnName}\" DATETIME NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == "_part_") {
					ddl.Append($"	\"{c.ColumnName}\" INT NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == "_source_") {
					ddl.Append($"	\"{c.ColumnName}\" VARCHAR(100) NOT NULL,\n");
					continue;
				}
				if (c.ColumnName == "_app_") {
					ddl.Append($"	\"{c.ColumnName}\" VARCHAR(100) NOT NULL,\n");
					continue;
				}

				ddl.Append($"	\"{c.ColumnName}\" {type} NULL,\n");
			}

			ddl.Length -= 2;
			ddl.Append("\n)");

			try {
				var success = ExecuteCommand(ddl.ToString());
				return success;
			} catch (Exception ex) {

				// SQL Azure Data Warehouse defaults to Column Store indexes which don't support some
				// get data types, so if thats the case lets retry the command creating the table as a
				// heap (row store) table instead.
				if (ex.Message.Contains("columnstore")) {
					ddl.Append(" WITH(HEAP)");
					var success = ExecuteCommand(ddl.ToString());
					return true;
				}

				var errorDetails = $"{DateTime.Now}\tSqlBulkInsert.CreateTable\t{ex.Message}: {ex.StackTrace}\r\n\r\n";

				System.Diagnostics.Debug.WriteLine($"\t{ddl}");
				return false;
			}
		}

		public bool DropTable(string schemaName, string tableName) {

			try {
				using (var cn = _connectionFactory()) {
					var sql = $"DROP TABLE [{schemaName}].[{tableName}]";
					using (var cmd = new SqlCommand(sql, cn)) {
						cmd.ExecuteNonQuery();
						return true;
					}
				}
			} catch  {
				return false;
			}
		}

		/// <summary>
		/// The ExecuteCommand
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public bool ExecuteCommand(string sql) {
			using (var cn = _connectionFactory()) {
				using (var cmd = new SqlCommand(sql, cn)) {
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
		/// The ModifySchema
		/// </summary>
		/// <param name="fromTbl">The <see cref="SqlTable"/></param>
		/// <param name="toTbl">The <see cref="SqlTable"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool ModifySchema(SqlTable fromTbl, SqlTable toTbl) {
			var fromColumns = fromTbl.Columns;
			var toColumns = toTbl.Columns;

			foreach (var c in toColumns) {
				var existing = fromColumns
					.FirstOrDefault(x => SameColumn(x, c));

				// Add the new column...
				if (existing == null) {
					var type = DbTypesSqlAzure.SqlServer.GetCreateColumnDefinition(c.SimpleType);
					var ddl = $"ALTER TABLE \"{fromTbl.DatasetId}\".\"{fromTbl.TableId}\" ADD \"{c.ColumnName}\" {type} NULL";
					fromTbl.AddColumn(c);
					try {
						ExecuteCommand(ddl);
					} catch (Exception ex) {

						var errorDetails = $"{DateTime.Now}\tSqlBulkInsert.ModifySchema\t{ex.Message}: {ex.StackTrace}\r\n\r\n";

						System.Diagnostics.Debug.WriteLine(errorDetails);
						System.Diagnostics.Debug.WriteLine($"\t{ddl}");

						throw;
					}
				} else {
					if (c.SimpleType.Code != existing.SimpleType.Code) {
						var type = DbTypesSqlAzure.SqlServer.GetCreateColumnDefinition(c.SimpleType);
						var newType = SimpleDbType.Get(typeof(string));
						var pgType = DbTypesSqlAzure.Get(newType);
						var ddl = $"ALTER TABLE \"{fromTbl.DatasetId}\".\"{fromTbl.TableId}\" ALTER COLUMN \"{c.ColumnName}\" {type}";


						try {
							ExecuteCommand(ddl);
						} catch (Exception ex) {

							var errorDetails = $"{DateTime.Now}\tSqlBulkInsert.ModifySchema\t{ex.Message}: {ex.StackTrace}\r\n\r\n";

							System.Diagnostics.Debug.WriteLine(errorDetails);
							System.Diagnostics.Debug.WriteLine($"\t{ddl}");

							throw;
						}

					}
				}

			}

			return true;
		}
	}
}
