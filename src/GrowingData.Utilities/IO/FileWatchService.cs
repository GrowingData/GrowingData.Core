using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GrowingData.Utilities {
	public class FileUpdate {
		public FileInfo TargetFile;
		public string DeletedFileFullPath;
		public WatcherChangeTypes ChangeType;
		public DateTime NotifiedAt;
	}

	public class FileWatchService {

		protected FileSystemWatcher _watcher;
		protected AutoResetEvent _requireUpdate;
		private string _watchPath;
		private ConcurrentDictionary<string, FileUpdate> _fileUpdates;
		private bool _notifyingOfChanges = false;

		private List<Action<FileUpdate>> _updateCallbacks;

		public FileWatchService(string watchPath, string filter) {
			_watchPath = watchPath;

			_requireUpdate = new AutoResetEvent(false);
			_updateCallbacks = new List<Action<FileUpdate>>();
			_fileUpdates = new ConcurrentDictionary<string, FileUpdate>();


			_watcher = new FileSystemWatcher(_watchPath, filter);
			_watcher.Changed += _watcher_Changed;
			_watcher.Renamed += _watcher_Renamed;
			_watcher.Deleted += _watcher_Deleted;
			_watcher.Created += _watcher_Created;
			_watcher.IncludeSubdirectories = true;
			_watcher.EnableRaisingEvents = true;

			Task.Run(() => {
				Start();
			});

		}


		protected void Start() {
			while (true) {
				if (_requireUpdate.WaitOne()) {
					while (_notifyingOfChanges) {
						Task.Delay(1000).Wait();
					}
					ProcessChanges();

				}
			}
		}

		protected void ProcessChanges() {

			try {
				// This is where we actually do the work....
				Log.Information("FileWatchService.ProcessChanges: Got {numChanges} changes", _fileUpdates.Count);

				// Give the process that modified the file a chance to finish.
				Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();

				// Grab the current batch of changes
				var updates = _fileUpdates.ToList();
				_notifyingOfChanges = true;
				foreach (var update in updates) {
					foreach (var callback in _updateCallbacks) {
						callback(update.Value);
					}
				}

			} catch (Exception ex) {
				Log.Error(ex, "FileWatchService.ProcessChanges");
				SimpleConsoleWriter.Instance.Error($"Invalid model update ", ex);
			} finally {
				_notifyingOfChanges = false;
			}

		}

		public FileWatchService OnUpdate(Action<FileUpdate> callback) {
			_updateCallbacks.Add(callback);
			return this;
		}

		private void _watcher_Changed(object sender, FileSystemEventArgs e) {
			if (!ValidateChange(e.FullPath)) {
				return;
			}

			var newFile = new FileInfo(e.FullPath);
			var yamlFile = newFile;
			if (yamlFile == null) {
				return;
			}

			_fileUpdates[yamlFile.FullName] = new FileUpdate() {
				ChangeType = WatcherChangeTypes.Changed,
				TargetFile = yamlFile,
				DeletedFileFullPath = null,
				NotifiedAt = DateTime.UtcNow
			};
			//ConsoleWriter.Simple.Information($"Change detected in {yamlFile.FullName}");
			_requireUpdate.Set();
		}


		private void _watcher_Created(object sender, FileSystemEventArgs e) {
			if (!ValidateChange(e.FullPath)) {
				return;
			}
			var newFile = new FileInfo(e.FullPath);


			var createdYamlFile = newFile;
			if (createdYamlFile != null) {
				_fileUpdates[createdYamlFile.FullName] = new FileUpdate() {
					ChangeType = WatcherChangeTypes.Created,
					TargetFile = createdYamlFile,
					DeletedFileFullPath = null,
					NotifiedAt = DateTime.UtcNow
				};
			}


			_requireUpdate.Set();
		}

		private bool ValidateChange(string fullPath) {
			try {
				//detect whether its a directory or file
				if (Directory.Exists(fullPath)) {
					return false;
				}

				// Check the things that HyperModel writes
				if (PathContainsSlug(fullPath, ".schema")
					|| PathContainsSlug(fullPath, ".out")
					|| PathContainsSlug(fullPath, ".data")
					|| PathContainsSlug(fullPath, ".cache")
					|| PathContainsSlug(fullPath, "__pycache__")) {
					return false;
				}
				// Visual studio Code will also create a temp file that
				// has a portion of the filenam as a Guid, which we have no
				// interest in working with.

				var fileName = Path.GetFileName(fullPath);
				var segments = fileName.Split('.');
				foreach (var segment in segments) {
					if (Guid.TryParse(segment, out var guid)) {
						return false;
					}
				}


				return true;
			} catch (Exception ex) {
				Log.Information("Unable to verify watched path: {fullPath}: {message}", fullPath, ex.Message);
				return false;
			}
		}

		private bool PathContainsSlug(string fullPath, string slug) {
			var wrappedSlug = $"{Path.DirectorySeparatorChar}{slug}";
			return fullPath.Contains(wrappedSlug);

		}

		private void _watcher_Deleted(object sender, FileSystemEventArgs e) {
			if (!ValidateChange(e.FullPath)) {
				return;
			}


			var oldFile = new FileInfo(e.FullPath);
			var deletedYamlFile = oldFile;
			if (deletedYamlFile != null) {
				// Just treat it as a deletion, since 
				_fileUpdates[deletedYamlFile.FullName] = new FileUpdate() {
					ChangeType = WatcherChangeTypes.Deleted,
					TargetFile = null,
					DeletedFileFullPath = deletedYamlFile.FullName,
					NotifiedAt = DateTime.UtcNow
				};
				SimpleConsoleWriter.Instance.Information($"Yaml file {e.FullPath} deleted");
			}

		}

		private void _watcher_Renamed(object sender, RenamedEventArgs e) {
			if (!ValidateChange(e.FullPath)) {
				return;
			}
			var oldFile = new FileInfo(e.OldFullPath);
			var newFile = new FileInfo(e.FullPath);




			var deletedYamlFile = oldFile;
			if (deletedYamlFile != null) {
				// Just treat it as a deletion, since 
				_fileUpdates[deletedYamlFile.FullName] = new FileUpdate() {
					ChangeType = WatcherChangeTypes.Deleted,
					TargetFile = null,
					DeletedFileFullPath = deletedYamlFile.FullName,
					NotifiedAt = DateTime.UtcNow
				};
			}

			var createdYamlFile = newFile;
			if (createdYamlFile != null) {
				_fileUpdates[createdYamlFile.FullName] = new FileUpdate() {
					ChangeType = WatcherChangeTypes.Created,
					TargetFile = createdYamlFile,
					DeletedFileFullPath = null,
					NotifiedAt = DateTime.UtcNow
				};
			}


			_requireUpdate.Set();
		}
	}
}
