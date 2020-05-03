#region Copyright
// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.
#endregion

namespace GrowingData.Data {
	using System;
	using System.Data.Common;
	using System.Data.SqlClient;
	using Npgsql;

	/// <summary>
	/// Defines the <see cref="IDatabaseService" />
	/// </summary>
	public interface IDatabaseService : IReportableLatency {

		/// <summary>
		/// The ConnectPostgres
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="Func{DbConnection}"/></returns>
		Func<DbConnection> GetConnection(string configurationSettingName);


		/// <summary>
		/// The ConnectPostgres
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <returns>The <see cref="Func{DbConnection}"/></returns>
		DbConnection Connect(string configurationSettingName);

	}
}
