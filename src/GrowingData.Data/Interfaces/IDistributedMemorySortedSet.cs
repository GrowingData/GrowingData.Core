// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System.Collections.Generic;
	using System.Linq;
	/// <summary>
	/// Defines the <see cref="IDistributedMemoryQueue{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDistributedMemorySortedSet<T> where T : new() {

		/// <summary>
		/// The Push
		/// </summary>
		/// <param name="value">The <see cref="T"/></param>
		void Add(string key, T value);

		List<T> ListRangeByRank(long startIndex, long length, bool ascending);
		List<T> ListRangeByScore(double startScore, double endScore, bool ascending);
		long? Rank(T value);
		bool Remove(T value);
		long RemoveRangeByRank(long startIndex, long length);
		long RemoveRangeByScore(double startScore, double endScore);
	}
}
