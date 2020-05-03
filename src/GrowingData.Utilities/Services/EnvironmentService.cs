// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the expresslllll
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Newtonsoft.Json;
	using GrowingData.Utilities;
	using Serilog;

	public class EnvironmentService : IEnvironmentService {

		private Dictionary<string, string> _variables;

		public DirectoryInfo TempPath => new DirectoryInfo(Path.GetTempPath());
		public DirectoryInfo CurrentWorkingDirectory => new DirectoryInfo(".");
		public string[] CommandLineArguments => Environment.GetCommandLineArgs();

		public IEnumerable<string> Keys => _variables.Keys.Select(x => x).OrderBy(x => x).ToList();

		public EnvironmentService() {

			_variables = new Dictionary<string, string>();

			ReadSecrets();
			ReadEnvironment();
			ReadCommandLine();
		}

		private void ReadSecrets() {
			var secretsPath = "../../../.secrets/local-dev";
			var secretsPathEnv = Environment.GetEnvironmentVariable("SECRETS_PATH");
			if (!string.IsNullOrEmpty(secretsPathEnv)) {
				secretsPath = secretsPathEnv;
				Log.Information("EnvironmentService: Loading secrets from: {secretsPath}", secretsPath);
			}
			ReadSecrets(secretsPath);
		}
		private void ReadSecrets(string path) {
			var subDirectories = Directory.GetDirectories(path);
			foreach (var sub in subDirectories) {
				ReadSecrets(sub);
			}

			foreach (var file in Directory.GetFiles(path, "*.json")) {
				try {
					var jsonText = File.ReadAllText(file);
					var jsonValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
					foreach (var k in jsonValues.Keys) {
						_variables[k] = jsonValues[k];
					}
				} catch (Exception ex) {
					Log.Error("EnvironmentService.ReadSecrets: Read failed for {filename} ({message})", file, ex.Message);
				}
			}
		}

		private void ReadEnvironment() {
			var environment = Environment.GetEnvironmentVariables();
			foreach (string key in environment.Keys) {
				var value = (string)environment[key];
				_variables[key] = value;

				//Console.WriteLine($"EnvironmentService: Found variable: {key}: {value}");
			}
		}

		private void ReadCommandLine() {
			// Parse additional command line arguments
			var commandLineArgs = Environment.GetCommandLineArgs()
				.Skip(1)
				.ToArray();

			for (var i = 0; i < commandLineArgs.Length; i++) {
				if (commandLineArgs[i].StartsWith("--")) {
					var name = commandLineArgs[i].Substring(2);

					string next = null;
					if (i + 1 < commandLineArgs.Length) {
						if (!commandLineArgs[i + 1].StartsWith("--")) {
							next = commandLineArgs[i + 1];
							i++;
						}
					}
					if (string.IsNullOrEmpty(next)) {
						_variables[name] = "true";
					} else {
						_variables[name] = next;
					}
				}
			}

		}

		public bool ContainsKey(string variableName) {
			var lowerKey = variableName.ToLower();
			return _variables.ContainsKey(lowerKey);
		}

		public string this[string key] {
			get {
				if (!_variables.ContainsKey(key)) {
					Log.Warning("Unable to find Environment Variable: {variable}", key);
					return null;
				}
				return _variables[key];
			}
			set {
				_variables[key] = value;

			}

		}

		public bool IsLocalDevelopment {
			get {
				if (_variables.ContainsKey("local-dev")) {
					return _variables["local-dev"] == "true";
				}
				return false;
			}
		}

	}
}
