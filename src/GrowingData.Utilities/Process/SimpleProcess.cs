// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Serilog;

	/// <summary>
	/// Defines the <see cref="SimpleProcess" />
	/// </summary>
	public class SimpleProcess : IDisposable {
		/// <summary>
		/// Defines the _command
		/// </summary>
		protected string _command;

		protected bool _waitForExit = true;

		public bool HasBeenKilled { get; private set; }

		/// <summary>
		/// Defines the _arguments
		/// </summary>
		protected string _arguments;

		/// <summary>
		/// Defines the _workingDirectory
		/// </summary>
		protected string _workingDirectory;

		/// <summary>
		/// Defines the _environmentVariables
		/// </summary>
		protected Dictionary<string, string> _environmentVariables;

		/// <summary>
		/// Defines the _errorOutput
		/// </summary>
		protected StringBuilder _standardError = new StringBuilder();

		/// <summary>
		/// Defines the _standardOutput
		/// </summary>
		protected StringBuilder _standardOutput = new StringBuilder();

		/// <summary>
		/// Defines the _process
		/// </summary>
		protected Process _process;

		/// <summary>
		/// Gets the ExitCode
		/// </summary>
		public int? ExitCode {
			get {
				if (_process != null && _process.HasExited) {
					return _process.ExitCode;
				}
				return new int?();
			}
		}

		public int ProcessId => _process == null ? -1 : _process.Id;

		//private Action<SimpleProcess> _deadConsole;

		// No process means it hasnt started, so technically hasn't exited
		public bool HasExited => _process == null ? false : _process.HasExited;

		public bool IsRunning => _process == null ? false : !_process.HasExited;

		//private bool _autoRestart = false;

		/// <summary>
		/// Gets the StandardOut
		/// </summary>
		public string StandardOut => _standardOutput.ToString();

		/// <summary>
		/// Gets the StandardError
		/// </summary>
		public string StandardError => _standardError.ToString();

		/// <summary>
		/// Defines the _customOutputFilter
		/// </summary>
		protected Func<string, bool> _customOutputFilter;

		public SimpleProcess OnExit(Action<SimpleProcess> action) {
			_onExit.Add(action);
			return this;
		}


		private List<Action<SimpleProcess, string>> _onStandardOut = new List<Action<SimpleProcess, string>>();
		private List<Action<SimpleProcess, string>> _onStandardError = new List<Action<SimpleProcess, string>>();
		private List<Action<SimpleProcess>> _onExit = new List<Action<SimpleProcess>>();



		public SimpleProcess OnWriteStandardOut(Action<SimpleProcess, string> callback) {
			_onStandardOut.Add(callback);
			return this;
		}

		public SimpleProcess OnWriteStandardError(Action<SimpleProcess, string> callback) {
			_onStandardError.Add(callback);
			return this;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleProcess"/> class.
		/// </summary>
		/// <param name="env">The <see cref="IPipelineEnvironment"/></param>
		/// <param name="parent">The <see cref="Pipeline"/></param>
		/// <param name="name">The <see cref="string"/></param>
		/// <param name="command">The <see cref="string"/></param>
		/// <param name="arguments">The <see cref="string"/></param>
		/// <param name="workingDirectory">The <see cref="string"/></param>
		public SimpleProcess(string name, string command, string arguments, string workingDirectory) {
			_command = command;
			_arguments = arguments;
			_workingDirectory = workingDirectory;
			_environmentVariables = new Dictionary<string, string>();

			// Make sure we dispose of this object when the application exits so that
			// we don't have orphaned processess all up the wazoo
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

		}

		public SimpleProcess Start() {
			ExecuteShellCommand();
			return this;
		}

		private void CurrentDomain_ProcessExit(object sender, EventArgs e) {
			Dispose();
		}

		public void Dispose() {
			if (_process != null && !_process.HasExited) {
				Log.Information("Simpleshell: {_command} {_arguments}: Killing process with process_id: {processId}", _command, _arguments, _process.Id);
				_process.KillTree();
			}

		}

		/// <summary>
		/// The GetCommand
		/// </summary>
		/// <returns>The <see cref="string"/></returns>
		protected virtual string GetCommand() {
			return _command;
		}

		public bool WaitForOutput(string outputPrefix, TimeSpan timeout) {
			var start = DateTime.UtcNow;

			while (true) {
				if (StandardOut.ToString().Contains($"\n{outputPrefix}")) {
					return true;
				}
				if (timeout < DateTime.UtcNow.Subtract(start)) {
					return false;
				}
				System.Threading.Thread.Sleep(100);
			}


		}

		/// <summary>
		/// The WithCustomOutputFilter
		/// </summary>
		/// <param name="filter">The <see cref="Func{string, bool}"/></param>
		/// <returns>The <see cref="SimpleProcess"/></returns>
		public SimpleProcess WithCustomOutputFilter(Func<string, bool> filter) {
			_customOutputFilter = filter;
			return this;
		}

		/// <summary>
		/// The WithWorkingDirectory
		/// </summary>
		/// <param name="workingDirectory">The <see cref="string"/></param>
		/// <returns>The <see cref="SimpleProcess"/></returns>
		public SimpleProcess WithWorkingDirectory(string workingDirectory) {
			_workingDirectory = workingDirectory;
			return this;
		}

		/// <summary>
		/// The WithEnvironmentVariables
		/// </summary>
		/// <param name="variables">The <see cref="Dictionary{string, string}"/></param>
		/// <returns>The <see cref="SimpleProcess"/></returns>
		public SimpleProcess WithEnvironmentVariables(Dictionary<string, string> variables) {
			foreach (var kv in variables) {
				_environmentVariables[kv.Key] = kv.Value;
			}
			return this;
		}

		public SimpleProcess WithEnvironmentVariable(string key, string value) {
			_environmentVariables[key] = value;

			return this;
		}

		/// <summary>
		/// The ExecuteShellCommand
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public bool ExecuteShellCommand() {

			_deadConsoleCalled = false;
			_standardOutReadFinished = false;
			_standardErrorReadFinished = false;

			_process = new Process();
			_process.StartInfo.FileName = GetCommand();
			_process.StartInfo.Arguments = _arguments;

			// Load the current environment into the start info (PATH etc)
			var currentEnvironment = System.Environment.GetEnvironmentVariables();
			foreach (string key in currentEnvironment.Keys) {
				_process.StartInfo.EnvironmentVariables[key] = (string)currentEnvironment[key];
			}

			// Override with explicit ones
			foreach (var kv in _environmentVariables) {
				_process.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;
			}
			_process.StartInfo.EnvironmentVariables["hyper_model_child_process"] = "yes";

			_process.StartInfo.UseShellExecute = false;// Do not use OS shell			

			_process.StartInfo.WorkingDirectory = _workingDirectory;

			_process.StartInfo.RedirectStandardOutput = true;
			_process.StartInfo.RedirectStandardError = true;

			_process.Start();

			ReadOutput(_process.StandardOutput, true, (line) => OutputReceived(line));
			ReadOutput(_process.StandardError, false, (line) => ErrorReceived(line));

			// Listen for output
			Log.Information("Simpleshell: {_command} {_arguments}: Started process with process_id: {processId}", _command, _arguments, _process.Id);

			// Wait for it to exit
			if (_waitForExit) {
				_process.WaitForExit();
				while (!_standardOutReadFinished || !_standardErrorReadFinished) {
					// Wait a bit longer for any output to also be read
					Thread.Sleep(5);
				}
				Log.Information("Simpleshell: {_command} {_arguments}: Exit with exit code {exitCode}: {processId}", _command, _arguments, _process.ExitCode, _process.Id);
			}
			return true;
		}

		public SimpleProcess WithWaitForExit(bool wait = true) {
			_waitForExit = wait;
			return this;
		}


		private bool _standardErrorReadFinished = false;
		private bool _standardOutReadFinished = false;

		private object _deadLocker = new object();
		private bool _deadConsoleCalled = false;

		private void ReadOutput(StreamReader reader, bool isStandardOutput, Action<string> lineRead) {

			var lineBuffer = new StringBuilder();
			Task.Run(() => {
				var buff = 0;
				try {
					while (!_process.HasExited || buff != -1) {
						buff = reader.Read();
						if (buff > -1) {
							var c = (char)buff;
							lineBuffer.Append(c);
							if (c == '\n' || c == '\r') {
								var line = lineBuffer.ToString().Trim();
								if (line.Length > 0) {
									lineRead(line);
								}
								lineBuffer.Clear();
							}
						}
					}
				} catch (Exception ex) {
					SimpleConsoleWriter.Instance.Error("SimpleProcess.ReadOutput", ex);

				} finally {
					if (isStandardOutput) {
						_standardOutReadFinished = true;
					}
					if (!isStandardOutput) {
						_standardErrorReadFinished = true;
					}

					lock (_deadLocker) {
						if (!_deadConsoleCalled) {
							foreach (var action in _onExit) {
								action(this);
							}
						}
						Log.Error($"Dead process: {_process.StartInfo.FileName} {_process.StartInfo.Arguments} (pid: {_process.Id})");
					}
				}
			});
		}

		/// <summary>
		/// The KillProcessTree
		/// </summary>
		public void KillProcessTree() {
			HasBeenKilled = true;
			_process.KillTree();
		}

		/// <summary>
		/// The OutputReceived
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="args">The <see cref="DataReceivedEventArgs"/></param>
		protected virtual void OutputReceived(string line) {
			if (string.IsNullOrEmpty(line)) {
				return;
			}
			_standardOutput.AppendLine(line);
			foreach (var callback in _onStandardOut) {
				try {
					callback(this, line);
				} catch {

				}
			}
		}

		/// <summary>
		/// The ErrorReceived
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="args">The <see cref="DataReceivedEventArgs"/></param>
		protected virtual void ErrorReceived(string line) {
			if (string.IsNullOrEmpty(line)) {
				return;
			}
			// Don't log it directly (as an error), as we will save it all up for an exception when the 
			// shell command exists
			_standardError.AppendLine(line);
			foreach (var callback in _onStandardError) {
				try {
					callback(this, line);
				} catch {

				}
			}
		}
	}
}
