// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;

	/// <summary>
	/// Defines the <see cref="IEventSubscriber{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEventSubscriber<T> where T : new() {
		/// <summary>
		/// The Stream
		/// </summary>
		/// <param name="callback">The <see cref="Action{T}"/></param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
		void Stream(Action<T> callback);
	}
}
