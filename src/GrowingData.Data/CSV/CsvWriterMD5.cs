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
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="CsvWriterMD5" />
	/// </summary>
	public class CsvWriterMD5 : IDisposable {
		/// <summary>
		/// Defines the _validTypes
		/// </summary>
		private static HashSet<Type> _validTypes = new HashSet<Type>(SimpleDbType.ValidTypes.Select(x => x.DotNetType));

		/// <summary>
		/// Defines the _stream
		/// </summary>
		private Stream _stream;

		/// <summary>
		/// Defines the _cryptoStream
		/// </summary>
		private CryptoStream _cryptoStream;

		/// <summary>
		/// Defines the _md5
		/// </summary>
		private MD5 _md5;

		/// <summary>
		/// Defines the _columns
		/// </summary>
		private List<SqlColumn> _columns;

		/// <summary>
		/// Defines the _rowBuffer
		/// </summary>
		private List<string> _rowBuffer = new List<string>();

		/// <summary>
		/// Defines the _rows
		/// </summary>
		private int _rows = 0;

		/// <summary>
		/// Gets the Rows
		/// </summary>
		public int Rows => _rows;

		/// <summary>
		/// Defines the _casters
		/// </summary>
		private List<Func<DbDataReader, int, string>> _casters;

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriterMD5"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		public CsvWriterMD5(Stream stream, DbDataReader reader) {
			_stream = stream;
			_md5 = MD5.Create();
			_cryptoStream = new CryptoStream(_stream, _md5, CryptoStreamMode.Write);

			InitializeColumns(reader);
			WriteHeader();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriterMD5"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/></param>
		/// <param name="headers">The <see cref="IEnumerable{string}"/></param>
		public CsvWriterMD5(Stream stream, IEnumerable<string> headers) {
			_stream = stream;
			_md5 = MD5.Create();
			_cryptoStream = new CryptoStream(_stream, _md5, CryptoStreamMode.Write);
			WriteLine(string.Join("\t", headers));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvWriterMD5"/> class.
		/// </summary>
		/// <param name="stream">The <see cref="FileStream"/></param>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		/// <param name="fieldSerializers">The <see cref="List{Func{DbDataReader, int, string}}"/></param>
		public CsvWriterMD5(FileStream stream, DbDataReader reader, List<Func<DbDataReader, int, string>> fieldSerializers) {
			_stream = stream;
			_md5 = MD5.Create();
			_cryptoStream = new CryptoStream(_stream, _md5, CryptoStreamMode.Write);
			_casters = fieldSerializers;
			InitializeColumns(reader);
			WriteHeader();
		}

		/// <summary>
		/// The InitializeColumns
		/// </summary>
		/// <param name="reader">The <see cref="DbDataReader"/></param>
		private void InitializeColumns(DbDataReader reader) {
			var columns = new List<SqlColumn>();
			for (var i = 0; i < reader.FieldCount; i++) {
				columns.Add(new SqlColumn(reader.GetName(i), reader.GetFieldType(i)));
			}
			_columns = columns;
		}

		/// <summary>
		/// Gets the MD5Hash
		/// </summary>
		public string MD5Hash {
			get {
				FlushBatch();

				if (!_cryptoStream.HasFlushedFinalBlock) {
					_cryptoStream.FlushFinalBlock();
				}
				return System.Convert.ToBase64String(_md5.Hash);
			}
		}

		/// <summary>
		/// The FlushBatch
		/// </summary>
		private void FlushBatch() {
			// We need to add a blank line at the end
			_rowBuffer.Add("");
			var bytes = Encoding.UTF8.GetBytes(string.Join("\n", _rowBuffer));
			_cryptoStream.Write(bytes, 0, bytes.Length);
			_cryptoStream.Flush();
			_rowBuffer.Clear();
		}

		/// <summary>
		/// The WriteRow
		/// </summary>
		/// <param name="row">The <see cref="IEnumerable{string}"/></param>
		public void WriteRow(IEnumerable<string> row) {
			var line = string.Join("\t", row);
			WriteLine(line);
		}

		/// <summary>
		/// The WriteLine
		/// </summary>
		/// <param name="line">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		private int WriteLine(string line) {
			_rowBuffer.Add(line);
			if (_rowBuffer.Count > 1000) {
				FlushBatch();
			}

			return _rowBuffer.Count;
		}

		/// <summary>
		/// The WriteHeader
		/// </summary>
		private void WriteHeader() {
			WriteLine(string.Join("\t", _columns.Select(c => $"{c.ColumnName}")));
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
					fields[i] = _casters[i](reader, i);
				}
			} else {

				for (var i = 0; i < fields.Length; i++) {
					fields[i] = CsvSerializer.Serialize(reader[i]);
				}
				//var rowData = _columns.Select(x => Serialize(reader[x.ColumnName]));

			}
			var line = string.Join("\t", fields);
			WriteLine(line);
			_rows++;
		}

		/// <summary>
		/// The Dispose
		/// </summary>
		public void Dispose() {
			if (!_cryptoStream.HasFlushedFinalBlock) {
				_cryptoStream.FlushFinalBlock();
			}
			_cryptoStream.Dispose();
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
