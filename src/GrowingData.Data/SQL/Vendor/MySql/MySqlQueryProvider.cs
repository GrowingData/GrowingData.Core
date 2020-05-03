// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="MySqlQueryProvider" />
	/// </summary>
	public class MySqlQueryProvider : SqlQueryProvider {
		/// <summary>
		/// Defines the _mySqlTypeConverterMap
		/// </summary>
		protected List<SqlTypeMap> _mySqlTypeConverterMap;

		/// <summary>
		/// The GetTypeMap
		/// </summary>
		/// <returns>The <see cref="List{SqlTypeMap}"/></returns>
		public override List<SqlTypeMap> GetTypeMap() {
			return new List<SqlTypeMap>() {
				new SqlTypeMap(typeof(string), "VARCHAR(512) NULL"),
				new SqlTypeMap(typeof(bool), "bit NOT NULL"),

				new SqlTypeMap(typeof(DateTime), "DATETIME NOT NULL"),

				new SqlTypeMap(typeof(float), "float NOT NULL"),
				new SqlTypeMap(typeof(double), "float NOT NULL"),

				new SqlTypeMap(typeof(decimal), "NUMERIC(28,6) NOT NULL"),

				new SqlTypeMap(typeof(byte), "smallint NOT NULL"),
				new SqlTypeMap(typeof(short), "smallint NOT NULL"),
				new SqlTypeMap(typeof(int), "int NOT NULL"),
				new SqlTypeMap(typeof(long), "bigint NOT NULL"),
				new SqlTypeMap(typeof(Guid), "uniqueidentifier NOT NULL"),



				new SqlTypeMap(typeof(bool?), "bit NULL"),

				new SqlTypeMap(typeof(DateTime?), "DATETIME NULL"),

				new SqlTypeMap(typeof(float?), "float NULL"),
				new SqlTypeMap(typeof(double?), "float NULL"),

				new SqlTypeMap(typeof(decimal?), "NUMERIC(28,6) NULL"),

				new SqlTypeMap(typeof(byte?), "tinyint NULL"),
				new SqlTypeMap(typeof(short?), "smallint NULL"),
				new SqlTypeMap(typeof(int?), "int NULL"),
				new SqlTypeMap(typeof(long?), "bigint NULL"),
				new SqlTypeMap(typeof(Guid?), "uniqueidentifier NULL")
			};
		}

		/// <summary>
		/// The GetTableReference
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetTableReference(object ps) {
			return GetTableName(ps);
		}
	}
}
