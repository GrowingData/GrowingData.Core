// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	/// <summary>
	/// Defines the <see cref="ArgumentBinder" />
	/// </summary>
	public class ArgumentBinder {
		/// <summary>
		/// The Bind
		/// </summary>
		/// <param name="args">The <see cref="string[]"/></param>
		/// <returns>The <see cref="Dictionary{string, string}"/></returns>
		public static Dictionary<string, string> Bind(string[] args) {
			var parameters = new Dictionary<string, string>();

			if (args.Length == 0) {
				return parameters;
			}

			var firstNamed = -1;
			for (var i = 0; i < args.Length; i++) {
				if (args[i].StartsWith("--")) {
					firstNamed = i;
					break;
				}
			}

			if (firstNamed == -1) {
				return parameters;
			}
			for (var i = firstNamed; i < args.Length; i += 2) {
				var argName = args[i + 0];
				var argValue = args[i + 1];

				if (!argName.StartsWith("--")) {
					throw new Exception("Named arguments must start with '--'");
				}

				var reflectedName = argName.Substring(2);
				parameters[reflectedName] = argValue;

			}
			return parameters;
		}

		/// <summary>
		/// The BindJson
		/// </summary>
		/// <param name="args">The <see cref="string[]"/></param>
		/// <returns>The <see cref="string"/></returns>
		public static string BindJson(string[] args) {
			var parameters = Bind(args);
			var json = JsonConvert.SerializeObject(parameters);
			return json;
		}

		/// <summary>
		/// The Bind
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="args">The <see cref="string[]"/></param>
		/// <returns>The <see cref="T"/></returns>
		public T Bind<T>(string[] args) {
			// Binds arguments to a 
			var json = BindJson(args);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
