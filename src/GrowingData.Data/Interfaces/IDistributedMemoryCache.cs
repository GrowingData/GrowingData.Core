// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;

	/// <summary>
	/// Defines the <see cref="IDistributedMemoryCache" />
	/// </summary>
	public interface IDistributedMemoryCache : IReportableLatency {
		/// <summary>
		/// The CacheExpire
		/// </summary>
		/// <param name="key">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool CacheExpire(string key);

		/// <summary>
		/// The CacheGet
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The <see cref="string"/></param>
		/// <returns>The <see cref="T"/></returns>
		T CacheGet<T>(string key) where T : new();

		/// <summary>
		/// The CacheGet
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The <see cref="string"/></param>
		/// <param name="fallback">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		T CacheGet<T>(string key, Func<T> fallback) where T : new();

		/// <summary>
		/// The CacheGet
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The <see cref="string"/></param>
		/// <param name="expiry">The <see cref="TimeSpan"/></param>
		/// <param name="fallback">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		T CacheGet<T>(string key, TimeSpan expiry, Func<T> fallback) where T : new();

		/// <summary>
		/// The CacheSet
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The <see cref="string"/></param>
		/// <param name="value">The <see cref="T"/></param>
		/// <param name="expiry">The <see cref="TimeSpan"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool CacheSet<T>(string key, T value, TimeSpan expiry) where T : new();

		/// <summary>
		/// The AcquireLock
		/// </summary>
		/// <param name="lockName">The <see cref="string"/></param>
		/// <param name="id">The <see cref="string"/></param>
		/// <param name="maxHoldTime">The <see cref="TimeSpan"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool AcquireLock(string lockName, string id, TimeSpan maxHoldTime);

		/// <summary>
		/// The ReleaseLock
		/// </summary>
		/// <param name="lockName">The <see cref="string"/></param>
		void ReleaseLock(string lockName);
	}
}
