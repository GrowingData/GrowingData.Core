// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System.Collections.Generic;
	using System.IO;
	using GrowingData.Utilities;


	/// <summary>
	/// Defines the <see cref="ISqlTableExtensions" />
	/// </summary>
	public static class ObjectCsvExtensions {


		/// <summary>
		/// The SqlIdentityColumnName
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static IEnumerable<string> ToCsvColumnNames(this object ps) {
			if (ps == null) {
				yield break;
			}

			var type = ps.GetType();
			var properties = type.GetProperties();
			var fields = type.GetFields();


			foreach (var p in properties) {
				if (p.GetMethod.IsPublic) {
					if (!p.PropertyType.IsClass || p.PropertyType == typeof(string)) {
						yield return p.Name.ToDatabaseSafeLabel();
					}
				}
			}
			foreach (var f in fields) {
				if (f.IsPublic) {
					if (!f.FieldType.IsClass || f.FieldType == typeof(string)) {
						yield return f.Name.ToDatabaseSafeLabel();
					}
				}
			}
		}

		public static List<SqlColumn> ReflectColumns(this object ps) {
			if (ps == null) {
				return null;
			}

			var cols = new List<SqlColumn>();

			var type = ps.GetType();
			var properties = type.GetProperties();
			var fields = type.GetFields();

			foreach (var p in properties) {
				if (p.GetMethod.IsPublic) {
					if (!p.PropertyType.IsClass || p.PropertyType == typeof(string)) {
						cols.Add(new SqlColumn(p.Name, p.PropertyType));
					}
				}
			}
			foreach (var f in fields) {
				if (f.IsPublic) {
					if (!f.FieldType.IsClass || f.FieldType == typeof(string)) {
						cols.Add(new SqlColumn(f.Name, f.FieldType));
					}
				}
			}
			return cols;
		}

		/// <summary>
		/// The SqlInsertColumns
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public static IEnumerable<string> ToCsvColumnValues(this object ps) {
			if (ps == null) {
				yield break;
			}
			var type = ps.GetType();

			var properties = type.GetProperties();
			var fields = type.GetFields();


			foreach (var p in properties) {
				if (p.GetMethod.IsPublic) {
					if (!p.PropertyType.IsClass || p.PropertyType == typeof(string)) {
						yield return CsvSerializer.Serialize(p.GetValue(ps));
					}
				}
			}
			foreach (var f in fields) {
				if (f.IsPublic) {
					if (!f.FieldType.IsClass || f.FieldType == typeof(string)) {
						yield return CsvSerializer.Serialize(f.GetValue(ps));
					}
				}
			}
		}

		public static string ToCsvRow(this object ps, char seperator = '\t') {
			return string.Join(seperator, ToCsvColumnValues(ps));
		}

		public static string ToCsvHeaderRow(this object ps, char seperator = '\t') {
			return string.Join(seperator, ToCsvColumnNames(ps));
		}

		public static int ToCsv(this IEnumerable<object> collection, TextWriter writer, char seperator = '\t') {
			var rowCount = 0;
			foreach (var item in collection) {
				if (rowCount == 0) {
					writer.WriteLine(ToCsvHeaderRow(item, seperator));
				}
				writer.WriteLine(ToCsvRow(item, seperator));
				rowCount++;
			}
			return rowCount;
		}
	}
}
