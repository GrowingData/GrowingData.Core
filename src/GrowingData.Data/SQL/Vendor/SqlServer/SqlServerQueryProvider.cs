// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="SqlServerQueryProvider" />
	/// </summary>
	public class SqlServerQueryProvider : SqlQueryProvider {
		/// <summary>
		/// The GetTypeMap
		/// </summary>
		/// <returns>The <see cref="List{SqlTypeMap}"/></returns>
		public override List<SqlTypeMap> GetTypeMap() {
			return new List<SqlTypeMap>() {
				new SqlTypeMap(typeof(string), "nvarchar(512) NULL"),
				new SqlTypeMap(typeof(bool), "bit  NOT NULL"),

				new SqlTypeMap(typeof(DateTime), "smalldatetime  NOT NULL"),

				new SqlTypeMap(typeof(float), "float  NOT NULL"),
				new SqlTypeMap(typeof(double), "float NOT NULL"),

				new SqlTypeMap(typeof(decimal), "money NOT NULL"),

				new SqlTypeMap(typeof(byte), "tinyint NOT NULL"),
				new SqlTypeMap(typeof(short), "smallint NOT NULL"),
				new SqlTypeMap(typeof(int), "int NOT NULL"),
				new SqlTypeMap(typeof(long), "bigint NOT NULL"),
				new SqlTypeMap(typeof(Guid), "uniqueidentifier NOT NULL"),



				new SqlTypeMap(typeof(bool?), "bit NULL"),

				new SqlTypeMap(typeof(DateTime?), "smalldatetime NULL"),

				new SqlTypeMap(typeof(float?), "float NULL"),
				new SqlTypeMap(typeof(double?), "float NULL"),

				new SqlTypeMap(typeof(decimal?), "money NULL"),

				new SqlTypeMap(typeof(byte?), "tinyint NULL"),
				new SqlTypeMap(typeof(short?), "smallint NULL"),
				new SqlTypeMap(typeof(int?), "int NULL"),
				new SqlTypeMap(typeof(long?), "bigint NULL"),
				new SqlTypeMap(typeof(Guid?), "uniqueidentifier NULL")
			};
		}

		/// <summary>
		/// Gets the StringTypeKey
		/// </summary>
		public override string StringTypeKey {
			get { return "NVARCHAR(512)"; }
		}

		/// <summary>
		/// Gets the StringTypeLong
		/// </summary>
		public override string StringTypeLong {
			get { return "NVARCHAR(MAX)"; }
		}

		/// <summary>
		/// The GetTableName
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetTableName(object ps) {
			return base.GetTableName(ps).Replace("ciq_", "");
		}

		/// <summary>
		/// The DropTableIfExistsSql
		/// </summary>
		/// <param name="ps">The <see cref="object"/></param>
		/// <param name="fullName">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public string DropTableIfExistsSql(object ps, string fullName) {
			return $@"
				IF OBJECT_ID('{fullName}', 'U') IS NOT NULL
				BEGIN
					DROP TABLE {GetTableReference(ps)}
				END
			";
		}
	}
}
