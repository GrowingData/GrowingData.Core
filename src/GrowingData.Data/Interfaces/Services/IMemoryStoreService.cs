#region Copyright
// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.
#endregion

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="IMemoryStoreService" />
	/// </summary>
	public interface IMemoryStoreService : IReportableLatency {

		/// <summary>
		/// The ConnectEventChannel
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionName">The <see cref="string"/></param>
		/// <param name="topic">The <see cref="string"/></param>
		/// <returns>The <see cref="IEventChannel{T}"/></returns>
		IEventChannel<T> ConnectEventChannel<T>(string connectionName, string topic) where T : new();

		/// <summary>
		/// The ConnectEventPublisher
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionName">The <see cref="string"/></param>
		/// <param name="topic">The <see cref="string"/></param>
		/// <returns>The <see cref="EventPublisherRedis{T}"/></returns>
		IEventPublisher<T> ConnectEventPublisher<T>(string connectionName, string topic) where T : new();

		/// <summary>
		/// The ConnectEventSubscriber
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionName">The <see cref="string"/></param>
		/// <param name="topic">The <see cref="string"/></param>
		/// <returns>The <see cref="EventSubscriberRedis{T}"/></returns>
		IEventSubscriber<T> ConnectEventSubscriber<T>(string connectionName, string topic) where T : new();

		/// <summary>
		/// The GetQueue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionName">The <see cref="string"/></param>
		/// <param name="queueName">The <see cref="string"/></param>
		/// <returns>The <see cref="IDistributedMemoryQueue{T}"/></returns>
		IDistributedMemoryQueue<T> GetQueue<T>(string connectionName, string queueName) where T : class, new();

		/// <summary>
		/// The GetCache
		/// </summary>
		/// <param name="connectionName">The <see cref="string"/></param>
		/// <returns>The <see cref="IDistributedMemoryCache"/></returns>
		IDistributedMemoryCache GetCache(string connectionName);


	}
}
