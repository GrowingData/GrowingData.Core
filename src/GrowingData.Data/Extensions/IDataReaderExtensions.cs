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
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Defines the <see cref="DataReaderExtensions" />
	/// </summary>
	public static class DataReaderExtensions {
		/// <summary>
		/// The SelectRows
		/// </summary>
		/// <param name="reader">The <see cref="IDataReader"/></param>
		/// <returns>The <see cref="List{SqlRow}"/></returns>
		public static List<SqlRow> SelectRows(this IDataReader reader) {

			var columnNames = Enumerable.Range(0, reader.FieldCount)
					.Select(x => reader.GetName(x))
					.ToList();

			var rows = new List<SqlRow>();
			while (reader.Read()) {
				var row = new SqlRow(columnNames);
				for (var i = 0; i < columnNames.Count; i++) {
					row[columnNames[i]] = reader[i];
				}
				rows.Add(row);
			}

			return rows;
		}

		/// <summary>
		/// The GetColumnNameKeys
		/// </summary>
		/// <param name="r">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="Dictionary{string, string}"/></returns>
		public static Dictionary<string, string> GetColumnNameKeys(DbDataReader r) {
			var columnNames = new Dictionary<string, string>();
			for (var i = 0; i < r.FieldCount; i++) {
				var key = DataReaderExtensions.StandardiseName(r.GetName(i));
				columnNames[key] = r.GetName(i);
			}
			return columnNames;
		}

		/// <summary>
		/// Binds the current row in the reader to a new object of Type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <returns></returns>
		public static T ReflectResult<T>(this DbDataReader r) where T : new() {

			var type = typeof(T);
			var properties = ReflectPropertyKeys(type);
			var fields = ReflectFieldKeys(type);

			Dictionary<string, string> columnNames = null;

			var obj = new T();
			if (columnNames == null) {
				columnNames = DataReaderExtensions.GetColumnNameKeys(r);
			}

			BindProperties(r, properties, columnNames, obj);
			BindFields(r, fields, columnNames, obj);

			return obj;
		}

		/// <summary>
		/// The StandardiseName
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string StandardiseName(string name) {
			return name.ToLower().Replace("_", "");
		}

		/// <summary>
		/// The ReflectFieldKeys
		/// </summary>
		/// <param name="type">The <see cref="Type"/></param>
		/// <returns>The <see cref="Dictionary{string, FieldInfo}"/></returns>
		public static Dictionary<string, FieldInfo> ReflectFieldKeys(Type type) {
			return type.GetFields().ToDictionary(x => DataReaderExtensions.StandardiseName(x.Name));
		}

		/// <summary>
		/// The ReflectPropertyKeys
		/// </summary>
		/// <param name="type">The <see cref="Type"/></param>
		/// <returns>The <see cref="Dictionary{string, PropertyInfo}"/></returns>
		public static Dictionary<string, PropertyInfo> ReflectPropertyKeys(Type type) {
			return type.GetProperties().ToDictionary(x => DataReaderExtensions.StandardiseName(x.Name));
		}

		/// <summary>
		/// The BindFields
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r">The <see cref="DbDataReader"/></param>
		/// <param name="fields">The <see cref="Dictionary{string, FieldInfo}"/></param>
		/// <param name="columnNames">The <see cref="Dictionary{string, string}"/></param>
		/// <param name="obj">The <see cref="T"/></param>
		public static void BindFields<T>(DbDataReader r,
		Dictionary<string, FieldInfo> fields,
		Dictionary<string, string> columnNames,
		T obj) where T : new() {
			foreach (var p in fields) {
				if (columnNames.ContainsKey(p.Key)) {
					var columnName = columnNames[p.Key];
					if (r[columnName] != DBNull.Value) {
						if (p.Value.FieldType == typeof(int)
							&& r[columnName].GetType() == typeof(long)) {

							p.Value.SetValue(obj, (int)(long)r[columnName]);
						} else {

							p.Value.SetValue(obj, r[columnName]);
						}

					} else {
						if (p.Value.FieldType.GetTypeInfo().IsClass) {
							p.Value.SetValue(obj, null);
						}
						// Nullable value like "int?"
						if (Nullable.GetUnderlyingType(p.Value.FieldType) != null) {
							p.Value.SetValue(obj, null);
						}

					}
				}
			}
		}

		/// <summary>
		/// The BindProperties
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r">The <see cref="DbDataReader"/></param>
		/// <param name="properties">The <see cref="Dictionary{string, PropertyInfo}"/></param>
		/// <param name="columnNames">The <see cref="Dictionary{string, string}"/></param>
		/// <param name="obj">The <see cref="T"/></param>
		public static void BindProperties<T>(DbDataReader r,
			Dictionary<string, PropertyInfo> properties,
			Dictionary<string, string> columnNames,
			T obj) where T : new() {
			foreach (var p in properties) {
				if (!p.Value.CanWrite) {
					continue;
				}
				if (columnNames.ContainsKey(p.Key)) {
					var columnName = columnNames[p.Key];
					if (r[columnName] != DBNull.Value) {

						if (p.Value.PropertyType == typeof(int)
							&& r[columnName].GetType() == typeof(long)) {

							p.Value.SetValue(obj, (int)r[columnName]);
						}

						p.Value.SetValue(obj, r[columnName]);
					} else {
						if (p.Value.PropertyType.GetTypeInfo().IsClass) {
							p.Value.SetValue(obj, null);
						}
						// Nullable value like "int?"
						if (Nullable.GetUnderlyingType(p.Value.PropertyType) != null) {
							p.Value.SetValue(obj, null);

						}
					}
				}
			}
		}



		/// <summary>
		/// The GetColumnNameKeys
		/// </summary>
		/// <param name="r">The <see cref="DbDataReader"/></param>
		/// <returns>The <see cref="Dictionary{string, string}"/></returns>
		public static List<SqlColumn> GetColumnSchema(this IDataReader r) {
			var columns = new List<SqlColumn>();
			for (var i = 0; i < r.FieldCount; i++) {
				var name = r.GetName(i);
				var type = r.GetFieldType(i);
				var column = new SqlColumn(name, type);
				columns.Add(column);
			}
			return columns;
		}

	}
}
