// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines the <see cref="SmithWaterman" />
	/// </summary>
	public static class SmithWaterman {
		/// <summary>
		/// The DistanceSmithWaterman
		/// </summary>
		/// <param name="me">The <see cref="string"/></param>
		/// <param name="other">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		public static int DistanceSmithWaterman(this string me, string other) {
			var sw = new SmithWatermanAlignment(me, other);
			return sw.ComputeSmithWaterman();
		}

		/// <summary>
		/// The DistanceSmithWatermanAlignment
		/// </summary>
		/// <param name="me">The <see cref="string"/></param>
		/// <param name="other">The <see cref="string"/></param>
		/// <returns>The <see cref="SmithWatermanAlignment"/></returns>
		public static SmithWatermanAlignment DistanceSmithWatermanAlignment(this string me, string other) {
			var sw = new SmithWatermanAlignment(me, other);
			return sw;
		}
	}

	/// <summary>
	/// Defines the <see cref="SmithWatermanAlignment" />
	/// </summary>
	public class SmithWatermanAlignment {
		/// <summary>
		/// Defines the one, two
		/// </summary>
		private string one, two;

		/// <summary>
		/// Defines the matrix
		/// </summary>
		private int[,] matrix;

		/// <summary>
		/// Defines the gap
		/// </summary>
		private int gap;

		/// <summary>
		/// Defines the match
		/// </summary>
		private int match;

		/// <summary>
		/// Defines the o
		/// </summary>
		private int o;

		/// <summary>
		/// Defines the l
		/// </summary>
		private int l;

		/// <summary>
		/// Defines the e
		/// </summary>
		private int e;

		/// <summary>
		/// Initializes a new instance of the <see cref="SmithWatermanAlignment"/> class.
		/// </summary>
		/// <param name="one">The <see cref="string"/></param>
		/// <param name="two">The <see cref="string"/></param>
		public SmithWatermanAlignment(string one, string two) {
			this.one = one.ToLower();
			this.two = two.ToLower();

			// Define affine gap starting values
			this.match = 2;
			o = -2;
			l = 0;
			e = -1;

			// initialize matrix to 0
			matrix = new int[one.Length + 1, two.Length + 1];
			for (var i = 0; i < one.Length; i++)
				for (var j = 0; j < two.Length; j++)
					matrix[i, j] = 0;
		}

		// returns the alignment score
		/// <summary>
		/// The ComputeSmithWaterman
		/// </summary>
		/// <returns>The <see cref="int"/></returns>
		public int ComputeSmithWaterman() {
			for (var i = 0; i < one.Length; i++) {
				for (var j = 0; j < two.Length; j++) {
					gap = o + (l - 1) * e;
					if (i != 0 && j != 0) {
						if (one[i] == two[j]) {
							// match
							// reset l
							l = 0;
							matrix[i, j] = Math.Max(0, Math.Max(
											matrix[i - 1, j - 1] + match, Math.Max(
															matrix[i - 1, j] + gap,
															matrix[i, j - 1] + gap)));
						} else {
							// gap
							l++;
							matrix[i, j] = Math.Max(0, Math.Max(
											matrix[i - 1, j - 1] + gap, Math.Max(
															matrix[i - 1, j] + gap,
															matrix[i, j - 1] + gap)));

						}
					}
				}
			}

			// find the highest value
			var longest = 0;
			int iL = 0, jL = 0;
			for (var i = 0; i < one.Length; i++) {
				for (var j = 0; j < two.Length; j++) {
					if (matrix[i, j] > longest) {
						longest = matrix[i, j];
						iL = i;
						jL = j;
					}
				}
			}

			// Backtrack to reconstruct the path
			var ii = iL;
			var jj = jL;
			var actions = new Stack<String>();

			while (ii != 0 && jj != 0) {
				// diag case
				if (Math.Max(matrix[ii - 1, jj - 1],
								Math.Max(matrix[ii - 1, jj], matrix[ii, jj - 1])) == matrix[ii - 1, jj - 1]) {
					actions.Push("align");
					//Console.WriteLine("a");
					ii = ii - 1;
					jj = jj - 1;
					// left case
				} else if (Math.Max(matrix[ii - 1, jj - 1],
								Math.Max(matrix[ii - 1, jj], matrix[ii, jj - 1])) == matrix[ii, jj - 1]) {
					actions.Push("insert");
					//Console.WriteLine("i");
					jj = jj - 1;
					// up case
				} else {
					actions.Push("delete");
					//Console.WriteLine("d");
					ii = ii - 1;
				}
			}

			var alignOne = "";
			var alignTwo = "";
			var tmp = new string[actions.Count];
			actions.CopyTo(tmp, 0);

			var backActions = new Stack<string>(tmp);
			for (var z = 0; z < one.Length; z++) {
				alignOne = alignOne + one[z];
				if (actions.Count > 0) {
					var curAction = actions.Pop();
					// Console.WriteLine(curAction);
					if (curAction.Equals("insert")) {
						alignOne = alignOne + "-";
						while (actions.Peek().Equals("insert")) {
							alignOne = alignOne + "-";
							actions.Pop();
						}
					}
				}
			}

			for (var z = 0; z < two.Length; z++) {
				alignTwo = alignTwo + two[z];
				if (backActions.Count > 0) {
					var curAction = backActions.Pop();
					if (curAction.Equals("delete")) {
						alignTwo = alignTwo + "-";
						while (backActions.Peek().Equals("delete")) {
							alignTwo = alignTwo + "-";
							backActions.Pop();
						}
					}
				}
			}

			// print alignment
			//Console.WriteLine(alignOne + "\n" + alignTwo);
			return longest;
		}

		/// <summary>
		/// The printMatrix
		/// </summary>
		public void printMatrix() {
			for (var i = 0; i < one.Length; i++) {
				if (i == 0) {
					for (var z = 0; z < two.Length; z++) {
						if (z == 0)
							Console.Write("   ");
						Console.Write(two[z] + "  ");

						if (z == two.Length - 1)
							Console.WriteLine();
					}
				}

				for (var j = 0; j < two.Length; j++) {
					if (j == 0) {
						Console.Write(one[i] + "  ");
					}
					Console.Write(matrix[i, j] + "  ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}
	}
}
