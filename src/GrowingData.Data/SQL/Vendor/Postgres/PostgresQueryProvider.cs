#region Copyright
// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.
#endregion

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Linq;
	using System.Reflection;
	using GrowingData.Utilities;

	/// <summary>
	/// Defines the <see cref="PostgresQueryProvider" />
	/// </summary>
	public class PostgresQueryProvider : SqlQueryProvider {
		/// <summary>
		/// The SqlInsertColumns
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="underscoreCase">The <see cref="bool"/></param>
		/// <returns>The <see cref="List{string}"/></returns>
		public List<string> SqlInsertColumns(ISqlTable ps, bool underscoreCase) {
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
					items.Add(underscoreCase ? p.Name.ToDnsSafeLabel() : p.Name);
				}

			}

			foreach (var f in fields) {
				//var obj = f.GetValue(ps);
				var isId = f.GetCustomAttribute<SqlTableIdentityAttribute>() != null;
				var isIgnore = f.GetCustomAttribute<SqlIgnoreAttribute>() != null;

				if (!isId && !isIgnore) {
					items.Add(underscoreCase ? f.Name.ToDnsSafeLabel() : f.Name);
				}

			}
			return items;
		}

		/// <summary>
		/// The SqlInsert
		/// </summary>
		/// <param name="ps">The <see cref="ISqlTable"/></param>
		/// <param name="cn">The <see cref="DbConnection"/></param>
		/// <param name="table">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public int SqlInsert(ISqlTable ps, DbConnection cn, string table) {
			var columns = SqlInsertColumns(ps, true);
			var parameters = SqlInsertColumns(ps, false);
			var sql = $@"INSERT INTO {table} ({string.Join(",", columns)})
				VALUES ({string.Join(",", parameters.Select(x => "@" + x))}) 

			";
			return cn.ExecuteNonQuery(sql, ps);
		}

		/// <summary>
		/// The GetTypeMap
		/// </summary>
		/// <returns>The <see cref="List{SqlTypeMap}"/></returns>
		public override List<SqlTypeMap> GetTypeMap() {
			return new List<SqlTypeMap>() {
				new SqlTypeMap(typeof(string), "text NULL"),
				new SqlTypeMap(typeof(bool), "boolean NOT NULL"),

				new SqlTypeMap(typeof(DateTime), "timestamp NOT NULL"),

				new SqlTypeMap(typeof(float), "float  NOT NULL"),
				new SqlTypeMap(typeof(double), "float NOT NULL"),

				new SqlTypeMap(typeof(decimal), "numeric(28,6) NOT NULL"),

				new SqlTypeMap(typeof(byte), "smallint NOT NULL"),
				new SqlTypeMap(typeof(short), "smallint NOT NULL"),
				new SqlTypeMap(typeof(int), "int NOT NULL"),
				new SqlTypeMap(typeof(long), "bigint NOT NULL"),
				new SqlTypeMap(typeof(Guid), "uniqueidentifier NOT NULL"),



				new SqlTypeMap(typeof(bool?), "boolean NULL"),

				new SqlTypeMap(typeof(DateTime?), "timestamp NULL"),

				new SqlTypeMap(typeof(float?), "float NULL"),
				new SqlTypeMap(typeof(double?), "float NULL"),

				new SqlTypeMap(typeof(decimal?), "numeric(28,6) NULL"),

				new SqlTypeMap(typeof(byte?), "tinyint NULL"),
				new SqlTypeMap(typeof(short?), "smallint NULL"),
				new SqlTypeMap(typeof(int?), "int NULL"),
				new SqlTypeMap(typeof(long?), "bigint NULL"),
				new SqlTypeMap(typeof(Guid?), "uniqueidentifier NULL")
			};
		}
	}
}
