// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	/// <summary>
	/// Defines the <see cref="IDistributedMemoryQueue{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISortableItem
	{
		double SortKey { get; }
	}
}
