// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="StringParser" />
	/// </summary>
	public static class StringEmailToName {

		public static string ToNameFromEmail(this string email) {
			if (string.IsNullOrEmpty(email)) {
				return null;
			}

			return email.Split('@').First().ToLower().Replace(".", " ");
		}

	}
}
