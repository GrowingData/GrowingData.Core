// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Diagnostics;

	/// <summary>
	/// Defines the <see cref="SimpleTimer" />
	/// </summary>
	public class SimpleTimer : IDisposable {
		/// <summary>
		/// Defines the _watch
		/// </summary>
		private Stopwatch _watch;

		/// <summary>
		/// Defines the _callback
		/// </summary>
		private Action<double> _callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleTimer"/> class.
		/// </summary>
		/// <param name="callback">The <see cref="Action{double}"/></param>
		public SimpleTimer(Action<double> callback) {
			_watch = new Stopwatch();
			_callback = callback;
			_watch.Start();
		}

		/// <summary>
		/// The Dispose
		/// </summary>
		public void Dispose() {
			_watch.Stop();
			_callback(_watch.ElapsedMilliseconds);
		}

		/// <summary>
		/// The Time
		/// </summary>
		/// <param name="run">The <see cref="Action"/></param>
		/// <param name="callback">The <see cref="Action{double}"/></param>
		public static void Time(Action run, Action<double> callback) {
			using (var t = new SimpleTimer(callback)) {
				run();
			}
		}

		/// <summary>
		/// The Time
		/// </summary>
		/// <param name="run">The <see cref="Action"/></param>
		/// <param name="callback">The <see cref="Action{double}"/></param>
		public static void Time(Action run, Action<TimeSpan> callback) {
			var watch = SimpleStopwatch.Start();
			run();
			callback(watch.Elapsed);
		}

		/// <summary>
		/// The DebugTime
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="run">The <see cref="Action"/></param>
		public static void DebugTime(string name, Action run) {
			using (var t = new SimpleTimer((ms) => Debug.WriteLine("{0}\t {1}ms", name, ms))) {
				run();
			}
		}
	}
}
