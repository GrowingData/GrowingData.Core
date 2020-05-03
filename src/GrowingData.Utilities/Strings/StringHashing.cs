// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="StringHashing" />
	/// </summary>
	public static class StringHashing {
		/// <summary>
		/// The HashStringSHA1
		/// </summary>
		/// <param name="valueUTF8">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string HashStringSHA1(this string valueUTF8) {
			var valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			var sha1 = SHA1.Create();
			var hashBytes = sha1.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");
		}

		/// <summary>
		/// The HashStringMD5
		/// </summary>
		/// <param name="valueUTF8">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string HashStringMD5(this string valueUTF8) {
			var valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			var md5 = MD5.Create();
			var hashBytes = md5.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");
		}
		/// <summary>
		/// Calculate the MD5 hash of the string, then return the first 32 bits
		/// of data as an integer
		/// </summary>
		/// <param name="valueUTF8">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static int HashStringToInteger(this string valueUTF8) {
			var valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			var md5 = MD5.Create();
			var hashBytes = md5.ComputeHash(valueBytes);
			var ivalue = BitConverter.ToInt32(hashBytes, 0);
			return ivalue;
		}


		/// <summary>
		/// The HashStringSHA256
		/// </summary>
		/// <param name="valueUTF8">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string HashStringSHA256(this string valueUTF8) {
			var valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");
		}

		/// <summary>
		/// The CreateSalt
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public static string CreateSalt() {
			//Generate a cryptographic random number.
			var size = 256;
			using (var rng = RandomNumberGenerator.Create()) {
				var buff = new byte[size];
				rng.GetBytes(buff);

				// Return a Base64 string representation of the random number.
				return Convert.ToBase64String(buff);
			}
		}

		/// <summary>
		/// The HashStrings
		/// </summary>
		/// <param name="valueUTF8">The <see cref="string"/></param>
		/// <param name="saltBase64">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string HashStrings(string valueUTF8, string saltBase64) {
			var valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			var saltBytes = Convert.FromBase64String(saltBase64);

			var hashBytes = HashBytes(valueBytes, saltBytes);

			return Convert.ToBase64String(hashBytes);
		}

		/// <summary>
		/// The HashBytes
		/// </summary>
		/// <param name="value">The <see cref="byte[]"/></param>
		/// <param name="salt">The <see cref="byte[]"/></param>
		/// <returns>The <see cref="byte[]"/></returns>
		public static byte[] HashBytes(byte[] value, byte[] salt) {
			var saltedValue = value.Concat(salt).ToArray();
			// Alternatively use CopyTo.
			//var saltedValue = new byte[value.Length + salt.Length];
			//value.CopyTo(saltedValue, 0);
			//salt.CopyTo(saltedValue, value.Length);

			return SHA256.Create().ComputeHash(saltedValue);
		}
	}
}
