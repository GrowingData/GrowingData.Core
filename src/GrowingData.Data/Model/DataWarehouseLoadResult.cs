// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="DataWarehouseLoadResult" />
	/// </summary>
	public class DataWarehouseLoadResult {
		/// <summary>
		/// Defines the TableId
		/// </summary>
		public string TableId;

		/// <summary>
		/// Defines the DatasetId
		/// </summary>
		public string DatasetId;

		/// <summary>
		/// Defines the TotalBytesProcessed
		/// </summary>
		public long TotalBytesProcessed;
		public long RowsProcessed;
	}
}
