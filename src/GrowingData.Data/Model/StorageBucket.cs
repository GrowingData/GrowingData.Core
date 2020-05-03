// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Just a fancy wrapper for "string" to make method signatures a bit nicer
	/// </summary>
	public class StorageBucket {
		/// <summary>
		/// Defines the _bucketReference
		/// </summary>
		private string _bucketReference;

		/// <summary>
		/// Prevents a default instance of the <see cref="StorageBucket"/> class from being created.
		/// </summary>
		/// <param name="bucketReference">The <see cref="string"/></param>
		private StorageBucket(string bucketReference) {
			_bucketReference = bucketReference;
		}

		/// <summary>
		/// The ToString
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public override string ToString() {
			return _bucketReference;
		}



		public static implicit operator string(StorageBucket b) {
			return b._bucketReference;
		}
		public static implicit operator StorageBucket(string bucketReference) {
			return new StorageBucket(bucketReference);
		}
	}
}
