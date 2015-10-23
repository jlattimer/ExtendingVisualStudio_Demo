using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace $rootnamespace$
{
	class $safeitemrootname$
	{
		public static void MakeRequest(string serviceUrl, string token)
		{
			Task.WaitAll(Task.Run(async () => await DoWork(serviceUrl, token)));
		}

		private static async Task DoWork(string serviceUrl, string token)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(serviceUrl);
				httpClient.Timeout = new TimeSpan(0, 2, 0);
				httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
				httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
				httpClient.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", token);

				HttpResponseMessage response =
					await httpClient.GetAsync("api/data/WhoAmI");
			}
		}
	}
}
