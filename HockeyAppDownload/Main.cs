using System;

namespace HockeyAppDownload
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			HockeyApp.GetLogs(DateTime.Now.AddDays(-1), 1).Wait();
		}
	}
}
