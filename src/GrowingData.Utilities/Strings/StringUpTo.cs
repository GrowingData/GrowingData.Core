using System;
using System.Collections.Generic;
using System.Text;

namespace GrowingData.Utilities {
	public static class StringUpTo {
		public static string UpTo(this string input, string upTo) {
			var pos = input.IndexOf(upTo);
			if (pos == -1) {
				return input;
			}

			return input.Substring(0, pos);
		}

		public static string Between(this string input, string prefix, string suffix) {
			var posPrefix = input.IndexOf(prefix);
			if (posPrefix == -1) {
				return null;
			}
			var posSuffix = input.IndexOf(suffix, posPrefix + 1);
			if (posSuffix == -1) {
				return null;
			}

			return input.Substring(posPrefix + 1, -1 + posSuffix - posPrefix);
		}

		public static IEnumerable<string> Betweens(this string input, string prefix, string suffix) {
			var posPrefix = input.IndexOf(prefix);

			while (posPrefix > -1) {
				// Can't find the start
				if (posPrefix == -1) {
					break;
				}
				var posSuffix = input.IndexOf(suffix, posPrefix + suffix.Length);
				// Can't find the end
				if (posSuffix == -1) {
					break;
				}
				if (posPrefix >= -1) {
					var start = posPrefix + prefix.Length;
					var length = posSuffix - posPrefix - prefix.Length;
					var value = input.Substring(start, length).Trim();

					if (!string.IsNullOrEmpty(value)) {
						yield return value;
					}
				}
				posPrefix = input.IndexOf(prefix, posSuffix + suffix.Length);
			}
		}
	}
}
