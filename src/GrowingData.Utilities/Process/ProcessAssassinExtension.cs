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
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;

	/// <summary>
	/// Defines the <see cref="ProcessAssassinExtension" />
	/// </summary>
	public static class ProcessAssassinExtension {
		/// <summary>
		/// Defines the _isWindows
		/// </summary>
		private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		/// <summary>
		/// Defines the _defaultTimeout
		/// </summary>
		private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

		/// <summary>
		/// The KillTree
		/// </summary>
		/// <param name="process">The <see cref="Process"/></param>
		public static void KillTree(this Process process) {
			process.KillTree(_defaultTimeout);
		}

		/// <summary>
		/// The KillTree
		/// </summary>
		/// <param name="process">The <see cref="Process"/></param>
		/// <param name="timeout">The <see cref="TimeSpan"/></param>
		public static void KillTree(this Process process, TimeSpan timeout) {
			string stdout;
			if (_isWindows) {
				RunProcessAndWaitForExit(
					"taskkill",
					$"/T /F /PID {process.Id}",
					timeout,
					out stdout);
			} else {
				var children = new HashSet<int>();
				GetAllChildIdsUnix(process.Id, children, timeout);
				foreach (var childId in children) {
					KillProcessUnix(childId, timeout);
				}
				KillProcessUnix(process.Id, timeout);
			}
		
		}

		/// <summary>
		/// The GetAllChildIdsUnix
		/// </summary>
		/// <param name="parentId">The <see cref="int"/></param>
		/// <param name="children">The <see cref="ISet{int}"/></param>
		/// <param name="timeout">The <see cref="TimeSpan"/></param>
		private static void GetAllChildIdsUnix(int parentId, ISet<int> children, TimeSpan timeout) {
			string stdout;
			var exitCode = RunProcessAndWaitForExit(
				"pgrep",
				$"-P {parentId}",
				timeout,
				out stdout);

			if (exitCode == 0 && !string.IsNullOrEmpty(stdout)) {
				using (var reader = new StringReader(stdout)) {
					while (true) {
						var text = reader.ReadLine();
						if (text == null) {
							return;
						}

						int id;
						if (int.TryParse(text, out id)) {
							children.Add(id);
							// Recursively get the children
							GetAllChildIdsUnix(id, children, timeout);
						}
					}
				}
			}
		}

		/// <summary>
		/// The KillProcessUnix
		/// </summary>
		/// <param name="processId">The <see cref="int"/></param>
		/// <param name="timeout">The <see cref="TimeSpan"/></param>
		private static void KillProcessUnix(int processId, TimeSpan timeout) {
			string stdout;
			RunProcessAndWaitForExit(
				"kill",
				$"-TERM {processId}",
				timeout,
				out stdout);
		}

		/// <summary>
		/// The RunProcessAndWaitForExit
		/// </summary>
		/// <param name="fileName">The <see cref="string"/></param>
		/// <param name="arguments">The <see cref="string"/></param>
		/// <param name="timeout">The <see cref="TimeSpan"/></param>
		/// <param name="stdout">The <see cref="string"/></param>
		/// <returns>The <see cref="int"/></returns>
		private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout, out string stdout) {
			var startInfo = new ProcessStartInfo {
				FileName = fileName,
				Arguments = arguments,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			var process = Process.Start(startInfo);

			stdout = null;
			if (process.WaitForExit((int)timeout.TotalMilliseconds)) {
				stdout = process.StandardOutput.ReadToEnd();
			} else {
				process.Kill();
			}

			return process.ExitCode;
		}

		/// <summary>
		/// The StartAndWaitForExitAsync
		/// </summary>
		/// <param name="subject">The <see cref="Process"/></param>
		/// <returns>The <see cref="Task"/></returns>
		public static Task StartAndWaitForExitAsync(this Process subject) {
			var taskCompletionSource = new TaskCompletionSource<object>();

			subject.EnableRaisingEvents = true;

			subject.Exited += (s, a) => {
				taskCompletionSource.SetResult(null);

				subject.Dispose();
			};

			subject.Start();

			return taskCompletionSource.Task;
		}
	}
}
