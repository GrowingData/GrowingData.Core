// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// A munged data file is a 
	/// </summary>
	public class ReaderState {
		/// <summary>
		/// Defines the LineNumber
		/// </summary>
		public int LineNumber;

		/// <summary>
		/// Defines the FieldNumber
		/// </summary>
		public int FieldNumber;
	}

	/// <summary>
	/// Defines the <see cref="CsvDbDataReader" />
	/// </summary>
	public class CsvDbDataReader : DbDataReader {
#if NET45
		public override System.Data.DataTable GetSchemaTable() {
			throw new NotImplementedException();
		}


		public override void Close() {
			_lineReader.Dispose();
		}
#endif
#if NET45
		public override System.Data.DataTable GetSchemaTable() {
			throw new NotImplementedException();
		}


		public override void Close() {
			_lineReader.Dispose();
		}
#endif        /// <summary>
		/// Defines the _lineReader
		/// </summary>
		private LineReader _lineReader;

		/// <summary>
		/// Defines the _cursor
		/// </summary>
		private IEnumerator<string[]> _cursor;

		/// <summary>
		/// Defines the _columns
		/// </summary>
		private List<SqlColumn> _columns;

		/// <summary>
		/// Gets the Columns
		/// </summary>
		public List<SqlColumn> Columns => _columns;

		/// <summary>
		/// Defines the _columnOrdinals
		/// </summary>
		private Dictionary<string, int> _columnOrdinals;

		/// <summary>
		/// Gets the RowNumber
		/// </summary>
		public int RowNumber => _lineReader.LineNumber;

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvDbDataReader"/> class.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/></param>
		/// <param name="columns">The <see cref="List{SqlColumn}"/></param>
		/// <param name="options">The <see cref="CsvFileOptions"/></param>
		public CsvDbDataReader(TextReader reader, List<SqlColumn> columns, CsvFileOptions options) {
			_lineReader = new LineReader(options, reader);
			_cursor = _lineReader.GetLines().GetEnumerator();
			_columns = columns;

			if (options.HasHeaders) {
				// Skip over the first line as we have already defiend out columns
				// explictly.
				_cursor.MoveNext();

			}

			_columnOrdinals = new Dictionary<string, int>();
			for (var i = 0; i < _columns.Count; i++) {
				_columnOrdinals[columns[i].ColumnName] = i;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvDbDataReader"/> class.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/></param>
		/// <param name="options">The <see cref="CsvFileOptions"/></param>
		public CsvDbDataReader(TextReader reader, CsvFileOptions options) {
			_lineReader = new LineReader(options, reader);
			_cursor = _lineReader.GetLines().GetEnumerator();

			// Read the first line, as we expect it to have headers
			_cursor.MoveNext();

			// The file format here is that the first row includes the 
			// columnHeaders in the format "<name>:<type>"
			var columnHeaders = _cursor.Current;

			var columns = new SqlColumn[columnHeaders.Length];
			_columnOrdinals = new Dictionary<string, int>();


			for (var i = 0; i < columnHeaders.Length; i++) {
				// Unquote headers
				var header = columnHeaders[i].Replace("\"", "");
				var splitHeader = header.Split(':');
				if (splitHeader.Length == 2) {
					var columnName = splitHeader[0];
					var typeHeader = splitHeader[1];
					var type = SimpleDbType.Get(typeHeader);
					columns[i] = new SqlColumn(columnName, type.DotNetType);
				} else {
					if (header.Length == 0) {
						header = $"column_{i}";
					}
					columns[i] = new SqlColumn(header, SimpleDbType.Get(System.Data.DbType.String).DotNetType);
				}
				_columnOrdinals[columns[i].ColumnName] = i;
			}
			_columns = columns.ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvDbDataReader"/> class.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/></param>
		public CsvDbDataReader(TextReader reader) : this(reader, new CsvFileOptions() {
			FieldTerminator = '\t',
			HasHeaders = true,
			InvalidTextAction = InvalidTextMode.Throw,
			QuoteChar = '\"'
		}) {
		}

		/// <summary>
		/// The NextResult
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public override bool NextResult() {
			return false;
		}

		/// <summary>
		/// The Read
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public override bool Read() {
			return _cursor.MoveNext();
		}

		/// <summary>
		/// Gets the Depth
		/// </summary>
		public override int Depth => 1;

		/// <summary>
		/// Gets the FieldCount
		/// </summary>
		public override int FieldCount => _columns.Count;

		/// <summary>
		/// Gets a value indicating whether HasRows
		/// </summary>
		public override bool HasRows => _lineReader.IsReading;

		/// <summary>
		/// Gets a value indicating whether IsClosed
		/// </summary>
		public override bool IsClosed => _lineReader.IsEOF;

		/// <summary>
		/// Gets the RecordsAffected
		/// </summary>
		public override int RecordsAffected => -1;



		public override object this[string name] {
			get {
				var ordinal = GetOrdinal(name);
				if (ordinal != -1) {
					return GetValue(ordinal);
				}

				throw new KeyNotFoundException($"Unable to find column with name {name}");
			}
		}

		public string[] RawRow => _cursor.Current;

		public override object this[int ordinal] => GetValue(ordinal);
		/// <summary>
		/// The GetValue
		/// </summary>
		/// <param name="fieldIndex">The <see cref="int"/></param>
		/// <returns>The <see cref="object"/></returns>
		public override object GetValue(int fieldIndex) {
			if (fieldIndex == -1 || fieldIndex == int.MaxValue) {
				return DBNull.Value;
			}
			if (fieldIndex >= _cursor.Current.Length) {
				throw new InvalidOperationException($"Unable to read CSV, expecting {_columns.Count} columns, but only {_cursor.Current.Length} were found.  Line: {_lineReader.LineNumber}");
			}

			var val = _cursor.Current[fieldIndex];

			return CsvConverter.Parse(val, _columns[fieldIndex].DotNetType);
		}

		public string GetValueRaw(int fieldIndex) {
			return _cursor.Current[fieldIndex];
		}
		/// <summary>
		/// The GetBoolean
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool GetBoolean(int ordinal) {
			return (bool)GetValue(ordinal);
		}

		/// <summary>
		/// The GetByte
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="byte"/></returns>
		public override byte GetByte(int ordinal) {
			return (byte)GetValue(ordinal);
		}

		/// <summary>
		/// The GetBytes
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <param name="dataOffset">The <see cref="long"/></param>
		/// <param name="buffer">The <see cref="byte[]"/></param>
		/// <param name="bufferOffset">The <see cref="int"/></param>
		/// <param name="length">The <see cref="int"/></param>
		/// <returns>The <see cref="long"/></returns>
		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// The GetChars
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <param name="dataOffset">The <see cref="long"/></param>
		/// <param name="buffer">The <see cref="char[]"/></param>
		/// <param name="bufferOffset">The <see cref="int"/></param>
		/// <param name="length">The <see cref="int"/></param>
		/// <returns>The <see cref="long"/></returns>
		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// The GetChar
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="char"/></returns>
		public override char GetChar(int ordinal) {
			return (char)GetValue(ordinal);
		}

		/// <summary>
		/// The GetDataTypeName
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetDataTypeName(int ordinal) {
			return _columns[ordinal].SimpleType.ToString();
		}

		/// <summary>
		/// The GetDateTime
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="DateTime"/></returns>
		public override DateTime GetDateTime(int ordinal) {
			return (DateTime)GetValue(ordinal);
		}

		/// <summary>
		/// The GetDecimal
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="decimal"/></returns>
		public override decimal GetDecimal(int ordinal) {
			return (decimal)GetValue(ordinal);
		}

		/// <summary>
		/// The GetDouble
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="double"/></returns>
		public override double GetDouble(int ordinal) {
			return (double)GetValue(ordinal);
		}

		/// <summary>
		/// The GetEnumerator
		/// </summary>
		/// <returns>The <see cref="IEnumerator"/></returns>
		public override IEnumerator GetEnumerator() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// The GetFieldType
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="Type"/></returns>
		public override Type GetFieldType(int ordinal) {
			return typeof(string);
		}

		/// <summary>
		/// The GetFloat
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="float"/></returns>
		public override float GetFloat(int ordinal) {
			return (float)GetValue(ordinal);
		}

		/// <summary>
		/// The GetGuid
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="Guid"/></returns>
		public override Guid GetGuid(int ordinal) {
			return (Guid)GetValue(ordinal);
		}

		/// <summary>
		/// The GetInt16
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="short"/></returns>
		public override short GetInt16(int ordinal) {
			return (short)GetValue(ordinal);
		}

		/// <summary>
		/// The GetInt32
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetInt32(int ordinal) {
			return (int)GetValue(ordinal);
		}

		/// <summary>
		/// The GetInt64
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="long"/></returns>
		public override long GetInt64(int ordinal) {
			return (long)GetValue(ordinal);
		}

		/// <summary>
		/// The GetName
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetName(int ordinal) {
			return _columns[ordinal].ColumnName;
		}

		/// <summary>
		/// The GetOrdinal
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetOrdinal(string name) {
			var i = -1;
			if (_columnOrdinals.TryGetValue(name, out i)) {
				return i;
			}
			//for (var i = 0; i < _columns.Count; i++) {
			//	if (_columns[i].ColumnName == name) {
			//		return i;
			//	}
			//}
			return int.MaxValue;
		}

		/// <summary>
		/// The GetString
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetString(int ordinal) {
			return GetValue(ordinal).ToString();
		}

		/// <summary>
		/// The GetValues
		/// </summary>
		/// <param name="values">The <see cref="object[]"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetValues(object[] values) {
			if (values.Length != _columns.Count) {
				throw new InvalidOperationException($"Expected the values array to contain {_columns.Count} values, not {values.Length} as was received.");
			}

			for (var i = 0; i < _columns.Count; i++) {
				values[i] = GetValue(i);
			}
			return _columns.Count;
		}

		/// <summary>
		/// The IsDBNull
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool IsDBNull(int ordinal) {
			if (ordinal == -1 || ordinal == int.MaxValue) {
				return true;
			}

			var val = _cursor.Current[ordinal].ToLower();
			return CsvConverter.IsDBNull(val);
		}
	}
}
