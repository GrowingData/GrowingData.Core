// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System.Diagnostics;

	/// <summary>
	/// Defines the <see cref="SimpleStopwatch" />
	/// </summary>
	public class SimpleStopwatch {

		public static Stopwatch Start() {
			Stopwatch watch = new Stopwatch();
			watch.Start();
			return watch;
		}
	}
}
