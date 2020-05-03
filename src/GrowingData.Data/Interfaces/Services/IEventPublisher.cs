// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="IEventPublisher{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEventPublisher<T> where T : new() {
		/// <summary>
		/// The Publish
		/// </summary>
		/// <param name="message">The <see cref="T"/></param>
		/// <param name="retry">The <see cref="int"/></param>
		void Publish(T message, int retry = 0);
	}
}
