// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Defines the <see cref="HashsetHelpers" />
	/// </summary>
	public static class HashsetHelpers {
		/// <summary>
		/// The ToHashSet
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="source">The <see cref="IEnumerable{TSource}"/></param>
		/// <param name="selector">The <see cref="Func{TSource, TResult}"/></param>
		/// <returns>The <see cref="HashSet{TResult}"/></returns>
		public static HashSet<TResult> ToHashSet<TSource, TResult>(
			this IEnumerable<TSource> source,
			Func<TSource, TResult> selector

		) {

			return new HashSet<TResult>(source.Select(x => selector(x)));
		}
	}
}
