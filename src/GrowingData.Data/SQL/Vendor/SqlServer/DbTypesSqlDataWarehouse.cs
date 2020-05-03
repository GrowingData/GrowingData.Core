// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="DbTypesSqlDataWarehouse" />
	/// </summary>
	public class DbTypesSqlDataWarehouse : DbTypeConverter {
		/// <summary>
		/// Defines the IgnoreTypes
		/// </summary>
		public static HashSet<string> IgnoreTypes = new HashSet<string>() {
			"geography"
		};

		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="type">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="SqlServerType"/></returns>
		public static SqlServerType Get(SimpleDbType type) {
			var t = Types.FirstOrDefault(p => p.MungType.Code == type.Code);
			if (t == null) {
				throw new Exception($"Unable to find PostgresqlType for: '{type.Code.ToString()}' ({type.DotNetType})");
			}

			return t;
		}

		/// <summary>
		/// Defines the Types
		/// </summary>
		public static List<SqlServerType> Types = new List<SqlServerType>() {
			// No NVARCHAR in SQL Server Data Warehouse Column Store indexes
			// https://msdn.microsoft.com/en-us/library/gg492153.aspx
			new SqlServerType(SimpleDbType.Get(typeof(string)), "nvarchar", "nvarchar(MAX)"),
			new SqlServerType(SimpleDbType.Get(typeof(string)), "varchar", "nvarchar(MAX)"),


			new SqlServerType(SimpleDbType.Get(typeof(bool)), "bit", "bit"),

			new SqlServerType(SimpleDbType.Get(typeof(DateTime)), "datetime", "datetime"),
			new SqlServerType(SimpleDbType.Get(typeof(DateTime)), "datetime2", "datetime2"),
			new SqlServerType(SimpleDbType.Get(typeof(DateTime)), "smalldatetime", "smalldatetime"),
			new SqlServerType(SimpleDbType.Get(typeof(DateTime)), "date", "date"),
			new SqlServerType(SimpleDbType.Get(typeof(DateTime)), "datetimeoffset", "datetimeoffset"),

			new SqlServerType(SimpleDbType.Get(typeof(string)), "char", "char(100)"),
			new SqlServerType(SimpleDbType.Get(typeof(float)), "float", "float"),
			new SqlServerType(SimpleDbType.Get(typeof(double)), "float", "float"),
			new SqlServerType(SimpleDbType.Get(typeof(double)), "real", "real"),

			new SqlServerType(SimpleDbType.Get(typeof(decimal)), "money", "money"),
			new SqlServerType(SimpleDbType.Get(typeof(decimal)), "decimal", "decimal"),

			new SqlServerType(SimpleDbType.Get(typeof(byte)), "tinyint", "tinyint"),
			new SqlServerType(SimpleDbType.Get(typeof(short)), "smallint", "smallint"),
			new SqlServerType(SimpleDbType.Get(typeof(int)), "int", "int"),
			new SqlServerType(SimpleDbType.Get(typeof(long)), "bigint", "bigint"),
			new SqlServerType(SimpleDbType.Get(typeof(Guid)), "uniqueidentifier", "uniqueidentifier")
		};

		/// <summary>
		/// The GetCreateColumnDefinition
		/// </summary>
		/// <param name="type">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetCreateColumnDefinition(SimpleDbType type) {
			var t = Types.FirstOrDefault(x => x.MungType == type);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {type.DotNetType} is unknown to SqlServerTypeConverter");
			}
			return t.DatabaseTypeName;
		}

		/// <summary>
		/// The GetTypeFromInformationSchema
		/// </summary>
		/// <param name="infoSchemaName">The <see cref="string"/></param>
		/// <returns>The <see cref="SimpleDbType"/></returns>
		public override SimpleDbType GetTypeFromInformationSchema(string infoSchemaName) {
			if (IgnoreTypes.Contains(infoSchemaName)) {
				return null;
			}

			// Only support VARCHAR because SQL Server Data Warehouse doesnt support NVARCHAR(MAX)
			if (infoSchemaName == "nvarchar") {
				infoSchemaName = "varchar";
			}

			var t = Types.FirstOrDefault(x => x.InfoSchemaName == infoSchemaName);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {infoSchemaName} is unknown to SqlServerTypeConverter");
			}
			return t.MungType;
		}
	}
}
