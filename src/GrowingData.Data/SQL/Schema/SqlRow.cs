// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Linq;
	using Newtonsoft.Json;

	/// <summary>
	/// Defines the <see cref="SqlRowExtensions" />
	/// </summary>
	public static class SqlRowExtensions {
		/// <summary>
		/// The SqlInsertBulk
		/// </summary>
		/// <param name="stream">The <see cref="IEnumerable{SqlRow}"/></param>
		/// <param name="cn">The <see cref="SqlConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int SqlInsertBulk(this IEnumerable<SqlRow> stream, SqlConnection cn, string table) {

			SqlRowStream streamer = null;
			var copy = new SqlBulkCopy(cn);

			copy.DestinationTableName = table;
			copy.EnableStreaming = true;
			copy.BulkCopyTimeout = 0;
			var rows = 0;
			var isFirst = true;
			var cursor = stream.GetEnumerator();
			// Use the first row to set up everything.
			if (cursor.MoveNext()) {
				var row = cursor.Current;
				rows++;
				streamer = new SqlRowStream(row.Keys, () => {

					if (!isFirst && !cursor.MoveNext()) {
						return null;
					}
					isFirst = false;
					rows++;
					return cursor.Current;
				});
				rows++;
			}
			copy.WriteToServer(streamer);
			return rows;
		}
	}

	/// <summary>
	/// Defines the <see cref="SqlRow" />
	/// </summary>
	public class SqlRow : Dictionary<string, object> {
		private string[] _columnNames;

		[JsonIgnore]
		public IEnumerable<string> Columns => _columnNames;

		public SqlRow(IEnumerable<string> columnNames) {
			_columnNames = columnNames.ToArray();
		}

		public SqlRow Clone() {
			var cloned = new SqlRow(_columnNames);
			foreach(var kv in this) {
				cloned[kv.Key] = kv.Value;
			}
			return cloned;
		}

		public string [] GetRowArray() {
			string[] rowData = new string[_columnNames.Length];
			for(var i=0; i < _columnNames.Length; i++) {
				rowData[i] = this[_columnNames[i]]?.ToString(); 
			}
			return rowData;
		}

	}
}
