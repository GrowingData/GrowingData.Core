// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="RandomString" />
	/// </summary>
	public static class RandomString {
		/// <summary>
		/// The Get
		/// </summary>
		/// <param name="length">The <see cref="int"/></param>
		/// <param name="allowedChars">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string Get(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") {
			if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
			if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

			const int byteSize = 0x100;
			var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
			if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

			// Guid.NewGuid and System.Random are not particularly random. By using a
			// cryptographically-secure random number generator, the caller is always
			// protected, regardless of use.
			using (var rng = RandomNumberGenerator.Create()) {
				var result = new StringBuilder();
				var buf = new byte[128];
				while (result.Length < length) {
					rng.GetBytes(buf);
					for (var i = 0; i < buf.Length && result.Length < length; ++i) {
						// Divide the byte into allowedCharSet-sized groups. If the
						// random value falls into the last group and the last group is
						// too small to choose from the entire allowedCharSet, ignore
						// the value in order to avoid biasing the result.
						var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
						if (outOfRangeStart <= buf[i]) continue;
						result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
					}
				}
				return result.ToString();
			}
		}
	}
}
