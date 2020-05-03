// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="IEmailService" />
	/// </summary>
	public interface IEmailService {
		/// <summary>
		/// The SendEmail
		/// </summary>
		/// <param name="destinations">The <see cref="string"/></param>
		/// <param name="subject">The <see cref="string"/></param>
		/// <param name="body">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool SendEmail(string destinations, string subject, string body);

		/// <summary>
		/// The Send
		/// </summary>
		/// <param name="destinations">The <see cref="string"/></param>
		/// <param name="subject">The <see cref="string"/></param>
		/// <param name="template">The <see cref="string"/></param>
		/// <param name="templateValues">The <see cref="Dictionary{string, string}"/></param>
		/// <returns>The <see cref="bool"/></returns>
		bool Send(string destinations, string subject, string template, Dictionary<string, string> templateValues);
	}
}
