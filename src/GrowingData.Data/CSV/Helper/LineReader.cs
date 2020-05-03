// Adapted from: https://bitbucket.org/nxt/csv-toolkit

namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Defines the <see cref="LineReader" />
	/// </summary>
	public class LineReader : IDisposable {

		/// <summary>
		/// Defines the State
		/// </summary>
		private enum State {
			/// <summary>
			/// Defines the ProcessingText1
			/// </summary>
			ProcessingText1,
			/// <summary>
			/// Defines the AfterProcessingText
			/// </summary>
			AfterProcessingText,
			/// <summary>
			/// Defines the InQuotedString
			/// </summary>
			InQuotedString,
			/// <summary>
			/// Defines the IgnoreWhitespace
			/// </summary>
			IgnoreWhitespace
		}

		/// <summary>
		/// Defines the _reader
		/// </summary>
		private readonly TextReader _reader;

		/// <summary>
		/// Defines the _dispose
		/// </summary>
		private readonly bool _dispose;

		/// <summary>
		/// Defines the _opts
		/// </summary>
		private readonly CsvFileOptions _opts;

		/// <summary>
		/// Defines the _lineNumber
		/// </summary>
		private int _lineNumber = 0;

		/// <summary>
		/// Defines the _columnNumber
		/// </summary>
		private int _columnNumber = 0;

		/// <summary>
		/// Defines the _isReading
		/// </summary>
		private bool _isReading = false;

		/// <summary>
		/// Defines the _isEOF
		/// </summary>
		private bool _isEOF = false;

		/// <summary>
		/// Defines the _newLineCount
		/// </summary>
		private long _newLineCount = 0;

		/// <summary>
		/// Gets the LineNumber
		/// </summary>
		public int LineNumber => _lineNumber;

		/// <summary>
		/// Gets a value indicating whether IsReading
		/// </summary>
		public bool IsReading => _isReading;

		/// <summary>
		/// Gets a value indicating whether IsEOF
		/// </summary>
		public bool IsEOF => _isEOF;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input">The stream to read</param>
		/// <param name="dispose"></param>
		public LineReader(CsvFileOptions options, TextReader reader, bool dispose = false) {
			_opts = options;
			this._reader = reader;
			this._dispose = dispose;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LineReader"/> class.
		/// </summary>
		/// <param name="options">The <see cref="CsvFileOptions"/></param>
		/// <param name="input">The <see cref="Stream"/></param>
		/// <param name="encoding">The <see cref="Encoding"/></param>
		/// <param name="dispose">The <see cref="bool"/></param>
		public LineReader(CsvFileOptions options, Stream input, Encoding encoding, bool dispose = false) {
			_opts = options;
			this._dispose = dispose;
			this._reader = new StreamReader(input, encoding);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LineReader"/> class.
		/// </summary>
		/// <param name="options">The <see cref="CsvFileOptions"/></param>
		/// <param name="file">The <see cref="FileInfo"/></param>
		/// <param name="encoding">The <see cref="Encoding"/></param>
		public LineReader(CsvFileOptions options, FileInfo file, Encoding encoding) {
			_opts = options;
			var input = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
			_dispose = true;
			_reader = new StreamReader(input, encoding);
		}

		/// <summary>
		/// The Dispose
		/// </summary>
		public void Dispose() {
			if (_dispose) {
				_reader.Dispose();
			}
		}

		/// <summary>
		/// The GetLines
		/// </summary>
		/// <returns>The <see cref="IEnumerable{string[]}"/></returns>
		public IEnumerable<string[]> GetLines() {
			_isReading = true;
			var sb = new StringBuilder();
			while (PeekChar() != -1) {
				_lineNumber++;
				_columnNumber = 0;
				var res = ConsumeLine(sb).ToArray();
				if (res.Length > 0) {
					yield return res;
				}
			}
			_isReading = false;
			_isEOF = true;
		}

		/// <summary>
		/// The ReadChar
		/// </summary>
		/// <returns>The <see cref="int"/></returns>
		private int ReadChar() {
			_columnNumber++;
			var c = _reader.Read();
			if (c == '\n') {
				_newLineCount++;
			}
			return c;
		}

		/// <summary>
		/// The PeekChar
		/// </summary>
		/// <returns>The <see cref="int"/></returns>
		private int PeekChar() {
			return _reader.Peek();
		}

		/// <summary>
		/// The ConsumeLine
		/// </summary>
		/// <param name="sb">The <see cref="StringBuilder"/></param>
		/// <returns>The <see cref="IEnumerable{string}"/></returns>
		private IEnumerable<string> ConsumeLine(StringBuilder sb) {

			var trailingWhitespace = new StringBuilder();
			var quoteChar = _opts.QuoteChar;
			var fieldSeparatorChar = _opts.FieldTerminator;
			var eolStyle = _opts.EndOfLineStyle;
			var allowQuotedFields = !_opts.DisableQuotedFields;
			var removeWhiteSpaceAroundSeparators = !_opts.KeepWhiteSpaceAroundSeparators;
			sb.Length = 0;
			var running = true;
			var initialState = removeWhiteSpaceAroundSeparators ? State.IgnoreWhitespace : State.ProcessingText1;
			var state = initialState;
			var lastChar = 'x';
			do {
				//read header
				var cc = ReadChar();
				if (cc == -1) {
					running = false;
					if (sb.Length > 0) {
						yield return sb.ToString();
					}

					sb.Length = 0;
					yield break;
				}
				var c = (char)cc;

				if (state == State.IgnoreWhitespace && Char.IsWhiteSpace(c) && c != '\r' && c != '\n' && c != fieldSeparatorChar) {
					continue;
				}

				if (allowQuotedFields && c == quoteChar && state != State.AfterProcessingText) {
					if (state == State.InQuotedString) {
						// If we are in a quoted string, then end the quoted string if we get the 
						// quote character. Unless its prefixed with a "\" or quotechar to escape it (provided
						// that the field doesn't look like '""',)
						//if (PeekChar() == (int)quoteChar || lastChar=='\\') {
						//	ReadChar();

						if (lastChar == '\\' && !_opts.ExcelQuoted) {
							sb.Append(c);
						} else {
							// Also allow double quote char ("") to escape a quote char, as long as its not the first 
							// character
							if (_opts.ExcelQuoted) {
								// Except where a field is  """Something that should be quoted"".
								if (PeekChar() == quoteChar) {
									// Is it a double quote, or is it the end of the field?

									// The quote
									ReadChar();

									if (PeekChar() == fieldSeparatorChar && sb.Length == 1) {
										state = State.AfterProcessingText;
									} else {
										//sb.Append("\\" + c);
										sb.Append(c);
										continue;
									}
								}
							}

							// Keep the '"' quote characters because I want them so I can use the JSON serializer
							sb.Append(quoteChar);
							state = State.AfterProcessingText;
						}
					} else {
						//switch mode
						// Keep the '"' quote characters because I want them so I can use the JSON serializer
						sb.Append(quoteChar);
						state = State.InQuotedString;
					}
				} else if (state == State.InQuotedString) {
					if (_opts.ExcelQuoted && c == '\\') {
						sb.Append('\\');
					}
					sb.Append(c);
				} else if (c == fieldSeparatorChar) {
					var res = sb.ToString();
					sb.Length = 0;
					state = initialState;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.Mixed && (c == '\r' || c == '\n')) {
					if (c == '\r' && PeekChar() == '\n') {
						c = (char)ReadChar();
					}
					//end
					var res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.CrLf && c == '\r' && PeekChar() == '\n') {
					c = (char)ReadChar();
					//end
					var res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (eolStyle == EndOfLineStyle.Lf && c == '\n') {
					var res = sb.ToString();
					sb.Length = 0;
					running = false;
					yield return res;
				} else if (state == State.AfterProcessingText) {
					if (Char.IsWhiteSpace(c) || _opts.InvalidTextAction == InvalidTextMode.Ignore) {
						continue;
					}

					var msg = string.Format("Unexpected character: '{0}' on line {1}, column: {2}. Expected: '{3}'", c, _lineNumber, _columnNumber, fieldSeparatorChar);
					// read untill end of line to keep reset the state
					var tmp = ReadChar();
					while (tmp != -1 && tmp != '\n') {
						tmp = ReadChar();
					}

					throw new InvalidDataException(msg);
				} else {
					if (removeWhiteSpaceAroundSeparators && Char.IsWhiteSpace(c)) {
						trailingWhitespace.Append(c);
					} else {
						if (state == State.IgnoreWhitespace) {
							state = State.ProcessingText1;
						}

						if (trailingWhitespace.Length > 0) {
							sb.Append(trailingWhitespace.ToString());
							trailingWhitespace.Length = 0;
						}
						sb.Append(c);
					}
				}
				lastChar = c;
			} while (running);
		}
	}
}
