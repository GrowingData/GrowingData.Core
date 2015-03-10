using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace GrowingData.Pipeliner {
	public class PipelineSqlConnection {

		public string Name;
		public string ConnectionString;


		public SqlConnection Connection {
			get {
				var cn = new SqlConnection(ConnectionString);
				cn.Open();
				return cn;
			}
		}


	}
}
