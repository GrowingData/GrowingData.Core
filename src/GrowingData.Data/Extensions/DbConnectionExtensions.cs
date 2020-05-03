// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.IO;
	using System.Linq;
	using System.Text;
	using GrowingData.Utilities;
	using Newtonsoft.Json;

	/// <summary>
	/// Defines the <see cref="DbConnectionExtensions" />
	/// </summary>
	public static class DbConnectionExtensions {
		/// <summary>
		/// Defines the DEFAULT_TIMEOUT
		/// </summary>
		public static int DEFAULT_TIMEOUT = 0;

		/// <summary>
		/// Defines the DEFAULT_PARAMETER_PREFIX
		/// </summary>
		public static char DEFAULT_PARAMETER_PREFIX = '@';

		/// <summary>
		/// Gets the TYPE_MAP
		/// </summary>
		private static Dictionary<Type, DbType> TYPE_MAP =>
			new Dictionary<Type, DbType> {
				[typeof(byte)] = DbType.Byte,
				[typeof(sbyte)] = DbType.SByte,
				[typeof(short)] = DbType.Int16,
				[typeof(ushort)] = DbType.UInt16,
				[typeof(int)] = DbType.Int32,
				[typeof(uint)] = DbType.UInt32,
				[typeof(long)] = DbType.Int64,
				[typeof(ulong)] = DbType.UInt64,
				[typeof(float)] = DbType.Single,
				[typeof(double)] = DbType.Double,
				[typeof(decimal)] = DbType.Decimal,
				[typeof(bool)] = DbType.Boolean,
				[typeof(string)] = DbType.String,
				[typeof(char)] = DbType.StringFixedLength,
				[typeof(Guid)] = DbType.Guid,
				[typeof(DateTime)] = DbType.DateTime,
				[typeof(DateTimeOffset)] = DbType.DateTimeOffset,
				[typeof(byte[])] = DbType.Binary,
				[typeof(byte?)] = DbType.Byte,
				[typeof(sbyte?)] = DbType.SByte,
				[typeof(short?)] = DbType.Int16,
				[typeof(ushort?)] = DbType.UInt16,
				[typeof(int?)] = DbType.Int32,
				[typeof(uint?)] = DbType.UInt32,
				[typeof(long?)] = DbType.Int64,
				[typeof(ulong?)] = DbType.UInt64,
				[typeof(float?)] = DbType.Single,
				[typeof(double?)] = DbType.Double,
				[typeof(decimal?)] = DbType.Decimal,
				[typeof(bool?)] = DbType.Boolean,
				[typeof(char?)] = DbType.StringFixedLength,
				[typeof(Guid?)] = DbType.Guid,
				[typeof(DateTime?)] = DbType.DateTime,
				[typeof(DateTimeOffset?)] = DbType.DateTimeOffset
				// [typeof(System.Data.Linq.Binary)] = DbType.Binary
			};

		/// <summary>
		/// The SqlParameterNames
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public static List<string> SqlParameterNames(string sql) {
			return SqlParameterNames(sql, DEFAULT_PARAMETER_PREFIX);
		}

		/// <summary>
		/// The SqlParameterNames
		/// </summary>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="parameterPrefix">The <see cref="char"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public static List<string> SqlParameterNames(string sql, char parameterPrefix) {

			var parameters = new HashSet<string>();
			var inVariable = false;
			var buffer = new StringBuilder();
			for (var i = 0; i < sql.Length; i++) {
				var c = sql[i];
				if (inVariable) {
					if (char.IsLetterOrDigit(c) || c == '_') {
						buffer.Append(c);
					} else {
						var p = buffer.ToString();
						if (p.Length > 0 && !parameters.Contains(p)) {
							parameters.Add(p);
						}

						buffer.Length = 0;
					}
				} else {
					if (c == parameterPrefix) {
						inVariable = true;
					}
				}
			}
			if (buffer.Length > 0) {
				var p = buffer.ToString();
				if (!parameters.Contains(p)) {
					parameters.Add(p);
				}
			}

			return parameters.ToList();
		}

		/// <summary>
		/// Parse the SQL looking for parameters, then create parameters for those 
		/// variables if the supplied object has a matching Field or Property
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		private static void BindParameters(DbCommand cmd, string sql, object ps) {
			if (ps != null) {


				var type = ps.GetType();

				var properties = type.GetProperties();
				var fields = type.GetFields();

				var sqlParameters = SqlParameterNames(sql);

				foreach (var p in properties) {
					if (sqlParameters.Contains(p.Name)) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + p.Name, p.GetValue(ps), p.PropertyType));
					} else if (sqlParameters.Contains(p.Name.ToDnsSafeLabel())) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + p.Name.ToDnsSafeLabel(), p.GetValue(ps), p.PropertyType));
					}
				}
				foreach (var f in fields) {
					if (sqlParameters.Contains(f.Name)) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + f.Name, f.GetValue(ps), f.FieldType));
					} else if (sqlParameters.Contains(f.Name.ToDnsSafeLabel())) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + f.Name.ToDnsSafeLabel(), f.GetValue(ps), f.FieldType));
					}
				}
			}
		}

		/// <summary>
		/// Parse the SQL looking for parameters, then create parameters for those 
		/// variables if the supplied object has a matching Field or Property
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		private static void BindParameterDictionary(DbCommand cmd, string sql, Dictionary<string, object> ps) {
			if (ps != null) {
				var sqlParameters = SqlParameterNames(sql);

				foreach (var p in ps) {
					if (sqlParameters.Contains(p.Key)) {
						cmd.Parameters.Add(GetParameter(cmd, DEFAULT_PARAMETER_PREFIX + p.Key, p.Value, p.Value.GetType()));
					} else {
						throw new Exception($"No parameter named: {DEFAULT_PARAMETER_PREFIX}{p.Key} was found in the query.\r\n--------\r\n{sql}");
					}
				}
			}
		}

		/// <summary>
		/// The GetParameter
		/// </summary>
		/// <param name="cmd">The <see cref="DbCommand"/></param>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="val">The <see cref="object"/></param>
		/// <param name="type">The <see cref="Type"/></param>
		/// <returns>The <see cref="DbParameter"/></returns>
		public static DbParameter GetParameter(DbCommand cmd, string name, object val, Type type) {
			var p = cmd.CreateParameter();
			p.ParameterName = name;
			p.Value = val ?? DBNull.Value;
			if (val == null) {
				p.DbType = TYPE_MAP[type];
			}

			return p;
		}

		/// <summary>
		/// Creates a command, using the specified Connection, with the specified SQL and with
		/// all SQL variables (@Param) bound to fields from the supplied object 
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static DbCommand CreateCommand(this DbConnection cn, string sql, object ps) {
			var cmd = cn.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandTimeout = DEFAULT_TIMEOUT;

			var paramDictionary = ps as Dictionary<string, object>;
			if (paramDictionary != null) {
				BindParameterDictionary(cmd, sql, paramDictionary);
			} else {
				BindParameters(cmd, sql, ps);
			}
			return cmd;
		}

		/// <summary>
		/// Creates a command, using the specified Connection, with the specified SQL and with
		/// all SQL variables (@Param) bound to fields from the supplied object 
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static DbCommand CreateCommand(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			var cmd = cn.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandTimeout = DEFAULT_TIMEOUT;
			BindParameterDictionary(cmd, sql, ps);
			return cmd;
		}

		/// <summary>
		/// The ReflectResults
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="List{T}"/></returns>
		public static List<T> ReflectResults<T>(DbDataReader r) where T : new() {

			var type = typeof(T);

			var properties = DataReaderExtensions.ReflectPropertyKeys(type);
			var fields = DataReaderExtensions.ReflectFieldKeys(type);

			Dictionary<string, string> columnNames = null;
			var results = new List<T>();
			while (r.Read()) {
				var obj = new T();

				if (columnNames == null) {
					columnNames = DataReaderExtensions.GetColumnNameKeys(r);
				}

				DataReaderExtensions.BindProperties(r, properties, columnNames, obj);
				DataReaderExtensions.BindFields(r, fields, columnNames, obj);

				results.Add(obj);
			}

			return results;
		}

		/// <summary>
		/// The Escape
		/// </summary>
		/// <param name="unescaped">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		private static string Escape(string unescaped) {
			return unescaped
				.Replace("\\", "\\" + "\\")     // '\' -> '\\'
				.Replace("\"", "\\" + "\"");        // '"' -> '""'
		}

		/// <summary>
		/// Use reflection to bind columns to the type of object specified
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<T> SelectAnonymous<T>(this DbConnection cn, string sql, object ps) where T : new() {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var type = typeof(T);
				var properties = type.GetProperties().ToDictionary(x => x.Name);
				var fields = type.GetFields().ToDictionary(x => x.Name);
				using (var r = cmd.ExecuteReader()) {
					return ReflectResults<T>(r);
				}
			}
		}

		/// <summary>
		/// The ExecuteReader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="DbDataReader"/></returns>
		public static DbDataReader ExecuteReader<T>(this DbConnection cn, string sql, object ps) where T : new() {
			var cmd = cn.CreateCommand(sql, ps);
			var r = cmd.ExecuteReader();
			return r;
		}

		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query. 
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, DbTransaction txn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				cmd.Transaction = txn;
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// The SelectForEach
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="fn">The <see cref="Action{DbDataReader}"/></param>
		public static void SelectForEach(this DbConnection cn, string sql, object ps, Action<DbDataReader> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						fn(reader);
					}
				}
			}
		}

		/// <summary>
		/// The SelectForEach
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="fn">The <see cref="Func{DbDataReader, TResult}"/></param>
		/// <returns>The <see cref="IEnumerable{TResult}"/></returns>
		public static IEnumerable<TResult> SelectForEach<TResult>(this DbConnection cn, string sql, object ps, Func<DbDataReader, TResult> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						yield return fn(reader);
					}
				}
			}
		}

		/// <summary>
		/// Yields a dictionary for each row that is returned from the query.
		/// The Dictionary is the same object for each row, with its values changed.
		/// e.g. if you try to call .ToList() on the enumeration all the rows will have
		/// the same values as the last row.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static IEnumerable<SqlRow> SelectRows(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					SqlRow rowData = null;
					while (reader.Read()) {
						if (rowData == null) {
							var columnNames = new List<string>();
							for (var i = 0; i < reader.FieldCount; i++) {
								var name = reader.GetName(i);
								columnNames.Add(name);
							}
							rowData = new SqlRow(columnNames);
						}

						for (var i = 0; i < reader.FieldCount; i++) {
							var name = reader.GetName(i);
							rowData[name] = reader[i];
						}
						yield return rowData;
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<SqlRow> SelectRowsList(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var rows = new List<SqlRow>();
				using (var reader = cmd.ExecuteReader()) {
					SqlRow rowData = null;
					while (reader.Read()) {
						if (rowData == null) {
							var columnNames = new List<string>();
							for (var i = 0; i < reader.FieldCount; i++) {
								var name = reader.GetName(i);
								columnNames.Add(name);
							}
							rowData = new SqlRow(columnNames);
						}

						for (var i = 0; i < reader.FieldCount; i++) {
							rowData[reader.GetName(i)] = reader[i];
						}
						rows.Add(rowData);
					}
				}
				return rows;
			}
		}

		/// <summary>
		/// The SelectList
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="List{T}"/></returns>
		public static List<T> SelectList<T>(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var list = new List<T>();
					while (reader.Read()) {
						if (reader[0] != DBNull.Value) {
							// Becasue for Sqlite all ints are long.  PAIN
							if (typeof(T) == typeof(int) && reader[0].GetType() == typeof(long)) {

								list.Add((T)(object)(int)(long)reader[0]);
							} else {

								list.Add((T)reader[0]);
							}
						}
					}
					return list;
				}
			}
		}

		/// <summary>
		/// The ExecuteReader
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="DbDataReader"/></returns>
		public static DbDataReader ExecuteReader(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteReader();
			}
		}

		/// <summary>
		/// The ExecuteTSV
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="writer">The <see cref="StreamWriter"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int ExecuteTSV(this DbConnection cn, string sql, object ps, StreamWriter writer) {
			var output = new StringBuilder();

			var rowCount = 0;
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var isFirst = true;
					while (reader.Read()) {

						if (isFirst) {

							var names = new List<string>();

							for (var i = 0; i < reader.FieldCount; i++) {
								names.Add(reader.GetName(i));
							}
							writer.WriteLine(string.Join("\t", names));

							isFirst = false;
						}

						var rowData = Enumerable.Range(0, reader.FieldCount).Select(i => CsvSerializer.Serialize(reader[i]));

						writer.WriteLine(string.Join("\t", rowData));

						rowCount++;

						if (rowCount % 1000 == 0) {
							writer.Flush();
							System.Diagnostics.Debug.WriteLine(string.Format("Wrote {0} rows", rowCount));
						}
					}
				}
			}


			return rowCount;
		}

		/// <summary>
		/// The ExecuteJsonRows
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string ExecuteJsonRows(this DbConnection cn, string sql, object ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					// Field names
					var columnNames =
						Enumerable.Range(0, reader.FieldCount)
							.Select(x => reader.GetName(x))
							.ToList();
					var data = new List<Dictionary<string, string>>();
					while (reader.Read()) {
						var rowData = new Dictionary<string, string>();
						for (var i = 0; i < reader.FieldCount; i++) {
							if (reader[i].GetType() == typeof(DateTime)) {
								// Use ISO time
								rowData[columnNames[i]] = ((DateTime)reader[i]).ToString("s");
							} else {
								rowData[columnNames[i]] = reader[i].ToString();
							}
						}
						data.Add(rowData);
					}
					return JsonConvert.SerializeObject(new { ColumnNames = columnNames, Rows = data });
				}
			}
		}

		//public static DataTable ExecuteDataTable(this DbConnection cn, string sql, object ps) {
		//	using (var cmd = cn.CreateCommand(sql, ps)) {
		//		using (var reader = cmd.ExecuteReader()) {
		//			// Field names
		//			new DataTable()
		//			var table = new DataTable();
		//			table.Load(reader);
		//			return table;
		//		}
		//	}
		//}


		/// <summary>
		/// Use reflection to bind columns to the type of object specified
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<T> SelectAnonymous<T>(this DbConnection cn, string sql, Dictionary<string, object> ps) where T : new() {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var type = typeof(T);
				var properties = type.GetProperties().ToDictionary(x => x.Name);
				var fields = type.GetFields().ToDictionary(x => x.Name);
				using (var r = cmd.ExecuteReader()) {
					return ReflectResults<T>(r);
				}
			}
		}

		/// <summary>
		/// The ExecuteReader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <returns>The <see cref="DbDataReader"/></returns>
		public static DbDataReader ExecuteReader<T>(this DbConnection cn, string sql, Dictionary<string, object> ps) where T : new() {
			var cmd = cn.CreateCommand(sql, ps);
			var r = cmd.ExecuteReader();
			return r;
		}

		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// The ExecuteNonQuery
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int ExecuteNonQuery(this DbConnection cn, string sql) {
			using (var cmd = cn.CreateCommand(sql, null)) {
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Executes an SQL Command using the supplied connection and sql query.
		/// The object, "ps" will be reflected such that its properties are bound
		/// as named parameters to the query.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery(this DbConnection cn, DbTransaction txn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				cmd.Transaction = txn;
				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// The SelectForEach
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <param name="fn">The <see cref="Action{DbDataReader}"/></param>
		public static void SelectForEach(this DbConnection cn, string sql, Dictionary<string, object> ps, Action<DbDataReader> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						fn(reader);
					}
				}
			}
		}

		/// <summary>
		/// The SelectForEach
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <param name="fn">The <see cref="Func{DbDataReader, TResult}"/></param>
		/// <returns>The <see cref="IEnumerable{TResult}"/></returns>
		public static IEnumerable<TResult> SelectForEach<TResult>(this DbConnection cn, string sql, Dictionary<string, object> ps, Func<DbDataReader, TResult> fn) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						yield return fn(reader);
					}
				}
			}
		}

		/// <summary>
		/// Yields a dictionary for each row that is returned from the query.
		/// The Dictionary is the same object for each row, with its values changed.
		/// e.g. if you try to call .ToList() on the enumeration all the rows will have
		/// the same values as the last row.
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static IEnumerable<SqlRow> SelectRows(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					SqlRow rowData = null;
					while (reader.Read()) {
						for (var i = 0; i < reader.FieldCount; i++) {
							rowData[reader.GetName(i)] = reader[i];
						}
						yield return rowData;
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		/// <returns></returns>
		public static List<SqlRow> SelectRowsList(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				var rows = new List<SqlRow>();
				using (var reader = cmd.ExecuteReader()) {
					SqlRow rowData = null;
					while (reader.Read()) {
						if (rowData == null) {
							var columnNames = new List<string>();
							for (var i = 0; i < reader.FieldCount; i++) {
								var name = reader.GetName(i);
								columnNames.Add(name);
							}
							rowData = new SqlRow(columnNames);
						}
						for (var i = 0; i < reader.FieldCount; i++) {
							rowData[reader.GetName(i)] = reader[i];
						}
						rows.Add(rowData);
					}
				}
				return rows;
			}
		}

		/// <summary>
		/// The SelectList
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <returns>The <see cref="List{T}"/></returns>
		public static List<T> SelectList<T>(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var list = new List<T>();
					while (reader.Read()) {
						if (reader[0] != DBNull.Value) {
							// Becasue for Sqlite all ints are long.  PAIN
							if (typeof(T) == typeof(int) && reader[0].GetType() == typeof(long)) {

								list.Add((T)(object)(int)(long)reader[0]);
							} else {

								list.Add((T)reader[0]);
							}
						}
					}
					return list;
				}
			}
		}

		/// <summary>
		/// The ExecuteReader
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <returns>The <see cref="DbDataReader"/></returns>
		public static DbDataReader ExecuteReader(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				return cmd.ExecuteReader();
			}
		}

		/// <summary>
		/// The ExecuteTSV
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <param name="writer">The <see cref="StreamWriter"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int ExecuteTSV(this DbConnection cn, string sql, Dictionary<string, object> ps, StreamWriter writer) {
			var output = new StringBuilder();

			var rowCount = 0;
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					var isFirst = true;
					while (reader.Read()) {

						if (isFirst) {

							var names = new List<string>();
							for (var i = 0; i < reader.FieldCount; i++) {
								names.Add(reader.GetName(i));
							}
							writer.WriteLine(string.Join("\t", names));

							isFirst = false;
						}

						var rowData = Enumerable.Range(0, reader.FieldCount).Select(i => CsvSerializer.Serialize(reader[i]));

						writer.WriteLine(string.Join("\t", rowData));

						rowCount++;

						if (rowCount % 1000 == 0) {
							writer.Flush();
							System.Diagnostics.Debug.WriteLine(string.Format("Wrote {0} rows", rowCount));
						}
					}
				}
			}


			return rowCount;
		}

		/// <summary>
		/// The ExecuteJsonRows
		/// </summary>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="ps">The <see cref="Dictionary{string, object}"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string ExecuteJsonRows(this DbConnection cn, string sql, Dictionary<string, object> ps) {
			using (var cmd = cn.CreateCommand(sql, ps)) {
				using (var reader = cmd.ExecuteReader()) {
					// Field names
					var columnNames =
						Enumerable.Range(0, reader.FieldCount)
							.Select(x => reader.GetName(x))
							.ToList();
					var data = new List<Dictionary<string, string>>();
					while (reader.Read()) {
						var rowData = new Dictionary<string, string>();
						for (var i = 0; i < reader.FieldCount; i++) {
							if (reader[i].GetType() == typeof(DateTime)) {
								// Use ISO time
								rowData[columnNames[i]] = ((DateTime)reader[i]).ToString("s");
							} else {
								rowData[columnNames[i]] = reader[i].ToString();
							}
						}
						data.Add(rowData);
					}
					return JsonConvert.SerializeObject(new { ColumnNames = columnNames, Rows = data });
				}
			}
		}
	}
}
