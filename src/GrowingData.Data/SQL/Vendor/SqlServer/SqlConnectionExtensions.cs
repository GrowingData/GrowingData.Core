// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Data.SqlClient;

	/// <summary>
	/// Defines the <see cref="SqlConnectionExtensions" />
	/// </summary>
	public static class SqlConnectionExtensions {
		/// <summary>
		/// The BulkCopy
		/// </summary>
		/// <param name="sourceConnection">The <see cref="SqlConnection"/></param>
		/// <param name="sql">The <see cref="string"/></param>
		/// <param name="destinationConnection">The <see cref="SqlConnection"/></param>
		/// <param name="tableName">The <see cref="string"/></param>
		/// <param name="notify">The <see cref="Action{long}"/></param>
		public static void BulkCopy(this SqlConnection sourceConnection, string sql,
			SqlConnection destinationConnection, string tableName, Action<long> notify) {

			var reader = sourceConnection.ExecuteReader(sql, null);
			var copy = new SqlBulkCopy(destinationConnection);
			copy.BulkCopyTimeout = 0;
			copy.BatchSize = 10000;
			copy.DestinationTableName = tableName;
			copy.EnableStreaming = true;

			copy.NotifyAfter = 10000;
			if (notify != null) {
				copy.SqlRowsCopied += (sender, e) => {
					notify(e.RowsCopied);

				};
			}
			copy.WriteToServer(reader);

			return;
		}

		/// <summary>
		/// The Copy_SqlRowsCopied
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="e">The <see cref="SqlRowsCopiedEventArgs"/></param>
		private static void Copy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) {
			throw new NotImplementedException();
		}
	}
}
