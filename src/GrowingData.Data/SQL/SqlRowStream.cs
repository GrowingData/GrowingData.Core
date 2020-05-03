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
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="SqlRowStream" />
	/// </summary>
	public class SqlRowStream : DbDataReader {
		/// <summary>
		/// Defines the _currentRow
		/// </summary>
		private SqlRow _currentRow;

		/// <summary>
		/// Defines the _columnNames
		/// </summary>
		private List<string> _columnNames;

		/// <summary>
		/// Defines the _nextRow
		/// </summary>
		private Func<SqlRow> _nextRow;

		/// <summary>
		/// Defines the _hasData
		/// </summary>
		private bool _hasData = false;

		/// <summary>
		/// Gets the RowData
		/// </summary>
		protected SqlRow RowData {
			get {
				if (_currentRow == null) {
					throw new InvalidOperationException("Unable to access CurrentRow until after .Read() has been called.");
				}
				return _currentRow;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlRowStream"/> class.
		/// </summary>
		/// <param name="columns">The <see cref="IEnumerable{string}"/></param>
		/// <param name="nextRow">The <see cref="Func{SqlRow}"/></param>
		public SqlRowStream(IEnumerable<string> columns, Func<SqlRow> nextRow) {
			_columnNames = columns.ToList();
			_nextRow = nextRow;
			_hasData = true;
		}

#if NET45
		public override System.Data.DataTable GetSchemaTable() {
			throw new NotImplementedException();
		}


		public override void Close() {
			return;
		}
#endif
		/// <summary>
		/// The Read
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public override bool Read() {
			var row = _nextRow();
			if (row == null) {
				_hasData = false;
				return false;
			}
			_currentRow = row;
			return true;
		}




		public override object this[string name] {
			get {
				return RowData[name];
			}
		}

		public override object this[int ordinal] {
			get {
				var name = _columnNames[ordinal];
				return RowData[name];
			}
		}
		/// <summary>
		/// Gets the Depth
		/// </summary>
		public override int Depth {
			get {
				return 0;
			}
		}

		/// <summary>
		/// Gets the FieldCount
		/// </summary>
		public override int FieldCount {
			get {
				return _columnNames.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether HasRows
		/// </summary>
		public override bool HasRows {
			get {
				return _hasData;
			}
		}

		/// <summary>
		/// Gets a value indicating whether IsClosed
		/// </summary>
		public override bool IsClosed {
			get {
				return false;
			}
		}

		/// <summary>
		/// Gets the RecordsAffected
		/// </summary>
		public override int RecordsAffected {
			get {
				return -1;
			}
		}

		/// <summary>
		/// The GetBoolean
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool GetBoolean(int ordinal) {
			return (bool)this[ordinal];
		}

		/// <summary>
		/// The GetByte
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="byte"/></returns>
		public override byte GetByte(int ordinal) {
			return (byte)this[ordinal];
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
		/// The GetChar
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="char"/></returns>
		public override char GetChar(int ordinal) {
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
		/// The GetDataTypeName
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetDataTypeName(int ordinal) {
			return this[ordinal].GetType().ToString();
		}

		/// <summary>
		/// The GetDateTime
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="DateTime"/></returns>
		public override DateTime GetDateTime(int ordinal) {
			return (DateTime)this[ordinal];
		}

		/// <summary>
		/// The GetDecimal
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="decimal"/></returns>
		public override decimal GetDecimal(int ordinal) {
			return (decimal)this[ordinal];
		}

		/// <summary>
		/// The GetDouble
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="double"/></returns>
		public override double GetDouble(int ordinal) {
			return (double)this[ordinal];
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
			return this[ordinal].GetType();
		}

		/// <summary>
		/// The GetFloat
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="float"/></returns>
		public override float GetFloat(int ordinal) {
			return (float)this[ordinal];
		}

		/// <summary>
		/// The GetGuid
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="Guid"/></returns>
		public override Guid GetGuid(int ordinal) {
			return (Guid)this[ordinal];
		}

		/// <summary>
		/// The GetInt16
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="short"/></returns>
		public override short GetInt16(int ordinal) {
			return (short)this[ordinal];
		}

		/// <summary>
		/// The GetInt32
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetInt32(int ordinal) {
			return (int)this[ordinal];
		}

		/// <summary>
		/// The GetInt64
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="long"/></returns>
		public override long GetInt64(int ordinal) {
			return (long)this[ordinal];
		}

		/// <summary>
		/// The GetName
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetName(int ordinal) {
			return _columnNames[ordinal];
		}

		/// <summary>
		/// The GetOrdinal
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetOrdinal(string name) {
			for (var i = 0; i < _columnNames.Count; i++) {
				if (name == _columnNames[i]) {
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// The GetString
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetString(int ordinal) {
			return (string)this[ordinal];
		}

		/// <summary>
		/// The GetValue
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="object"/></returns>
		public override object GetValue(int ordinal) {
			return this[ordinal];
		}

		/// <summary>
		/// The GetValues
		/// </summary>
		/// <param name="values">The <see cref="object[]"/></param>
		/// <returns>The <see cref="int"/></returns>
		public override int GetValues(object[] values) {
			for (var i = 0; i < _columnNames.Count; i++) {
				var name = _columnNames[i];
				values[i] = this[name];
			}
			return values.Length;
		}

		/// <summary>
		/// The IsDBNull
		/// </summary>
		/// <param name="ordinal">The <see cref="int"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public override bool IsDBNull(int ordinal) {
			return this[ordinal] == null;
		}

		/// <summary>
		/// The NextResult
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public override bool NextResult() {
			return false;
		}
	}
}
