// Copyright (C) 2013 - 2018 Growing Data Pty Ltd. - All rights reserved.  
// Proprietary and confidential
// Unauthorized copying of this file, via any medium is strictly prohibited without the express
// permission of Growing Data Pty Ltd.
// Contact Terence Siganakis <terence@growingdata.com.au>.

namespace GrowingData.Data {
	using System.Collections.Generic;

	public class DataWarehouseDryRunResult {

		public List<SqlColumn> Columns;
		public long EstimatedBytesProcessed;
		public bool Success;
		public string ErrorMessage;
		public string Query;
	}

}
