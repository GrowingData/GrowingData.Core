// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="DbTypeConverter" />
	/// </summary>
	public abstract class DbTypeConverter {
		/// <summary>
		/// The GetCreateColumnDefinition
		/// </summary>
		/// <param name="type">The <see cref="SimpleDbType"/></param>
		/// <returns>The <see cref="string"/></returns>
		public abstract string GetCreateColumnDefinition(SimpleDbType type);

		/// <summary>
		/// The GetTypeFromInformationSchema
		/// </summary>
		/// <param name="infoSchemaType">The <see cref="string"/></param>
		/// <returns>The <see cref="SimpleDbType"/></returns>
		public abstract SimpleDbType GetTypeFromInformationSchema(string infoSchemaType);

		/// <summary>
		/// Defines the SqlServer
		/// </summary>
		public static DbTypesSqlAzure SqlServer = new DbTypesSqlAzure();

		/// <summary>
		/// Defines the Postgresql
		/// </summary>
		public static PostgresqlTypeConverter Postgresql = new PostgresqlTypeConverter();
	}
}
