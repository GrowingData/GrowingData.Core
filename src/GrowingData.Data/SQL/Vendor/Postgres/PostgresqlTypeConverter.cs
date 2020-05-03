// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using NpgsqlTypes;

	/// <summary>
	/// Defines the <see cref="PostgresqlType" />
	/// </summary>
	public class PostgresqlType {
		/// <summary>
		/// Defines the InfoSchemaName
		/// </summary>
		public string InfoSchemaName;

		/// <summary>
		/// Defines the CreateColumnDefinition
		/// </summary>
		public string CreateColumnDefinition;

		/// <summary>
		/// Defines the PostgresqlDbType
		/// </summary>
		public NpgsqlDbType PostgresqlDbType;

		/// <summary>
		/// Defines the DatabaseType
		/// </summary>
		public DbType DatabaseType;

		/// <summary>
		/// Initializes a new instance of the <see cref="PostgresqlType"/> class.
		/// </summary>
		/// <param name="postgresqlColumnType">The <see cref="string"/></param>
		/// <param name="infoSchemaName">The <see cref="string"/></param>
		/// <param name="postgresqlDbType">The <see cref="NpgsqlDbType"/></param>
		/// <param name="databaseType">The <see cref="DbType"/></param>
		/// <param name="code">The <see cref="SimpleDbTypeCode"/></param>
		public PostgresqlType(string postgresqlColumnType, string infoSchemaName, NpgsqlDbType postgresqlDbType, DbType databaseType) {
			InfoSchemaName = infoSchemaName;
			CreateColumnDefinition = postgresqlColumnType;
			PostgresqlDbType = postgresqlDbType;
			DatabaseType = databaseType;
		}
	}

	/// <summary>
	/// Defines the <see cref="PostgresqlTypeConverter" />
	/// </summary>
	public class PostgresqlTypeConverter : DbTypeConverter {
		/// <summary>
		/// Defines the IgnoreTypes
		/// </summary>
		public static HashSet<string> IgnoreTypes = new HashSet<string>() {
			"json"
		};

		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="type">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="PostgresqlType"/></returns>
		public static PostgresqlType Get(SimpleDbType type) {
			var t = Types.FirstOrDefault(p => p.DatabaseType == type.DatabaseType);
			if (t == null) {
				throw new Exception($"Unable to find PostgresqlType for: '{type.DatabaseType.ToString()}' ({type.DotNetType})");
			}

			return t;
		}

		/// <summary>
		///  The ordering of these type sis important, as most lookups of this table
		///  will use .FirstOrDefault
		/// </summary>
		public static List<PostgresqlType> Types = new List<PostgresqlType>() {

			new PostgresqlType("text", "text", NpgsqlDbType.Text, DbType.String),
			new PostgresqlType("bool", "bool", NpgsqlDbType.Boolean, DbType.Boolean),

			new PostgresqlType("timestamptz", "timestamp with time zone", NpgsqlDbType.TimestampTz, DbType.DateTime),

			new PostgresqlType("double precision", "double precision", NpgsqlDbType.Double, DbType.Double),
			new PostgresqlType("numeric(28,6)", "numeric", NpgsqlDbType.Numeric, DbType.Decimal),
			new PostgresqlType("numeric(28,6)", "numeric", NpgsqlDbType.Real, DbType.Decimal),

			new PostgresqlType("bigint","bigint", NpgsqlDbType.Bigint, DbType.Int64),

			new PostgresqlType("uuid", "uuid",NpgsqlDbType.Uuid, DbType.Guid),


			// Lets add in some type alias'
			new PostgresqlType("bool", "boolean", NpgsqlDbType.Boolean, DbType.Boolean),
			new PostgresqlType("timestamptz", "timestamp", NpgsqlDbType.Timestamp, DbType.DateTime),
			new PostgresqlType("timestamptz", "date", NpgsqlDbType.Date, DbType.DateTime),
			new PostgresqlType("timestamptz", "time", NpgsqlDbType.Time, DbType.DateTime),
			new PostgresqlType("timestamptz", "time with time zone", NpgsqlDbType.TimeTz, DbType.DateTime),

			new PostgresqlType("double precision", "real", NpgsqlDbType.TimestampTz, DbType.DateTime),
			new PostgresqlType("real", "numeric", NpgsqlDbType.TimestampTz, DbType.DateTime),
			new PostgresqlType("bigint","smallint", NpgsqlDbType.Bigint, DbType.Int64),
			new PostgresqlType("bigint","integer", NpgsqlDbType.Bigint, DbType.Int64)


		};

		/// <summary>
		/// The GetCreateColumnDefinition
		/// </summary>
		/// <param name="type">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="string"/></returns>
		public override string GetCreateColumnDefinition(SimpleDbType type) {
			var t = Get(type);
			if (t == null) {
				throw new InvalidOperationException($"SimpleDbType for {type.DotNetType} is unknown to PostgresqlDbTypeConverter");
			}
			return t.CreateColumnDefinition;
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

			var t = Types.FirstOrDefault(x => x.InfoSchemaName == infoSchemaName);
			if (t == null) {
				throw new InvalidOperationException($"SimpleDbType for {infoSchemaName} is unknown to PostgresqlDbTypeConverter");
			}
			return SimpleDbType.Get(t.DatabaseType);
		}
	}
}
