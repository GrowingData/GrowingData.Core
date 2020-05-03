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
	using System.Reflection;
	using GrowingData.Utilities;

	/// <summary>
	/// Defines the <see cref="ISqlTable" />
	/// </summary>
	public interface ISqlTable {
	}

	/// <summary>
	/// Defines the <see cref="SqlTableIdentityAttribute" />
	/// </summary>
	public class SqlTableIdentityAttribute : Attribute {
	}

	/// <summary>
	/// Defines the <see cref="SqlInsertOnlyAttribute" />
	/// </summary>
	public class SqlInsertOnlyAttribute : Attribute {
	}

	/// <summary>
	/// Defines the <see cref="SqlIgnoreAttribute" />
	/// </summary>
	public class SqlIgnoreAttribute : Attribute {
	}

	/// <summary>
	/// Defines the <see cref="ISqlTableExtensions" />
	/// </summary>
	public static class ISqlTableExtensions {
		/// <summary>
		/// The SqlDropTableCommand
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlDropTableCommand(this ISqlTable ps, string name) {
			return $@"DROP TABLE {name}; ";
		}

		/// <summary>
		/// The SqlDropTableCommand
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlDropTableCommand(this ISqlTable ps) {
			return SqlDropTableCommand(ps, ps.SqlGetFullName());
		}

		/// <summary>
		/// The SqlInsert
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int SqlInsert(this ISqlTable ps, DbConnection cn, string table) {
			var columns = SqlInsertColumns(ps);
			var sql = $@"INSERT INTO {table} ({string.Join(",", columns)})
				VALUES ({string.Join(",", columns.Select(x => "@" + x))}) 

			";
			return cn.ExecuteNonQuery(sql, ps);
		}

		/// <summary>
		/// The SqlInsert
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T SqlInsert<T>(this ISqlTable ps, DbConnection cn, string table) where T : new() {
			var columns = SqlInsertColumns(ps);
			var identityColumn = SqlIdentityColumnName(ps);

			if (identityColumn == null) {
				throw new Exception("Unable to Insert without a Property or Field marked with the SqlTableIdentityAttribute");
			}

			var sql = $@"INSERT INTO {table} ({string.Join(",", columns)})
				VALUES ({string.Join(",", columns.Select(x => "@" + x))})
				SELECT * FROM {table} WHERE {identityColumn} = SCOPE_IDENTITY()

			";
			return cn.SelectAnonymous<T>(sql, ps).FirstOrDefault();
		}

		/// <summary>
		/// The SqlUpdate
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T SqlUpdate<T>(this ISqlTable ps, DbConnection cn, string table) where T : new() {
			var columns = SqlUpdateColumns(ps);
			var identityColumn = SqlIdentityColumnName(ps);

			if (identityColumn == null) {
				throw new Exception("Unable to Update without a Property or Field marked with the SqlTableIdentityAttribute");
			}
			var sql = $@"UPDATE {table} 
					SET {string.Join(", ", columns.Select(x => $"{x} = @{x}"))}
					WHERE {identityColumn} = @{identityColumn}

				SELECT * FROM {table} WHERE {identityColumn} = @{identityColumn}
			";
			return cn.SelectAnonymous<T>(sql, ps).FirstOrDefault();
		}

		/// <summary>
		/// The AsDictionary
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="Dictionary{string, object}"/></returns>
		public static Dictionary<string, object> AsDictionary(this ISqlTable ps) {
			return BindToDictionary(ps);
		}

		/// <summary>
		/// The BindToDictionary
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="Dictionary{string, object}"/></returns>
		internal static Dictionary<string, object> BindToDictionary(object ps) {
			var intermediate = new Dictionary<string, object>();
			var type = ps.GetType();
			var properties = type.GetProperties();
			foreach (var p in properties) {
				var obj = p.GetValue(ps);
				intermediate[p.Name] = obj;
			}
			var fields = type.GetFields();
			foreach (var f in fields) {
				var obj = f.GetValue(ps);
				intermediate[f.Name] = obj;
			}
			return intermediate;
		}

		/// <summary>
		/// The BindFromDictionary
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="values">The <see cref="Dictionary{string, object}"/></param>
		internal static void BindFromDictionary(object ps, Dictionary<string, object> values) {
			var type = ps.GetType();
			var properties = type.GetProperties();
			foreach (var p in properties) {
				if (values.ContainsKey(p.Name)) {
					p.SetValue(ps, values[p.Name]);
				}
			}
			var fields = type.GetFields();
			foreach (var f in fields) {
				if (values.ContainsKey(f.Name)) {
					f.SetValue(ps, values[f.Name]);
				}
			}
		}

		/// <summary>
		/// The FromDictionary
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="values">The <see cref="Dictionary{string, object}"/></param>
		public static void FromDictionary(this ISqlTable ps, Dictionary<string, object> values) {
			BindFromDictionary(ps, values);
		}

		/// <summary>
		/// The CopyFromObject
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="o">The <see cref="object"/></param>
		public static void CopyFromObject(this ISqlTable ps, object o) {
			var values = BindToDictionary(o);
			BindFromDictionary(ps, values);
		}

		/// <summary>
		/// The CopyToObject
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="o">The <see cref="object"/></param>
		public static void CopyToObject(this ISqlTable ps, object o) {
			var values = ps.AsDictionary();
			BindFromDictionary(o, values);
		}

		/// <summary>
		/// The SqlInsertBulk
		/// </summary>
		/// <param name="stream">The <see cref="IEnumerable{ISqlTable}"/></param>
		/// <param name="cn">The <see cref="SqlConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int SqlInsertBulk(this IEnumerable<ISqlTable> stream, SqlConnection cn) {
			SqlRowStream streamer = null;
			var copy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, null);

			copy.EnableStreaming = true;
			copy.BulkCopyTimeout = 0;

			var rows = 0;

			var cursor = stream.GetEnumerator();
			// Use the first row to set up everything.
			if (cursor.MoveNext()) {
				var obj = cursor.Current;
				// Do the column mapping thing
				var columnNames = new List<string>();
				foreach (var name in obj.SqlInsertColumns()) {
					copy.ColumnMappings.Add(name, name);
					columnNames.Add(name);
				}

				var row = new SqlRow(columnNames);
				//Bind the first object to the row so we can get the column names
				cursor.Current.SqlRow(row);

				copy.DestinationTableName = obj.SqlGetFullName();
				var isFirst = true;
				streamer = new SqlRowStream(row.Keys, () => {
					if (isFirst) {
						isFirst = false;
					} else {
						if (!cursor.MoveNext()) {
							return null;
						}
					}
					// Bind the position
					cursor.Current.SqlRow(row);

					rows++;
					return row;
				});
				rows++;
				copy.WriteToServer(streamer);
			}
			return rows;
		}

		/// <summary>
		/// The SqlInsertBulk
		/// </summary>
		/// <param name="stream">The <see cref="IEnumerable{ISqlTable}"/></param>
		/// <param name="cn">The <see cref="SqlConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int SqlInsertBulk(this IEnumerable<ISqlTable> stream, SqlConnection cn, string table) {
			SqlRowStream streamer = null;
			var copy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, null);

			copy.DestinationTableName = table;
			copy.EnableStreaming = true;
			copy.BulkCopyTimeout = 0;

			var rows = 0;

			var cursor = stream.GetEnumerator();
			// Use the first row to set up everything.
			if (cursor.MoveNext()) {
				var obj = cursor.Current;
				// Do the column mapping thing
				var columnNames = new List<string>();
				foreach (var name in obj.SqlInsertColumns()) {
					copy.ColumnMappings.Add(name, name);
					columnNames.Add(name);
				}
				var row = new SqlRow(columnNames);

				//Bind the first object to the row so we can get the column names
				cursor.Current.SqlRow(row);


				var isFirst = true;
				streamer = new SqlRowStream(row.Keys, () => {
					if (isFirst) {
						isFirst = false;
					} else {
						if (!cursor.MoveNext()) {
							return null;
						}
					}
					// Bind the position
					cursor.Current.SqlRow(row);

					rows++;
					return row;
				});
				rows++;
				copy.WriteToServer(streamer);
			}
			return rows;
		}

		/// <summary>
		/// The SqlIdentityColumnName
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlIdentityColumnName(this ISqlTable ps) {
			if (ps == null) {
				return null;
			}
			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();

			var items = new List<string>();

			foreach (var p in properties) {
				var obj = p.GetValue(ps);

				var isId = p.GetCustomAttribute<SqlTableIdentityAttribute>();
				if (isId != null) {
					return p.Name;
				}

			}

			foreach (var f in fields) {
				var obj = f.GetValue(ps);
				var isId = f.GetCustomAttribute<SqlTableIdentityAttribute>();

				if (isId != null) {
					return f.Name;
				}

			}
			return null;
		}

		/// <summary>
		/// The SqlInsertColumns
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public static List<string> SqlInsertColumns(this ISqlTable ps) {
			if (ps == null) {
				return null;
			}
			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();

			var items = new List<string>();

			foreach (var p in properties) {
				//var obj = p.GetValue(ps);
				var isId = p.GetCustomAttribute<SqlTableIdentityAttribute>() != null;
				var isIgnore = p.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isId && !isIgnore) {
					items.Add(p.Name);
				}

			}

			foreach (var f in fields) {
				//var obj = f.GetValue(ps);
				var isId = f.GetCustomAttribute<SqlTableIdentityAttribute>() != null;
				var isIgnore = f.GetCustomAttribute<SqlIgnoreAttribute>() != null;

				if (!isId && !isIgnore) {
					items.Add(f.Name);
				}

			}
			return items;
		}

		/// <summary>
		/// The SqlUpdateColumns
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public static List<string> SqlUpdateColumns(this ISqlTable ps) {
			if (ps == null) {
				return null;
			}
			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();

			var items = new List<string>();

			foreach (var p in properties) {
				//var obj = p.GetValue(ps);
				var isId = p.GetCustomAttribute<SqlTableIdentityAttribute>() != null;
				var isInsertOnly = p.GetCustomAttribute<SqlInsertOnlyAttribute>() != null;
				var isIgnore = p.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isId && !isInsertOnly && !isIgnore) {
					items.Add(p.Name);
				}

			}

			foreach (var f in fields) {
				//var obj = f.GetValue(ps);
				var isId = f.GetCustomAttribute<SqlTableIdentityAttribute>() != null;
				var isInsertOnly = f.GetCustomAttribute<SqlInsertOnlyAttribute>() != null;
				var isIgnore = f.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isId && !isInsertOnly && !isIgnore) {
					items.Add(f.Name);
				}

			}
			return items;
		}

		/// <summary>
		/// The SqlGetSchema
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public static SqlTable SqlGetSchema(this ISqlTable ps) {
			return new SqlTable(ps.SqlGetTableName(), ps.SqlGetSchemaName(), ps.SqlGetColumns());
		}

		/// <summary>
		/// The SqlGetValues
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="Dictionary{string, object}"/></returns>
		public static Dictionary<string, object> SqlGetValues(this ISqlTable ps) {
			var values = BindToDictionary(ps);
			var columns = SqlGetColumns(ps).ToHashSet(x => x.ColumnName);
			return values
				.Where(x => columns.Contains(x.Key))
				.ToDictionary(x => x.Key, x => x.Value);
		}

		/// <summary>
		/// Gets information about the fields within this object as if they
		/// were columns within an SQL Table
		/// </summary>
		/// <returns></returns>
		public static List<SqlColumn> SqlGetColumns(this ISqlTable ps) {
			if (ps == null) {
				return null;
			}
			var type = ps.GetType();
			var properties = type.GetProperties();
			var fields = type.GetFields();

			var items = new List<SqlColumn>();

			foreach (var p in properties) {
				//var obj = p.GetValue(ps);
				var isIgnore = p.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isIgnore) {
					items.Add(new SqlColumn(p.Name, p.PropertyType));
				}

			}

			foreach (var f in fields) {
				//var obj = f.GetValue(ps);
				var isIgnore = f.GetCustomAttribute<SqlIgnoreAttribute>() != null;
				if (!isIgnore) {
					items.Add(new SqlColumn(f.Name, f.FieldType));
				}

			}
			return items;
		}

		/// <summary>
		/// The SqlInsertList
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlInsertList(this ISqlTable ps) {

			return string.Join(",", SqlInsertColumns(ps));
		}

		/// <summary>
		/// The SqlTuple
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlTuple(this ISqlTable ps) {
			if (ps == null) {
				return string.Empty;
			}

			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();

			var items = new List<string>();

			foreach (var p in properties) {
				var obj = p.GetValue(ps);
				items.Add(CsvSerializer.Serialize(obj) + " AS \"" + p.Name + "\"");

			}

			foreach (var f in fields) {
				var obj = f.GetValue(ps);
				items.Add(CsvSerializer.Serialize(obj) + " AS \"" + f.Name + "\"");
			}
			return string.Join(",", items);
		}

		/// <summary>
		/// The SqlRow
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="bind">The <see cref="SqlRow"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlRow(this ISqlTable ps, SqlRow bind) {

			if (ps == null) {
				return string.Empty;
			}
			bind.Clear();

			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();
			var items = new List<string>();

			foreach (var p in properties) {
				var obj = p.GetValue(ps);
				bind[p.Name] = obj;

			}

			foreach (var f in fields) {
				var obj = f.GetValue(ps);
				bind[f.Name] = obj;
			}
			return string.Join(",", items);
		}

		/// <summary>
		/// The SqlGetFullName
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlGetFullName(this ISqlTable ps) {
			var t = ps.GetType();
			var a = t.GetTypeInfo().GetCustomAttribute<SqlTableAttribute>();
			if (a != null) {
				return $"{a.SchemaName}.{a.TableName}";
			}
			var schema = t.Namespace.Split('.').Last();
			var table = t.Name;

			return $"{schema}.{table}";
		}

		/// <summary>
		/// The SqlGetTableName
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlGetTableName(this ISqlTable ps) {
			var t = ps.GetType();
			var a = t.GetTypeInfo().GetCustomAttribute<SqlTableAttribute>();
			if (a != null) {
				return a.TableName;
			}
			var schema = t.Namespace.Split('.').Last();
			var table = t.Name;
			return table;
		}

		/// <summary>
		/// The SqlGetSchemaName
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string SqlGetSchemaName(this ISqlTable ps) {
			var t = ps.GetType();
			var a = t.GetTypeInfo().GetCustomAttribute<SqlTableAttribute>();

			if (a != null) {
				return a.SchemaName;
			}

			var schema = t.Namespace.Split('.').Last();
			var table = t.Name;
			return schema;
		}
	}
}
