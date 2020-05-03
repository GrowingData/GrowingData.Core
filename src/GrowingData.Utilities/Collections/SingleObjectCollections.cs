// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="SingleElementEnumerable" />
	/// </summary>
	public static class SingleElementEnumerable {
		/// <summary>
		/// The ToSingleElementEnumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">The <see cref="T"/></param>
		/// <returns>The <see cref="IEnumerable{T}"/></returns>
		public static IEnumerable<T> ToSingleElementEnumerable<T>(this T o) {
			yield return o;
		}

		/// <summary>
		/// The ToSingleElementList
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">The <see cref="T"/></param>
		/// <returns>The <see cref="List{T}"/></returns>
		public static List<T> ToSingleElementList<T>(this T o) {
			return ToSingleElementEnumerable(o).ToList();
		}

		/// <summary>
		/// The ToSingleElementArray
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">The <see cref="T"/></param>
		/// <returns>The <see cref="T[]"/></returns>
		public static T[] ToSingleElementArray<T>(this T o) {
			return ToSingleElementEnumerable(o).ToArray();
		}
	}
}
