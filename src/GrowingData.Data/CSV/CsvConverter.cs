// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System;
	using System.Data;
	using Newtonsoft.Json;

	/// <summary>
	/// Defines the <see cref="CsvConverter" />
	/// </summary>
	public class CsvConverter {
		/// <summary>
		/// The InvalidFormatErrorMessage
		/// </summary>
		/// <param name="expected">The <see cref="DbType"/></param>
		/// <param name="value">The <see cref="string"/></param>
		/// <param name="state">The <see cref="ReaderState"/></param>
		/// <returns>The <see cref="string"/></returns>
		private static string InvalidFormatErrorMessage(DbType expected, string value, ReaderState state) {
			if (state != null) {
				return $"Error reading line: {state.LineNumber}, field: {state.FieldNumber}. Expected a DbType.{expected}, got: '{value}'.";
			} else {
				return $"Error reading value: '{value}' as type DbType.{expected}.";
			}
		}

		/// <summary>
		/// The Read
		/// </summary>
		/// <param name="val">The <see cref="string"/></param>
		/// <param name="type">The <see cref="DbType"/></param>
		/// <returns>The <see cref="object"/></returns>
		public static object Read(string val, DbType type) {
			if (IsDBNull(val)) {
				return DBNull.Value;
			}

			if (type == DbType.String) {
				if (val.StartsWith("\"") && val.EndsWith("\"")) {
					val = val.Substring(1, val.Length - 2);
				}
				return val;
			}
			if (type == DbType.DateTime) {
				try {
					// BigQuery CSV writing is a bit dumb.
					if (val.EndsWith("12:00:00")) {
						val += " AM";
					}
					return DateTime.Parse(val);
				} catch { }

				try {
					return JsonConvert.DeserializeObject<DateTime>(val);
				} catch { }

				try {
					return DateTime.ParseExact(val, "yyyy-MM-dd", null);
				} catch { }

				throw new Exception($"Unable to parse DateTime value: '{val}'");
			}
			if (type == DbType.Boolean) {
				return JsonConvert.DeserializeObject<bool>(val);
			}

			if (type == DbType.Single) {
				return JsonConvert.DeserializeObject<double>(val);
			}

			if (type == DbType.Double) {
				return JsonConvert.DeserializeObject<double>(val);
			}

			if (type == DbType.Decimal) {
				return JsonConvert.DeserializeObject<decimal>(val);
			}

			if (type == DbType.Byte) {
				return JsonConvert.DeserializeObject<byte>(val);
			}

			if (type == DbType.Int16) {
				return JsonConvert.DeserializeObject<short>(val);
			}

			if (type == DbType.Int32) {
				return JsonConvert.DeserializeObject<int>(val);
			}

			if (type == DbType.Int64) {
				return JsonConvert.DeserializeObject<long>(val);
			}

			if (type == DbType.Guid) {
				return JsonConvert.DeserializeObject<Guid>(val);
			}

			throw new InvalidOperationException($"Unable to read value '{val}', as type {type} is unknown.");
		}

		public static object Parse(string val, Type type){

			if (string.IsNullOrEmpty(val) || val == "null" || val == "NULL") {
				return DBNull.Value;
			}

			if (type == typeof(string)) {
				if (val.StartsWith("\"") && val.EndsWith("\"")) {
					return val.Substring(1, val.Length - 2);
				}
				return val;
			}
			if (type == typeof(DateTime)) {
				if (val.Length == 10) {
					return DateTime.ParseExact(val, "yyyy-MM-dd", null);
				}
				try {
					// BigQuery CSV writing is a bit dumb.
					if (val.EndsWith("12:00:00")) {
						val += " AM";
					}
					return DateTime.Parse(val);
				} catch { }

				try {
					return JsonConvert.DeserializeObject<DateTime>(val);
				} catch { }

				throw new Exception($"Unable to parse DateTime value: '{val}'");
			}

			if (type == typeof(bool)) {
				return ParseBoolean(val);
			}

			if (type == typeof(float)) {
				return (float)ParseDouble(val);
			}

			if (type == typeof(double)) {
				return ParseDouble(val);
			}

			if (type == typeof(decimal)) {
				return (decimal)ParseDouble(val);
			}

			if (type == typeof(byte)) {
				return (byte)ParseInteger(val);
			}

			if (type == typeof(short)) {
				return (short)ParseInteger(val);
			}

			if (type == typeof(int)) {
				return (int)ParseInteger(val);
			}

			if (type == typeof(long)) {
				return ParseInteger(val);
			}

			if (type == typeof(Guid)) {
				return Guid.Parse(val);
			}
			throw new InvalidOperationException($"Unable to read value '{val}', as type {type} is unknown.");
		}

		/// <summary>
		/// The ReadO1
		/// </summary>
		/// <param name="val">The <see cref="string"/></param>
		/// <param name="type">The <see cref="DbType"/></param>
		/// <returns>The <see cref="object"/></returns>
		public static object Parse(string val, DbType type) {
			if (string.IsNullOrEmpty(val) || val == "null" || val == "NULL") {
				return DBNull.Value;
			}

			if (type == DbType.String) {
				if (val.StartsWith("\"") && val.EndsWith("\"")) {
					return val.Substring(1, val.Length - 2);
				}
				return val;
			}
			if (type == DbType.DateTime) {
				if (val.Length == 10) {
					return DateTime.ParseExact(val, "yyyy-MM-dd", null);
				}
				try {
					// BigQuery CSV writing is a bit dumb.
					if (val.EndsWith("12:00:00")) {
						val += " AM";
					}
					return DateTime.Parse(val);
				} catch { }

				try {
					return JsonConvert.DeserializeObject<DateTime>(val);
				} catch { }

				throw new Exception($"Unable to parse DateTime value: '{val}'");
			}

			if (type == DbType.Boolean) {
				return ParseBoolean(val);
			}

			if (type == DbType.Single) {
				return (float)ParseDouble(val);
			}

			if (type == DbType.Double) {
				return ParseDouble(val);
			}

			if (type == DbType.Decimal) {
				return (decimal)ParseDouble(val);
			}

			if (type == DbType.Byte) {
				return (byte)ParseInteger(val);
			}

			if (type == DbType.Int16) {
				return (short)ParseInteger(val);
			}

			if (type == DbType.Int32) {
				return (int)ParseInteger(val);
			}

			if (type == DbType.Int64) {
				return ParseInteger(val);
			}

			if (type == DbType.Guid) {
				return Guid.Parse(val);
			}

			throw new InvalidOperationException($"Unable to read value '{val}', as type {type} is unknown.");
		}

		/// <summary>
		/// The ParseBoolean
		/// </summary>
		/// <param name="s">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public static bool ParseBoolean(string s) {
			if (s == "0" || s == "False" || s.ToLower() == "false") {
				return false;
			}

			if (s == "1" || s == "True" || s.ToLower() == "true") {
				return true;
			}

			return JsonConvert.DeserializeObject<bool>(s);
		}

		/// <summary>
		/// The ParseDouble
		/// </summary>
		/// <param name="s">The <see cref="string"/></param>
		/// <returns>The <see cref="double"/></returns>
		public static double ParseDouble(string s) {
			try {
				return double.Parse(s);
			} catch {
				return JsonConvert.DeserializeObject<double>(s);
			}
		}

		/// <summary>
		/// The ParseInteger
		/// </summary>
		/// <param name="s">The <see cref="string"/></param>
		/// <returns>The <see cref="long"/></returns>
		public static long ParseInteger(string s) {
			return long.Parse(s);
		}

		/// <summary>
		/// The IsDBNull
		/// </summary>
		/// <param name="val">The <see cref="string"/></param>
		/// <returns>The <see cref="bool"/></returns>
		public static bool IsDBNull(string val) {
			val = val.ToLower();

			return string.IsNullOrEmpty(val)
				|| val == "null"
				|| val == "NULL"
				|| val == "DBNull.Value";
		}
	}
}
