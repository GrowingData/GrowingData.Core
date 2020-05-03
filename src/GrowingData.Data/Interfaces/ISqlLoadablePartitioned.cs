// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="ISqlLoadablePartitioned" />
	/// </summary>
	public interface ISqlLoadablePartitioned : ISqlLoadable {
		/// <summary>
		/// The GetPartitionColumn
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		string GetPartitionColumn();
	}
}
