using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Utilities;
using System.IO;
using GrowingData.Utilities.Csv;

namespace GrowingData.Utilities.Tester {
	public class Program {
		public static string CurrentPath { get { return AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin", "").Replace("\\Debug", ""); } }

		public static void Main(string[] args) {

			var filePath = Path.Combine(CurrentPath, "CsvFiles", "CourseAttributeExport.csv");

			using (var file = new StreamReader(File.OpenRead(filePath))) {
				var reader = new CsvReader(file, new CsvReaderOptions() {
					FirstLineContainsHeaders = true,
					ExcelQuoted = true,
					SeparatorChar = ','
				});

				Console.WriteLine(string.Join("\t", reader.Columns.Select(c => $"{c.ColumnName}:{c.MungType}")));
				while (reader.Read()) {
					Console.WriteLine(string.Join("\t", reader.Columns.Select(c => reader[c.ColumnName])));

				}
			}

			Console.ReadKey();

		}
	}
}
