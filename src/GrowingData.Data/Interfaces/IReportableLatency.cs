using System.Collections.Generic;


namespace GrowingData.Data {
	public interface IReportableLatency {
		Dictionary<string, double> CheckLatency();

	}
}
