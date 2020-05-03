// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using Newtonsoft.Json;

	/// <summary>
	/// Defines the <see cref="ISqlLoadable" />
	/// </summary>
	public interface ISqlLoadable : ISqlTable {
		/// <summary>
		/// Get the SourceQuery for this entity 
		/// </summary>
		/// <returns></returns>
		string GetSourceQuery();

		/// <summary>
		/// Get a reference to the loaded table suitable for querying
		/// </summary>
		/// <returns></returns>
		string GetFromReference();

		/// <summary>
		/// Get the Table Identified of the table (most often its table name)
		/// </summary>
		/// <returns></returns>
		string GetTableId();

		/// <summary>
		/// Get the schema of the table
		/// </summary>
		/// <returns></returns>
		string GetSchemaId();

		/// <summary>
		/// Returns a hash that can be used to determine if the schema of the table has changed
		/// </summary>
		/// <returns></returns>
		string GetSchemaHash();

		/// <summary>
		/// Get the keys used for this table's Primary Key
		/// </summary>
		/// <returns></returns>
		List<string> GetPrimaryKeyColumns();

		/// <summary>
		/// The GetColumns
		/// </summary>
		/// <returns>The <see cref="List{SqlColumn}"/></returns>
		List<SqlColumn> GetColumns();
	}

	/// <summary>
	/// Defines the <see cref="ISqlLoadableExtensions" />
	/// </summary>
	public static class ISqlLoadableExtensions {
		/// <summary>
		/// The GetSchema
		/// </summary>
		/// <param name="model">The <see cref="ISqlLoadable"/></param>
		/// <returns>The <see cref="SqlTable"/></returns>
		public static SqlTable GetSchema(this ISqlLoadable model) {
			return new SqlTable(model.GetTableId(), model.GetSchemaId(), model.GetColumns());
		}

		//public static string GetTableId(this ISqlLoadable model) { return model.GetType().Name; }
		/// <summary>
		/// The GetSerializers
		/// </summary>
		/// <param name="model">The <see cref="AutoSqlTable"/></param>
		/// <returns>The <see cref="List{Func{DbDataReader, int, string}}"/></returns>
		public static List<Func<DbDataReader, int, string>> GetSerializers(this ISqlLoadable model) {
			var fields = model.GetType().GetFields();
			var columns = new List<Func<DbDataReader, int, string>>();
			foreach (var f in fields) {
				if (f.IsPublic) {
					var serializer = GetSerializerForType(f.FieldType);
					columns.Add(serializer);
				}
			}
			return columns;
		}

		/// <summary>
		/// The GetSerializerForType
		/// </summary>
		/// <param name="t">The <see cref="Type"/></param>
		/// <returns>The <see cref="Func{DbDataReader, int, string}"/></returns>
		private static Func<DbDataReader, int, string> GetSerializerForType(Type t) {
			if (t == typeof(bool) || t == typeof(bool?)) {
				return (r, i) => r.GetBoolean(i) ? "1" : "0";
			}
			if (t == typeof(int) || t == typeof(int?)) {
				return (r, i) => r.GetInt32(i).ToString();
			}
			if (t == typeof(short) || t == typeof(short?)) {
				return (r, i) => r.GetInt16(i).ToString();
			}
			if (t == typeof(long) || t == typeof(long?)) {
				return (r, i) => r.GetInt64(i).ToString();
			}
			if (t == typeof(byte) || t == typeof(byte?)) {
				return (r, i) => r.GetByte(i).ToString();
			}
			if (t == typeof(decimal) || t == typeof(decimal?)) {
				return (r, i) => r.GetDecimal(i).ToString();
			}
			if (t == typeof(double) || t == typeof(double?)) {
				return (r, i) => r.GetDouble(i).ToString();
			}
			if (t == typeof(float) || t == typeof(float?)) {
				return (r, i) => r.GetFloat(i).ToString();
			}

			if (t == typeof(string)) {
				return (r, i) => {
					var text = r.GetString(i);
					var escaped = text.Replace("\\", "/").Replace("\"", "'").Replace("\t", "  ");

					return JsonConvert.SerializeObject(escaped);
				};
			}

			if (t == typeof(DateTime) || t == typeof(DateTime?)) {
				return (r, i) => r.GetDateTime(i).ToString("yyyy-MM-dd hh:mm:ss");

			}
			throw new Exception($"Unable to find Serializer for {t}");
		}
	}
}
