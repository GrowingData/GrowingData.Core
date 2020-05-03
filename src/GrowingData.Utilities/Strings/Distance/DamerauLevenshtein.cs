// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="DamerauLevenshtein" />
	/// </summary>
	public static class DamerauLevenshtein {
		/// <summary>
		/// The DistanceDamerauLevenshtein
		/// </summary>
		/// <param name="source">The <see cref="string"/></param>
		/// <param name="target">The <see cref="string"/></param>
		/// <param name="threshold">The <see cref="int"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int DistanceDamerauLevenshtein(this string source, string target, int threshold) {
			return Distance(
				source.Select(x => (int)x).ToArray(),
				target.Select(x => (int)x).ToArray(),
				threshold);
		}

		/// <summary>
		/// The Distance
		/// </summary>
		/// <param name="source">The <see cref="int[]"/></param>
		/// <param name="target">The <see cref="int[]"/></param>
		/// <param name="threshold">The <see cref="int"/></param>
		/// <returns>The <see cref="int"/></returns>
		private static int Distance(int[] source, int[] target, int threshold) {

			var length1 = source.Length;
			var length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2) {
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			var maxi = length1;
			var maxj = length2;

			var dCurrent = new int[maxi + 1];
			var dMinus1 = new int[maxi + 1];
			var dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (var i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (var j = 1; j <= maxj; j++) {

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				var minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (var i = 1; i <= maxi; i++) {

					var cost = source[im1] == target[jm1] ? 0 : 1;

					var del = dCurrent[im1] + 1;
					var ins = dMinus1[i] + 1;
					var sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					var min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
						min = Math.Min(min, dMinus2[im2] + cost);

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			var result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}

		/// <summary>
		/// The Swap
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arg1">The <see cref="T"/></param>
		/// <param name="arg2">The <see cref="T"/></param>
		internal static void Swap<T>(ref T arg1, ref T arg2) {
			var temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}
	}
}
