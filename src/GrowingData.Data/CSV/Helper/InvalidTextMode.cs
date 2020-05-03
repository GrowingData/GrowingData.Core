// Adapted from: https://bitbucket.org/nxt/csv-toolkit

namespace GrowingData.Data {
	public enum InvalidTextMode
	{
		/// <summary>
		/// Ignore extra characters that appear after a quoted column (e.g. "data" 1;"")
		/// </summary>
		Ignore,
		/// <summary>
		/// Throw an exception when extra characters appear after a quoted column
		/// </summary>
		Throw
	}

}

