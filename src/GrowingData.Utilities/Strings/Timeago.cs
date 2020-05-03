using System;

namespace GrowingData.Utilities {
	public static class Timeago {
		private const int SECOND = 1;
		private const int MINUTE = 60 * SECOND;
		private const int HOUR = 60 * MINUTE;
		private const int DAY = 24 * HOUR;
		private const int MONTH = 30 * DAY;

		public static string RelativeTime(this DateTime? startUtc) {
			if (startUtc.HasValue) {
				return RelativeTime(DateTime.UtcNow.Subtract(startUtc.Value));
			} else {
				return " - ";
			}
		}
		public static string RelativeTime(this DateTime startUtc) {
			return RelativeTime(DateTime.UtcNow.Subtract(startUtc));
		}

		public static string RelativeTime(this TimeSpan ts) {
			var delta = Math.Abs(ts.TotalSeconds);

			if (delta < 1 * MINUTE) {
				return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
			}

			if (delta < 2 * MINUTE) {
				return "a minute ago";
			}

			if (delta < 45 * MINUTE) {
				return ts.Minutes + " minutes ago";
			}

			if (delta < 90 * MINUTE) {
				return "an hour ago";
			}

			if (delta < 24 * HOUR) {
				return ts.Hours + " hours ago";
			}

			if (delta < 48 * HOUR) {
				return "yesterday";
			}

			if (delta < 30 * DAY) {
				return ts.Days + " days ago";
			}

			if (delta < 12 * MONTH) {
				var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return months <= 1 ? "one month ago" : months + " months ago";
			} else {
				var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
				return years <= 1 ? "one year ago" : years + " years ago";
			}
		}


		public static string RoundedTime(this DateTime? startUtc) {
			if (startUtc.HasValue) {
				return RoundedTime(DateTime.UtcNow.Subtract(startUtc.Value));
			} else {
				return " - ";
			}
		}
		public static string RoundedTime(this DateTime startUtc) {
			return RoundedTime(DateTime.UtcNow.Subtract(startUtc));
		}

		public static string RoundedTime(this TimeSpan ts) {
			var delta = Math.Abs(ts.TotalSeconds);

			if (delta < 2 * MINUTE) {
				return ts.Seconds + "s";
			}

			if (delta < 90 * MINUTE) {
				return ts.Minutes + "m";
			}

			if (delta < 24 * HOUR) {
				return ts.Hours + "h";
			}


			return ts.Days + "d";

		}
	}
}
