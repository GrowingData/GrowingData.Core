using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using GrowingData.Utilities.Csv;

namespace GrowingData.Utilities.Database {
	public abstract class DbBulkInserter {

		public abstract bool CreateTable(DbTable tbl);
		public abstract bool BulkInsert(string schemaName, string tableName, CsvReader reader);
		public abstract bool BulkInsert(string schemaName, string tableName, DbDataReader reader, Action<long> callback);
		public abstract bool ModifySchema(DbTable oldSchema, DbTable newSchema);


		public bool BulkInsert(string schemaName, string tableName, DbDataReader reader) {
			return BulkInsert(schemaName, tableName, reader, null);
		}

		public abstract DbTable GetDbSchema(string schema, string table);


		protected DbBulkInserter(Func<DbConnection> targetConnection) {

		}

		public bool Execute(string targetSchema, string targetTable, string filename) {
			using (var stream = File.OpenText(filename)) {
				using (var reader = new CsvReader(stream)) {

					BulkInsert(targetSchema, targetTable, reader);
				}

			}
			return true;
		}




	}
}
