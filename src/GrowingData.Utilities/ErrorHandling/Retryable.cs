
namespace GrowingData.Utilities {
	using System;
	using System.Linq;
	using System.Threading;
	using Serilog;

	/// <summary>
	/// Defines the BackOffStrategy
	/// </summary>
	public enum BackOffStrategy {
		/// <summary>
		/// Defines the Fixed
		/// </summary>
		Fixed = 1,

		/// <summary>
		/// Defines the Linear
		/// </summary>
		Linear = 2,

		/// <summary>
		/// Defines the DoubleLinear
		/// </summary>
		DoubleLinear = 3,

		/// <summary>
		/// Defines the Exponential
		/// </summary>
		Exponential = 4
	}

	/// <summary>
	/// Defines the <see cref="Retryable" />
	/// </summary>
	public static class Retryable {
		/// <summary>
		/// Defines the DEFAULT_RETRIES
		/// </summary>
		public static int DEFAULT_RETRIES = 5;

		/// <summary>
		/// Defines the DEFAULT_BACKOFF
		/// </summary>
		public static BackOffStrategy DEFAULT_BACKOFF = BackOffStrategy.DoubleLinear;

		/// <summary>
		/// Defines the DEFAULT_RETRY_DELAY
		/// </summary>
		public static TimeSpan DEFAULT_RETRY_DELAY = TimeSpan.FromSeconds(1);

		/// <summary>
		/// Defines the Randomizer
		/// </summary>
		private static Random Randomizer = new Random();

		private static string ShortStackTrace(Exception ex) {
			var stackLines = ex.StackTrace.Split('\n');
			var filesAndLineNumbers = stackLines
				.Select(line => line.Split('\\').LastOrDefault().Trim())
				.Where(line => !line.StartsWith("at System."))
				.ToList();

			var joined = string.Join(" > ", filesAndLineNumbers);
			return joined;
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="strategy">The <see cref="BackOffStrategy"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(string exceptionMessageFilter, int maxTries, TimeSpan initialDelay, BackOffStrategy strategy, Func<T> toRetry) {
			var currentTry = 0;
			while (true) {
				try {
					return toRetry();

				} catch (Exception ex) {
					// Syntax errors are not going to get fixed by re-trying
					if (ex.Message.Contains(" syntax ")) {
						throw;
					}

					Log.Warning($"Retryable.Try failed (try: {currentTry}): {(ex.Message.Lines().FirstOrDefault())} ({ShortStackTrace(ex)}");
					currentTry++;
					if (currentTry >= maxTries) {
						throw;
					}

					if (exceptionMessageFilter != null && !ex.Message.Contains(exceptionMessageFilter)) {
						throw;
					}

					var sleepTime = initialDelay.TotalMilliseconds;

					switch (strategy) {
						case BackOffStrategy.Fixed:
							sleepTime = initialDelay.TotalMilliseconds;
							break;
						case BackOffStrategy.Linear:
							sleepTime = sleepTime * currentTry;
							break;
						case BackOffStrategy.DoubleLinear:
							sleepTime = sleepTime * currentTry * 2;
							break;
						case BackOffStrategy.Exponential:
							sleepTime = Math.Pow(sleepTime, currentTry);
							break;

					}
					var jitter = (Randomizer.NextDouble() - 0.5d) * (sleepTime * 0.10);

					Thread.Sleep((int)(sleepTime + jitter));
				}
			}
			throw new Exception("Impossible - exited the while loop without exception or success");
		}

		public static TimeSpan Backoff(BackOffStrategy strategy, TimeSpan initialDelay, int currentTry, double jitterPercent) {

			var sleepTime = initialDelay.TotalSeconds;
			switch (strategy) {
				case BackOffStrategy.Fixed:
					sleepTime = initialDelay.TotalMilliseconds;
					break;
				case BackOffStrategy.Linear:
					sleepTime = sleepTime * currentTry;
					break;
				case BackOffStrategy.DoubleLinear:
					sleepTime = sleepTime * currentTry * 2;
					break;
				case BackOffStrategy.Exponential:
					sleepTime = Math.Pow(sleepTime, currentTry);
					break;

			}
			var jitter = (Randomizer.NextDouble() - 0.5d) * (sleepTime * jitterPercent);
			return TimeSpan.FromSeconds(sleepTime + jitter);
		}


		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(Func<T> toRetry) {
			return Try(null, DEFAULT_RETRIES, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(int maxTries, Func<T> toRetry) {
			return Try(null, maxTries, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(int maxTries, TimeSpan initialDelay, Func<T> toRetry) {
			return Try(null, maxTries, initialDelay, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(string exceptionMessageFilter, Func<T> toRetry) {
			return Try(exceptionMessageFilter, DEFAULT_RETRIES, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(string exceptionMessageFilter, int maxTries, Func<T> toRetry) {
			return Try(exceptionMessageFilter, maxTries, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="toRetry">The <see cref="Func{T}"/></param>
		/// <returns>The <see cref="T"/></returns>
		public static T Try<T>(string exceptionMessageFilter, int maxTries, TimeSpan initialDelay, Func<T> toRetry) {
			return Try(exceptionMessageFilter, maxTries, initialDelay, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="strategy">The <see cref="BackOffStrategy"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(string exceptionMessageFilter, int maxTries, TimeSpan initialDelay, BackOffStrategy strategy, Action toRetry) {
			Try(exceptionMessageFilter, maxTries, initialDelay, strategy, () => { toRetry(); return true; });
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(Action toRetry) {
			Try(null, DEFAULT_RETRIES, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(string exceptionMessageFilter, Action toRetry) {
			Try(exceptionMessageFilter, DEFAULT_RETRIES, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(int maxTries, Action toRetry) {
			Try(null, maxTries, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(int maxTries, TimeSpan initialDelay, Action toRetry) {
			Try(null, maxTries, initialDelay, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try<T>(string exceptionMessageFilter, Action toRetry) {
			Try(exceptionMessageFilter, DEFAULT_RETRIES, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try<T>(string exceptionMessageFilter, int maxTries, Action toRetry) {
			Try(exceptionMessageFilter, maxTries, DEFAULT_RETRY_DELAY, DEFAULT_BACKOFF, toRetry);
		}

		/// <summary>
		/// The Try
		/// </summary>
		/// <param name="exceptionMessageFilter">The <see cref="string"/></param>
		/// <param name="maxTries">The <see cref="int"/></param>
		/// <param name="initialDelay">The <see cref="TimeSpan"/></param>
		/// <param name="toRetry">The <see cref="Action"/></param>
		public static void Try(string exceptionMessageFilter, int maxTries, TimeSpan initialDelay, Action toRetry) {
			Try(exceptionMessageFilter, maxTries, initialDelay, DEFAULT_BACKOFF, toRetry);
		}
	}
}
