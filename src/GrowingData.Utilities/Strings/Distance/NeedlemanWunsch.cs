// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="NeedlemanWunschAlignment" />
	/// </summary>
	public class NeedlemanWunschAlignment {
		/// <summary>
		/// Gets or sets the Score
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		/// Gets or sets the Path
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the One
		/// </summary>
		public string One { get; set; }

		/// <summary>
		/// Gets or sets the Two
		/// </summary>
		public string Two { get; set; }

		/// <summary>
		/// Gets or sets the A
		/// </summary>
		public string A { get; set; }

		/// <summary>
		/// Gets or sets the B
		/// </summary>
		public string B { get; set; }

		/// <summary>
		/// The ToString
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		public new string ToString() {
			var s = string.Format("Score: {0}\r\nA: {1}\r\nB: {2}", Score, One, Two);
			return s;
		}
	}

	/// <summary>
	/// Defines the <see cref="NeedlemanWunsch" />
	/// </summary>
	public static class NeedlemanWunsch {
		// A rad algorithm for calculating alignments, which 
		// enables the comparison of similarity of strings by looking
		// at the distance between them in terms of edits.
		// A rad algorithm for calculating alignments, which 
		// enables the comparison of similarity of strings by looking
		// at the distance between them in terms of edits.        /// <summary>
		/// Defines the DONE
		/// </summary>
		internal const string DONE = @"¤";

		/// <summary>
		/// Defines the DIAG
		/// </summary>
		internal const string DIAG = @"\";

		/// <summary>
		/// Defines the UP
		/// </summary>
		internal const string UP = @"|";

		/// <summary>
		/// Defines the LEFT
		/// </summary>
		internal const string LEFT = @"-";

		/// <summary>
		/// The DistanceNeedlemanWunsch
		/// </summary>
		/// <param name="me">The <see cref="string"/></param>
		/// <param name="other">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int DistanceNeedlemanWunsch(this string me, string other) {
			return DistanceNeedlemanWunschAlignment(me, other).Score;
		}

		/// <summary>
		/// The DistanceNeedlemanWunschAlignment
		/// </summary>
		/// <param name="xs">The <see cref="string"/></param>
		/// <param name="ys">The <see cref="string"/></param>
		/// <returns>The <see cref="NeedlemanWunschAlignment"/></returns>
		public static NeedlemanWunschAlignment DistanceNeedlemanWunschAlignment(this string xs, string ys) {

			var GAP_PENALTY = -1;
			var MISMATCH_PENALTY = -2;
			var MATCH = 0;
			const string GAP = @"-";
			//int m = -2;

			var xLength = xs.Length;
			var yLength = ys.Length;

			var dpTable = new int[xLength + 1, yLength + 1];        // dynamic programming buttom up memory table
			var traceTable = new string[xLength + 1, yLength + 1];  // trace back path

			// Initialize arrays
			for (var i = 0; i < xLength + 1; i++) {
				dpTable[i, 0] = i * GAP_PENALTY;
			}

			for (var j = 0; j < yLength + 1; j++) {
				dpTable[0, j] = j * GAP_PENALTY;
			}
			traceTable[0, 0] = DONE;

			for (var i = 1; i < xLength + 1; i++) {
				traceTable[i, 0] = UP;
			}

			for (var j = 1; j < yLength + 1; j++) {
				traceTable[0, j] = LEFT;
			}
			// calc
			for (var i = 1; i < xLength + 1; i++) {
				for (var j = 1; j < yLength + 1; j++) {
					var alpha = MISMATCH_PENALTY;
					if (xs[i - 1] == ys[j - 1]) {
						alpha = MATCH;
					}

					//var alpha = Alpha(xs.ElementAt(i - 1).ToString(), ys.ElementAt(j - 1).ToString());
					var diag = alpha + dpTable[i - 1, j - 1];
					var up = GAP_PENALTY + dpTable[i - 1, j];
					var left = GAP_PENALTY + dpTable[i, j - 1];
					var max = Max(diag, up, left);
					dpTable[i, j] = max;

					if (max == diag)
						traceTable[i, j] = DIAG;
					else if (max == up)
						traceTable[i, j] = UP;
					else
						traceTable[i, j] = LEFT;
				}
			}

			var traceBack = ParseTraceBack(traceTable, xLength + 1, yLength + 1);

			var sb = new StringBuilder();
			string first, second;

			if (xs.Length != ys.Length) {
				string s;
				if (xs.Length > ys.Length) {
					s = ys;
					first = xs;
				} else {
					s = xs;
					first = ys;
				}


				var i = 0;
				foreach (var trace in traceBack) {
					if (trace.ToString() == DIAG)
						sb.Append(s.ElementAt(i++).ToString());
					else
						sb.Append(GAP);
				}

				second = sb.ToString();
			} else {
				first = xs;
				second = ys;
			}
			var sequence = new NeedlemanWunschAlignment() { Score = dpTable[xLength, yLength], Path = traceBack, One = first, Two = second, A = xs, B = ys };
			return sequence;
		}

		/// <summary>
		/// The ParseTraceBack
		/// </summary>
		/// <param name="T">The <see cref="string[,]"/></param>
		/// <param name="I">The <see cref="int"/></param>
		/// <param name="J">The <see cref="int"/></param>
		/// <returns>The <see cref="string"/></returns>
		internal static string ParseTraceBack(string[,] T, int I, int J) {
			var sb = new StringBuilder();
			var i = I - 1;
			var j = J - 1;
			var path = T[i, j];
			while (path != DONE) {
				sb.Append(path);
				if (path == DIAG) {
					i--;
					j--;
				} else if (path == UP)
					i--;
				else if (path == LEFT)
					j--;

				path = T[i, j];
			}
			//return sb.ToString().Reverse().ToString();
			return ReverseString(sb.ToString());
		}

		/// <summary>
		/// The ReverseString
		/// </summary>
		/// <param name="s">The <see cref="string"/></param>
		/// <returns>The <see cref="string"/></returns>
		internal static string ReverseString(string s) {
			var arr = s.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}

		/// <summary>
		/// The Max
		/// </summary>
		/// <param name="a">The <see cref="int"/></param>
		/// <param name="b">The <see cref="int"/></param>
		/// <param name="c">The <see cref="int"/></param>
		/// <returns>The <see cref="int"/></returns>
		internal static int Max(int a, int b, int c) {
			if (a >= b && a >= c)
				return a;
			if (b >= a && b >= c)
				return b;
			return c;
		}
	}
}
