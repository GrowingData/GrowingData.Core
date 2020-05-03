// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

using System.Collections.Generic;
using System.IO;

namespace GrowingData.Utilities {
	public interface IEnvironmentService {
		string this[string key] { get; set; }

		//string[] CommandLineArguments { get; }
		DirectoryInfo CurrentWorkingDirectory { get; }
		IEnumerable<string> Keys { get; }
		DirectoryInfo TempPath { get; }

		bool ContainsKey(string variableName);

		bool IsLocalDevelopment { get; }
	}
}
