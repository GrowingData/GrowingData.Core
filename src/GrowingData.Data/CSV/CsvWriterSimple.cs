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
	using System.Text;

	/// <summary>
	/// Defines the <see cref="CsvWriter" />
	/// </summary>
	public class CsvWriter : IDisposable {
		/// <summary>
		/// Defines the _validTypes
		/// </summary>
		private static HashSet<Type> _validTypes = new HashSet<Type>(SimpleDbType.ValidTypes.Select(x => x.DotNetType));

		/// <summary>
		/// Defines the _stream
		/// </summary>
		private Stream _stream;

		/// <summary>
		/// Defines the _columns
		/// </summary>
		private List<SqlColumn> _columns;

		/// <summary>
		/// Defines the _rows
		/// </summary>
		private int _rows = 0;

		/// <summary>
		/// Gets the Columns
		/// </summary>
		public List<SqlColumn> Columns => _columns;

		/// <summary>
		/// Defines the FieldDelimiter
		/// </summary>
		private byte[] FieldDelimiter = UTF8Encoding.UTF8.GetBytes("\t");

		/// <summary>
		/// Defines the RowDelimiter
		/// </summary>
		private byte[] RowDelimiter = UTF8Encoding.UTF8.GetBytes("\n");

		/// <summary>
		/// Gets the Rows
		/// </summary>
		public int Rows => _rows;

		/// <summary>
		/// Defines the _casters
		/// </summary>
		private List<Func<DbDataReader, int, string>> _casters;

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		public CsvWriter(Stream stream, DbDataReader reader) {
			_stream = stream;
			_columns = InitializeColumns(reader);
			WriteHeaders(_columns.Select(x => x.ColumnName).ToList());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/></param>
		/// <param name="headers">The <see cref="List{string}"/></param>
		public CsvWriter(Stream stream, IEnumerable<string> headers) {
			_stream = stream;
			WriteHeaders(headers.ToList());
		}

		public CsvWriter(Stream stream) {
			_stream = stream;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriter"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="FileStream"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <param name="fieldSerializers">The <see cref="List{Func{DbDataReader, int, string}}"/></param>
		public CsvWriter(FileStream stream, DbDataReader reader, List<Func<DbDataReader, int, string>> fieldSerializers) {
			_stream = stream;
			_casters = fieldSerializers;
			_columns = InitializeColumns(reader);
			WriteHeaders(_columns.Select(x => x.ColumnName).ToList());
		}

		/// <summary>
		/// The InitializeColumns
		/// </summary>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="List{SqlColumn}"/></returns>
		private List<SqlColumn> InitializeColumns(DbDataReader reader) {
			var columns = new List<SqlColumn>();
			for (var i = 0; i < reader.FieldCount; i++) {
				columns.Add(new SqlColumn(reader.GetName(i), reader.GetFieldType(i)));
			}
			return columns;
		}

		/// <summary>
		/// The InitializeColumns
		/// </summary>
		/// <param name="headers">The <see cref="List{string}"/></param>
		/// <returns>The <see cref="List{SqlColumn}"/></returns>
		public List<SqlColumn> InitializeColumns(List<string> headers) {
			var columns = new List<SqlColumn>();
			for (var i = 0; i < headers.Count; i++) {
				columns.Add(new SqlColumn(headers[i], typeof(string)));
			}
			return columns;
		}

		/// <summary>
		/// The WriteHeaders
		/// </summary>
		/// <param name="headers">The <see cref="List{string}"/></param>
		private void WriteHeaders(List<string> headers) {
			for (var i = 0; i < headers.Count; i++) {
				var value = headers[i];
				WriteValue(value, i == headers.Count - 1);
			}
		}

		/// <summary>
		/// The WriteValue
		/// </summary>
		/// <param name="value">The <see cref="string"/></param>
		/// <param name="isEndOfRow">The <see cref="bool"/></param>
		private void WriteValue(string value, bool isEndOfRow) {
			var valueBytes = UTF8Encoding.UTF8.GetBytes(value);
			_stream.Write(valueBytes, 0, valueBytes.Length);
			if (isEndOfRow) {
				_stream.Write(RowDelimiter, 0, RowDelimiter.Length);
			} else {
				_stream.Write(FieldDelimiter, 0, RowDelimiter.Length);

			}
		}

		/// <summary>
		/// The WriteAllRows
		/// </summary>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		public void WriteAllRows(DbDataReader reader) {
			while (reader.Read()) {
				WriteRow(reader);
			}
		}

		/// <summary>
		/// Writes a row of data to the file, using the same order as specified
		/// in _columns.
		/// </summary>
		/// <param name="reader"></param>
		public void WriteRow(DbDataReader reader) {
			if (_columns == null) {
				throw new InvalidOperationException("Unable to Write a row until the header has been written");
			}
			var fields = new string[reader.FieldCount];

			if (_casters != null) {
				for (var i = 0; i < fields.Length; i++) {
					if (reader.IsDBNull(i) && !_columns[i].IsNullable) {
						_columns[i].MarkTypeNullable();
					}

					var value = _casters[i](reader, i);
					WriteValue(value, i == fields.Length - 1);
				}
			} else {
				for (var i = 0; i < fields.Length; i++) {
					if (reader.IsDBNull(i) && !_columns[i].IsNullable) {
						_columns[i].MarkTypeNullable();
					}
					var value = CsvSerializer.Serialize(reader[i]);
					WriteValue(value, i == fields.Length - 1);
				}
			}
			_rows++;
		}

		/// <summary>
		/// Writes a row of data to the file, using the same order as specified
		/// in _columns.
		/// </summary>
		/// <param name="reader"></param>
		public void WriteRow(string[] rowValues) {
			//if (_columns == null) {
			//	throw new InvalidOperationException("Unable to Write a row until the header has been written");
			//}

			for (var i = 0; i < rowValues.Length; i++) {
				var value = rowValues[i];
				var serializedValue = CsvSerializer.Serialize(value);
				WriteValue(serializedValue, i == rowValues.Length - 1);
			}
			_rows++;
		}
		/// <summary>
		/// Writes a row of data to the file, using the same order as specified
		/// in _columns.
		/// </summary>
		/// <param name="reader"></param>
		public void WriteRow(object[] rowValues) {
			//if (_columns == null) {
			//	throw new InvalidOperationException("Unable to Write a row until the header has been written");
			//}

			for (var i = 0; i < rowValues.Length; i++) {
				var value = rowValues[i];
				var serializedValue = CsvSerializer.Serialize(value);
				WriteValue(serializedValue, i == rowValues.Length - 1);
			}
			_rows++;
		}

		/// <summary>
		/// The Dispose
		/// </summary>
		public void Dispose() {
			_stream.Dispose();
		}

		/// <summary>
		/// The WriteException
		/// </summary>
		/// <param name="ex">The <see cref="Exception"/></param>
		/// <param name="writer">The <see cref="TextWriter"/></param>
		public static void WriteException(Exception ex, TextWriter writer) {
			writer.Write("###ERROR###\r\n");
			writer.Write(ex.Message + "\r\n" + ex.StackTrace);
		}
	}
}
