using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HockeyAppDownload
{	
	internal static class WebHelper
	{
		private static DateTime lastRequest = DateTime.Now;

		/// <summary>Does an HTTP get on the supplied URL</summary>
		public static Task<T> Json<T>(string url)
		{
			return Json<T>(url, string.Empty);
		}
		
		/// <summary>Does an HTTP get if no post data is supplied, otherwise, a post</summary>
		public static Task<T> Json<T>(string url, string postData)
		{
			var diff = DateTime.Now - lastRequest;
			if (diff.Milliseconds <= 1000) {
				Console.Write("...");
				Thread.Sleep(1000 - diff.Milliseconds);
				lastRequest = DateTime.Now;
			}
			var tcs = new TaskCompletionSource<T>();
			var request = (HttpWebRequest)WebRequest.Create(url);
			
			if (!string.IsNullOrWhiteSpace(postData))
			{
				request.AllowWriteStreamBuffering = true;
				request.Method = "POST";
				request.ContentLength = postData.Length;
				request.ContentType = "application/x-www-form-urlencoded";

				
				StreamWriter writer = new StreamWriter(request.GetRequestStream());
				writer.Write(postData);
			}

			request.Headers["X-HockeyAppToken"] = Constants.Token;

			try
			{
				request.BeginGetResponse(iar =>
				                         {
					HttpWebResponse response = null;
					try
					{
						response = (HttpWebResponse)request.EndGetResponse(iar);
						if (response.StatusCode == HttpStatusCode.OK)
						{
							var sreader = new StreamReader(response.GetResponseStream());
							var result = sreader.ReadToEnd();

							if (typeof(T) == typeof(string)) {
								// if it's a string we're trying for, no need to keep going
								tcs.SetResult((T)(object)result);
								return;
							}
							DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
							byte[] bytes = Encoding.UTF8.GetBytes(result);
							using (var stream = new MemoryStream(bytes))
							{
								var deserialized = serializer.ReadObject(stream);
								
								tcs.SetResult( (T)deserialized );
							}
						}
						else
						{
							tcs.SetResult(default(T));
						}
					}
					catch (Exception exc) { tcs.SetException(exc); }
					finally { if (response != null) response.Close(); }
				}, null);
			}
			catch (Exception exc) { tcs.SetException(exc); }
			return tcs.Task;
			
		}
	}
}