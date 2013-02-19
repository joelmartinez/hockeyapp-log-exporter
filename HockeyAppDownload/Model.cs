using System;
using System.Collections.Generic;

namespace HockeyAppDownload
{
	public class Crash
	{
		public string model { get; set; }
		public bool has_log { get; set; }
		public string oem { get; set; }
		public string created_at { get; set; }
		public DateTime create_at_date {
			get { return DateTime.Parse(this.created_at); }
		}
		public string updated_at { get; set; }
		public bool has_description { get; set; }
		public string bundle_short_version { get; set; }
		public int id { get; set; }
		public int app_id { get; set; }
		public int app_version_id { get; set; }
		public int crash_reason_id { get; set; }
		public string bundle_version { get; set; }
		public string user_string { get; set; }
		public string os_version { get; set; }
		public bool jail_break { get; set; }
		public string contact_string { get; set; }
	}
	
	public class RootCrashObject
	{
		public List<Crash> crashes { get; set; }
		public int total_entries { get; set; }
		public int total_pages { get; set; }
		public int per_page { get; set; }
		public string status { get; set; }
		public int current_page { get; set; }
	}
}

