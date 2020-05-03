
namespace GrowingData.Data {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using GrowingData.Data;

	public interface IDataWarehouseCursor : IDisposable, IEnumerable<SqlRow> {
	}
}
