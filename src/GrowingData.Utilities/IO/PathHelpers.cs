// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Utilities {
	using System;
	using System.IO;

	/// <summary>
	/// Defines the <see cref="PathHelpers" />
	/// </summary>
	public class PathHelpers {
		/// <summary>
		/// The CreateDirectoryRecursively
		/// </summary>
		/// <param name="path">The <see cref="string"/></param>
		public static void EnsureDirectoryExists(string path) {
			if (path.StartsWith("~/")) {
				path = System.IO.Path.Combine(UserProfilePath().FullName, path.Substring(2));
			}
			var pathParts = path.Split('\\');

			for (var i = 0; i < pathParts.Length; i++) {
				if (i > 0) {
					pathParts[i] = System.IO.Path.Combine(pathParts[i - 1], pathParts[i]);
				}
				if (!Directory.Exists(pathParts[i])) {
					Directory.CreateDirectory(pathParts[i]);
				}
			}
		}

		public static void EnsureDirectoryExists(DirectoryInfo path) {

			if (!Directory.Exists(path.Parent.FullName)) {
				EnsureDirectoryExists(path.Parent);
			}

			if (!Directory.Exists(path.FullName)) {
				Directory.CreateDirectory(path.FullName);
			}

		}

		/// <summary>
		/// Get the User Profile Path ("~" in *nix)
		/// </summary>
		/// <returns></returns>
		public static DirectoryInfo UserProfilePath() {
			var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var info = new DirectoryInfo(path);

			return info;
		}

		/// <summary>
		/// Using a relative path (including '~', calculate the FilePath and verify that the File exists
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static DirectoryInfo AbsoluteDirectoryPath(string relativePath) {
			if (relativePath.StartsWith("~/")) {
				relativePath = System.IO.Path.Combine(UserProfilePath().FullName, relativePath.Substring(2));
			}
			var directoryInfo = new DirectoryInfo(relativePath);
			return directoryInfo;
		}

		/// <summary>
		/// Using a relative path (including '~', calculate the FilePath and verify that the File exists
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static FileInfo VerifyFileExists(string relativePath) {
			var fileInfo = CheckFile(relativePath);
			if (fileInfo == null) {
				throw new FileNotFoundException($"Unable to find file at: {relativePath}");
			}
			return fileInfo;
		}
		/// <summary>
		/// Using a relative path (including '~', calculate the FilePath and verify that the File exists
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static FileInfo CheckFile(string relativePath) {

			var fileInfo = new FileInfo(AbsoluteFilePath(relativePath));
			if (!File.Exists(fileInfo.FullName)) {
				return null;
			}
			return fileInfo;
		}


		public static string AbsoluteFilePath(string relativePath) {
			if (relativePath.StartsWith("~/")) {
				relativePath = System.IO.Path.Combine(UserProfilePath().FullName, relativePath.Substring(2));
			}
			var fileInfo = new FileInfo(relativePath);
			return fileInfo.FullName;

		}

		/// <summary>
		/// Using a relative path (including '~', calculate the FilePath and verify that the Directory exists
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static DirectoryInfo VerifyDirectoryExists(string relativePath) {

			var dirInfo = AbsoluteDirectoryPath(relativePath);
			if (!Directory.Exists(dirInfo.FullName)) {
				throw new FileNotFoundException($"Unable to find directory at: {dirInfo.FullName}");
			}
			return dirInfo;
		}


		/// <summary>
		/// Using a relative path (including '~', calculate the FilePath and verify that the Directory exists
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static DirectoryInfo CheckDirectory(string relativePath) {
			var dirInfo = AbsoluteDirectoryPath(relativePath);
			if (!Directory.Exists(dirInfo.FullName)) {
				return null;
			}
			return dirInfo;
		}
	}
}
