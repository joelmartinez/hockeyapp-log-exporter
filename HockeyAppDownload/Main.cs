using System;
using System.IO;

namespace HockeyAppDownload
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (File.Exists("results.csv")) {
				File.Delete("results.csv");
			}

			string[] values = {
				"id",
				"created_at",
				"bundle_short_version",
				"bundle_version",
				"jail_break",
				"model",
				"os_version",
				"oem",
				"user_string",
				"error",
				"stack"
			};

			using (var writer = new CsvFileWriter("results.csv", values)) {
				HockeyApp.GetLogs(DateTime.Now.AddDays(-21), 1, writer).Wait();
			}
		}
	}
}
