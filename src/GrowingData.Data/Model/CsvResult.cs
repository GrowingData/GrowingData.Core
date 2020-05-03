// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {

	/// <summary>
	/// Defines the <see cref="CsvResult" />
	/// </summary>
	public class CsvResult {
		/// <summary>
		/// Defines the CsvPath
		/// </summary>
		public string FilePath;

		/// <summary>
		/// Defines the CurrentMd5
		/// </summary>
		public string CurrentMd5;

		/// <summary>
		/// Defines the OldMd5
		/// </summary>
		public string OldMd5;

		/// <summary>
		/// Defines the Rows
		/// </summary>
		public int Rows;

		/// <summary>
		/// Defines the PartitionKey
		/// </summary>
		public int PartitionKey;

		/// <summary>
		/// Gets a value indicating whether IsChanged
		/// </summary>
		public bool IsChanged {
			get {
				if (CurrentMd5 == null || OldMd5 == null) {
					return true;
				}
				return CurrentMd5 != OldMd5;
			}
		}

		/// <summary>
		/// Defines the IsGZip
		/// </summary>
		public bool IsGZip;

		/// <summary>
		/// Defines the TableSchema
		/// </summary>
		public SqlTable TableSchema;
	}
}
