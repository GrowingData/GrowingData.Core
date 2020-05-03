// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	/// <summary>
	/// Defines the <see cref="ISqlLoadableDynamic" />
	/// </summary>
	public interface ISqlLoadableDynamic : ISqlLoadable {
		/// <summary>
		/// The GetSerializedValue
		/// </summary>
		/// <param name="field">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		string GetSerializedValue(string field);

		/// <summary>
		/// The GetValue
		/// </summary>
		/// <param name="field">The <see cref="string"/></param>
		/// <returns>The <see cref="object"/></returns>
		object GetValue(string field);
	}
}
