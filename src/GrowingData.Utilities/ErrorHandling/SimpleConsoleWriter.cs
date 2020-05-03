using System;
using System.Drawing;
using System.Linq;

namespace GrowingData.Utilities {
	public enum ConsoleWriterMode {
		Normal,
		Test
	}

	public class SimpleConsoleWriter {
		// Lock the writes so they don't get ugly
		public static object _locker = new object();

		public static SimpleConsoleWriter Instance = new SimpleConsoleWriter();

		private static ConsoleWriterMode _mode = ConsoleWriterMode.Normal;
		private static ConsoleColor _background = ConsoleColor.Black;
		public SimpleConsoleWriter() {
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			_background = Console.BackgroundColor;

		}

		public static void SetMode(ConsoleWriterMode mode) {
			_mode = mode;
		}

		public void Error(string message) {
			lock (_locker) {
				WriteStatusError();
				WriteError(new Exception(message), true);
				WriteEOL();
			}
		}


		public void Error(string message, Exception ex) {
			lock (_locker) {
				WriteStatusError();
				WriteMessage(message + ": ");
				WriteError(ex, true);
				WriteEOL();

			}
		}

		public void Information(string message) {
			if (_mode == ConsoleWriterMode.Test) {
				return;
			}

			lock (_locker) {
				WriteStatusInformation();
				WriteMessage(message);
				WriteEOL();
			}

		}
		public void Warn(string message) {
			lock (_locker) {
				WriteStatusWarning();
				WriteMessage(message);
				WriteEOL();
			}

		}
		public void Success(string message) {
			lock (_locker) {
				WriteStatusSuccess();
				WriteMessage(message);
				WriteEOL();
			}
		}

		public void TestPass(string message) {
			lock (_locker) {
				WriteStatusTestPass();
				WriteMessage(message);
				WriteEOL();
			}
		}


		public void TestFail(string message) {
			lock (_locker) {
				WriteStatusTestFail();
				WriteMessage(message);
				WriteEOL();
			}
		}

		private void WriteStatus(string message, ConsoleColor textColor, ConsoleColor bgColor) {
			var lastForeground = Console.ForegroundColor;
			var lastBackground = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = _background;
			Console.Write("[");

			Console.BackgroundColor = bgColor;
			Console.ForegroundColor = textColor;
			Console.Write(message);

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = _background;
			Console.Write("] ");

			Console.ForegroundColor = lastForeground;
			Console.BackgroundColor = lastBackground;

		}

		public void WriteMessage(string message, ConsoleColor textColor) {
			var lastForeground = Console.ForegroundColor;
			var lastBackground = Console.BackgroundColor;

			Console.ForegroundColor = textColor;
			Console.Write(message);

			Console.ForegroundColor = lastForeground;
			Console.BackgroundColor = lastBackground;
		}

		public void Write(string message, ConsoleColor color) {
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = oldColor;
		}

		public void WriteStatusError() {
			WriteStatus(" ERR ", ConsoleColor.White, ConsoleColor.DarkRed);

		}

		public void WriteStatusSuccess() {
			WriteStatus(" OK  ", ConsoleColor.White, ConsoleColor.DarkCyan);
			//Console.WriteFormatted(" [ OK  ] ", Color.Green);
			//FluentConsole.Instance
			//	.Black.Background.White.Text(" [")
			//	.DarkGreen.Background.White.Text(" OK  ")
			//	.Black.Background.White.Text("] "); ;
		}

		public void WriteStatusInformation() {
			WriteStatus(" INF ", ConsoleColor.White, ConsoleColor.Black);
			//Console.WriteFormatted(" [ INF ] ", Color.Gray);
			//FluentConsole.Instance
			//	.Black.Background.White.Text(" [")
			//	.DarkGray.Background.White.Text(" INF ")
			//	.Black.Background.White.Text("] "); ;
		}

		public void WriteStatusWarning() {
			WriteStatus(" WRN ", ConsoleColor.White, ConsoleColor.DarkMagenta);
			//Console.WriteFormatted(" [ WRN ] ", Color.Yellow);
			//FluentConsole.Instance
			//	.Black.Background.White.Text(" [")
			//	.DarkYellow.Background.White.Text(" WRN ")
			//	.Black.Background.White.Text("] ");
		}


		public void WriteStatusTestPass() {
			WriteStatus(" PASS ", ConsoleColor.White, ConsoleColor.Green);
			//Console.WriteFormatted(" [ PASS ] ", Color.Green);
			//FluentConsole.Instance
			//	.Black.Background.White.Text(" [")
			//	.Green.Background.White.Text(" PASS ")
			//	.Black.Background.White.Text("] "); ;
		}

		public void WriteStatusTestFail() {
			WriteStatus(" FAIL ", ConsoleColor.White, ConsoleColor.Red);
			//Console.WriteFormatted(" [ FAIL ] ", Color.Red);
			//FluentConsole.Instance
			//	.Black.Background.White.Text(" [")
			//	.Red.Background.White.Text(" FAIL ")
			//	.Black.Background.White.Text("] "); ;
		}



		public void WriteMessage(string message) {
			WriteMessage(message, ConsoleColor.Gray);
			//FluentConsole.Instance.Black.Background.White.Text(message);
		}

		public void WriteError(Exception ex, bool allowMultiline) {
			var message = "";
			if (allowMultiline) {
				message = ex.Message.Replace("\n", "\n\t");
			} else {
				message = ex.Message.Split('\n').FirstOrDefault().Trim();
			}

			WriteMessage(message, ConsoleColor.Red);
			//FluentConsole.Instance.Black.Background.Red.Text(message);
		}

		public void WriteEOL() {
			Console.WriteLine("");
			//FluentConsole.Instance
			//	.Black.Line("");
		}


	}
}
