// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="IEventChannel{T}" />
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEventChannel<T>
		: IEventPublisher<T>, IEventSubscriber<T>
		where T : new() {
	}
}
