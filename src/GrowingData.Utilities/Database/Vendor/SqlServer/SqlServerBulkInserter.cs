using System;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

using GrowingData.Utilities;
using GrowingData.Utilities.Csv;

namespace GrowingData.Utilities.Database {

	public class SqlServerBulkInserter : DbBulkInserter {
		//private SqlConnection _cn;

		protected Func<SqlConnection> _connectionFactory;

		public SqlServerBulkInserter(Func<DbConnection> targetConnection, string targetSchema, string targetTable)
			: base(targetConnection, targetSchema, targetTable) {

			_connectionFactory = () => {
				return targetConnection() as SqlConnection;
			};


			// Make sure that the schema exists
			//CreateSchemaIfRequired(targetSchema);
		}


		public override DbTable GetDbSchema() {
			var schemaReader = new SqlServerSchemaReader(_connectionFactory);
			return schemaReader.GetTables(_targetSchema, _targetTable).FirstOrDefault();
		}

		public override bool BulkInsert(DbTable table, CsvReader reader) {

			using (var cn = _connectionFactory()) {
				using (SqlBulkCopy copy = new SqlBulkCopy(cn)) {
					//copy.ColumnMappings = new SqlBulkCopyColumnMappingCollection();
					for (var i = 0; i < reader.Columns.Count; i++) {
						var column = reader.Columns[i].ColumnName;
						var sourceOrdinal = i;
						var destinationOrdinal = table.Columns.FindIndex(x => x.ColumnName == column);

						if (destinationOrdinal == -1) {
							var msg = string.Format("Unable to resolve column mapping, column: {0} was not found in destination table {1}",
								column,
								table.TableName
							);
							throw new Exception(msg);
						}
						copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, destinationOrdinal));
					}

					copy.DestinationTableName = string.Format("[{0}].[{1}]", table.SchemaName, table.TableName);

					copy.BatchSize = 1000;
					copy.BulkCopyTimeout = 9999999;

					copy.WriteToServer(reader);
				}

			}

			return true;
		}


		public bool CreateSchemaIfRequired(string schemaName) {
			var oldSchema = GetDbSchema();
			if (oldSchema == null) {
				var cmd = $"CREATE SCHEMA {schemaName}";
				ExecuteCommand(cmd);
			}
			return true;
		}


		public override bool BulkInsert(DbDataReader reader, Action<DbDataReader> eachRow) {
			throw new NotImplementedException();
		}

		public override bool CreateTable(DbTable table) {

			StringBuilder ddl = new System.Text.StringBuilder();
			ddl.Append(string.Format("CREATE TABLE [{0}].[{1}] (\r\n", table.SchemaName, table.TableName));


			foreach (var c in table.Columns) {
				var type = SqlServerTypeConverter.SqlServer.GetCreateColumnDefinition(c.MungType);
				ddl.Append(string.Format("\t[{0}] {1} NULL,\r\n", c.ColumnName, type));
			}
			ddl.Append(")");

			return ExecuteCommand(ddl.ToString());
		}

		public bool ExecuteCommand(string sql) {
			using (var cn = _connectionFactory()) {
				using (var cmd = new SqlCommand(sql, cn)) {
					cmd.ExecuteNonQuery();
				}
			}
			return true;

		}

		public bool SameColumn(DbColumn a, DbColumn b) {
			return a.ColumnName.ToLowerInvariant() == b.ColumnName.ToLowerInvariant();

		}


		public override bool ModifySchema(DbTable fromTbl, DbTable toTbl) {
			throw new NotImplementedException();
		}

	}


}