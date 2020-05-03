// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {

	/// <summary>
	/// Defines the <see cref="CsvFileOptions" />
	/// </summary>
	public class CsvFileOptions {
		/// <summary>
		/// Defines the HasHeaders
		/// </summary>
		public bool HasHeaders = true;

		/// <summary>
		/// Defines the IsGZipped
		/// </summary>
		public bool IsGZipped = false;

		/// <summary>
		/// Defines the FieldTerminator
		/// </summary>
		public char FieldTerminator = '\t';
		public char QuoteChar = '"';

		public bool ExcelQuoted = true;
		public bool IsFolderOfCsv = false;
		public bool KeepWhiteSpaceAroundSeparators = true;
		public bool DisableQuotedFields = false;
		public EndOfLineStyle EndOfLineStyle = EndOfLineStyle.Mixed;
		public InvalidTextMode InvalidTextAction = InvalidTextMode.Throw;


	}

	
}
