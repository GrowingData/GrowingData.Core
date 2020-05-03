// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;

	/// <summary>
	/// Defines the <see cref="ProcessSupervisor" />
	/// </summary>
	public class ProcessSupervisor {
		/// <summary>
		/// Defines the _process
		/// </summary>
		private Process _process;

		/// <summary>
		/// Defines the _processParentPid
		/// </summary>
		private int _processParentPid;

		/// <summary>
		/// Defines the _myPid
		/// </summary>
		private int _myPid;

		/// <summary>
		/// Defines the _processCommand
		/// </summary>
		private string _processCommand;

		/// <summary>
		/// Defines the _processArgs
		/// </summary>
		private string _processArgs;

		/// <summary>
		/// Defines the _processWorkingDirectory
		/// </summary>
		private string _processWorkingDirectory;

		/// <summary>
		/// Defines the _parentExited
		/// </summary>
		private bool _parentExited = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessSupervisor"/> class.
		/// </summary>
		/// <param name="args">The <see cref="string[]"/></param>
		public ProcessSupervisor(string[] args) {
			var argsMap = ArgumentBinder.Bind(args);
			System.Diagnostics.Debugger.Break();

			_processCommand = argsMap["cmd"];
			_processArgs = argsMap["args"];
			_processWorkingDirectory = argsMap["wd"];
			_processParentPid = int.Parse(argsMap["ppid"]);
			_myPid = Process.GetCurrentProcess().Id;

			Console.WriteLine($"PS:{_myPid}: init  {_processCommand} {_processArgs} ({_processWorkingDirectory})");

			_process = new Process();
			_process.StartInfo.FileName = _processCommand;
			_process.StartInfo.Arguments = _processArgs;

			// Load the current environment into the start info (PATH etc)
			var currentEnvironment = Environment.GetEnvironmentVariables();
			foreach (string key in currentEnvironment.Keys) {
				_process.StartInfo.EnvironmentVariables[key] = (string)currentEnvironment[key];
			}


			_process.StartInfo.WorkingDirectory = _processWorkingDirectory;

			_process.StartInfo.RedirectStandardOutput = true;
			_process.StartInfo.RedirectStandardError = true;
			_process.StartInfo.RedirectStandardInput = true;
			_process.StartInfo.UseShellExecute = false;
			//_process.StartInfo.CreateNoWindow = true;
			_process.OutputDataReceived += OutputReceived;
			_process.ErrorDataReceived += ErrorReceived;

			Task.Run(() => CheckParentExit());
		}

		/// <summary>
		/// The Start
		/// </summary>
		/// <returns>The <see cref="bool"/></returns>
		public bool Start() {
			if (_parentExited) {
				return false;
			}
			_process.Start();

			// Listen for output
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();

			Console.WriteLine($"PS.{_myPid}: child_start (childPid={_process.Id}) {_processCommand} {_processArgs}");

			// Wait for it to exit
			_process.WaitForExit();

			Console.WriteLine($"PS.{_myPid}: child_exit (childPid={_process.Id}) {_processCommand} {_processArgs}");
			return true;
		}

		/// <summary>
		/// The CheckParentExit
		/// </summary>
		public void CheckParentExit() {
			while (true) {
				var parent = Process.GetProcessById(_processParentPid);
				if (parent == null || parent.HasExited) {
					_process.KillTree();
					_parentExited = true;
					Console.WriteLine($"PS.{_myPid}: parent_exit {_processCommand} {_processArgs}");
					return;
				}
			}
		}

		/// <summary>
		/// The KillProcessTree
		/// </summary>
		public void KillProcessTree() {
			_process.StandardInput.Close();
			_process.KillTree();
		}

		/// <summary>
		/// The OutputReceived
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="args">The <see cref="DataReceivedEventArgs"/></param>
		private void OutputReceived(object sender, DataReceivedEventArgs args) {
			if (string.IsNullOrEmpty(args.Data)) {
				return;
			}
			Console.Out.WriteLine(args.Data);
		}

		/// <summary>
		/// The ErrorReceived
		/// </summary>
		/// <param name="sender">The <see cref="object"/></param>
		/// <param name="args">The <see cref="DataReceivedEventArgs"/></param>
		private void ErrorReceived(object sender, DataReceivedEventArgs args) {
			if (string.IsNullOrEmpty(args.Data)) {
				return;
			}
			Console.Out.WriteLine(args.Data);
		}
	}
}
