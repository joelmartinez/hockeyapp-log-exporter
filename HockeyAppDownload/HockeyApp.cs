using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HockeyAppDownload
{
	public static class HockeyApp
	{
		public enum DetailType {
			log,
			text
		}

		public static Task GetLogs(DateTime getUntil, int page) {
			string url = "https://rink.hockeyapp.net/api/2/apps/{0}/crashes?page={1}&symbolicated=1&per_page=100";
			url = string.Format(url, Constants.AppId, page);

			var crashes = WebHelper.Json<RootCrashObject>(url);

			return crashes.ContinueWith(t => {
				var crashPage = t.Result;
				var childTasks = ProcessCrashPage (crashPage, getUntil);

				Task.WaitAll(childTasks.ToArray());
			})
			.ContinueWith(t => {
					Console.Write("Fail getting page #{0}", page);
					foreach(var e in t.Exception.Flatten().InnerExceptions) {
						Console.Write(" - {0}", e.Message);
					}
					Console.WriteLine("");
				}, TaskContinuationOptions.OnlyOnFaulted);;
		}

		public static Task GetDetails(Crash crash, DetailType dtype) {
			Console.WriteLine("\tProcessing crash #{0}", crash.id);
			string url = "https://rink.hockeyapp.net/api/2/apps/{0}/crashes/{1}?format={2}";
			url = string.Format(url, Constants.AppId, crash.id, dtype.ToString());

			var details = WebHelper.Json<string>(url);
			return details.ContinueWith(t => {
				Console.WriteLine("-{0}\t{1}", dtype, t.Result);
			});
		}

		static List<Task> ProcessCrashPage (RootCrashObject crashPage, DateTime getUntil)
		{
			List<Task> childTasks = new List<Task>();
			Console.WriteLine("Processing page {0}", crashPage.current_page);
			DateTime earliestDateSeen = DateTime.Now;

			foreach (var crash in crashPage.crashes) {
				DateTime crashDate = crash.create_at_date;
				if (crashDate < earliestDateSeen) {
					earliestDateSeen = crashDate;
				}
				// add to list
				var dtask = GetDetails(crash, DetailType.log);
				dtask.ContinueWith(t => {
					Console.Write("\tFail getting crash #{0}", crash.id);
					foreach(var e in t.Exception.Flatten().InnerExceptions) {
						Console.Write(" - {0}", e.Message);
					}
					Console.WriteLine("");
				}, TaskContinuationOptions.OnlyOnFaulted);
				dtask.Wait();
				childTasks.Add(dtask);
			}
			if (crashPage.current_page < crashPage.total_pages && earliestDateSeen >= getUntil) {
				// we have more pages to process
				childTasks.Add(GetLogs(getUntil, crashPage.current_page + 1));
			}

			return childTasks;
		}
	}
}

