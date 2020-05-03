// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Threading;

	//static class TaskExtensions {
	//	public static void Forget(this Task task) {
	//	}
	//}

	//public static class BackgroundWorker {
	//	public static CancellationTokenSource Start(TimeSpan interval, Action action) {
	//		var token = new CancellationTokenSource();

	//		WorkAsync(action, new TimeSpan(0, 0, 0), interval, token.Token).Forget();

	//		return token;
	//	}

	//	public static CancellationTokenSource Start(TimeSpan dueTime, TimeSpan interval, Action action) {
	//		var token = new CancellationTokenSource();

	//		WorkAsync(action, dueTime, interval, token.Token).Forget();

	//		return token;
	//	}

	//	public static async Task WorkAsync(Action action, TimeSpan dueTime, TimeSpan interval, CancellationToken token) {
	//		// Initial wait time before we begin the periodic loop.
	//		if (dueTime > TimeSpan.Zero)
	//			await Task.Delay(dueTime, token);

	//		// Repeat this loop until cancelled.
	//		while (!token.IsCancellationRequested) {
	//			action();

	//			// Wait to repeat again.
	//			if (interval > TimeSpan.Zero)
	//				await Task.Delay(interval, token);
	//		}
	//	}

	//}
	/// <summary>
	/// Defines the <see cref="BackgroundWorker" />
	/// </summary>
	public class BackgroundWorker {
		/// <summary>
		/// Defines the _action
		/// </summary>
		private Action _action;

		/// <summary>
		/// Defines the _interval
		/// </summary>
		private TimeSpan _interval;

		/// <summary>
		/// Defines the _name
		/// </summary>
		private string _name;

		/// <summary>
		/// Defines the _cancelled
		/// </summary>
		private bool _cancelled;

		/// <summary>
		/// Defines the _thread
		/// </summary>
		private Thread _thread;

		/// <summary>
		/// Initializes a new instance of the <see cref="BackgroundWorker"/> class.
		/// </summary>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="interval">The <see cref="TimeSpan"/></param>
		/// <param name="action">The <see cref="Action"/></param>
		public BackgroundWorker(string name, TimeSpan interval, Action action) {
			_name = name;
			_action = action;
			_interval = interval;
		}

		/// <summary>
		/// The Start
		/// </summary>
		public void Start() {
			if (_thread != null) {
				throw new Exception("Unable to re-start a task again because I can't be bothered handling thread cancellation properly");

			}
			_thread = new Thread(RunForever);
			_thread.Start();
		}

		/// <summary>
		/// The Cancel
		/// </summary>
		public void Cancel() {
			_cancelled = true;
		}

		/// <summary>
		/// The RunForever
		/// </summary>
		private void RunForever() {
			while (_cancelled != true) {
				RunAction();
				if (_cancelled) {
					break;
				}
				Thread.Sleep((int)_interval.TotalMilliseconds);
			}
		}

		/// <summary>
		/// The RunAction
		/// </summary>
		private void RunAction() {
			try {
				_action();
			} catch (Exception ex) {
				var errorMessage = $@"
{DateTime.UtcNow.ToString()}: Background worker {_name} exception
	{ex.Message}
	{ex.StackTrace}
";

				System.Diagnostics.Debug.WriteLine(errorMessage);

				//try {
				//	   var basePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin", "").Replace("\\Debug", "");
				//	var logPath = Path.Combine(basePath, "log", $"BackgroundWorker-{_name}.log");
				//	File.WriteAllText(logPath, errorMessage);
				//} catch {

				//	System.Diagnostics.Debug.WriteLine("Exception logging exception");
				//}

			}
		}
	}
}
