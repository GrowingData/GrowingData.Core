using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GrowingData.Utilities;
using NLog;

namespace GrowingData.Pipeliner {
	public class PipelineStep {
		public Func<PipelineStep, int, bool> Step;
		public string StepName;
		
		public PipelineStep() {

		}


	}
}
