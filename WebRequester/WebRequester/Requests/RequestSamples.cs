using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebRequester.Requests
{
	public static class RequestSamples
	{
		public static void RunBasicSamples()
		{
			// GET
			HttpResponse getResponse = Requests.Get(
				"https://httpbin.org/get",
				queryParams: new Dictionary<string, string> { { "hello", "world" } },
				headers: new Dictionary<string, string> { { "User-Agent", "WebRequester" } },
				timeout: TimeSpan.FromSeconds(30));

			Console.WriteLine(getResponse.StatusCode);
			Console.WriteLine(getResponse.Text);
			Console.WriteLine(getResponse.Ok);

			// POST form data
			HttpResponse formResponse = Requests.Post(
				"https://httpbin.org/post",
				data: new Dictionary<string, string>
				{
					{ "username", "test" },
					{ "password", "1234" }
				});

			// POST JSON
			HttpResponse jsonResponse = Requests.Post(
				"https://httpbin.org/post",
				json: new Dictionary<string, object>
				{
					{ "name", "WebRequester" },
					{ "version", 1 }
				});

			var parsed = jsonResponse.Json();
			Console.WriteLine(parsed["json"]);

			// Basic Auth
			HttpResponse authResponse = Requests.Get(
				"https://httpbin.org/basic-auth/user/pass",
				auth: RequestAuth.Basic("user", "pass"));

			authResponse.RaiseForStatus();

			// Session — Keep Cookies and Headers
			using (var session = Requests.Session())
			{
				session.Headers["X-Api-Key"] = "my-key";
				session.Cookies["session_id"] = "abc123";

				HttpResponse first = session.Get("https://httpbin.org/cookies");
				HttpResponse second = session.Get("https://httpbin.org/headers");
				Console.WriteLine(second.Text);
			}

			// File Upload
			HttpResponse uploadResponse = Requests.Post(
				"https://httpbin.org/post",
				data: new Dictionary<string, string> { { "description", "sample upload" } },
				files: new[]
				{
					RequestFile.FromText("file", "hello from WebRequester", "hello.txt")
				});
		}

		public static async Task RunAsyncSample()
		{
			HttpResponse response = await Requests.GetAsync("https://httpbin.org/get");
			Console.WriteLine(response.StatusCode);
		}

		public static void PrintResponse(HttpResponse response)
		{
			Console.WriteLine(response);

			if (response.Success)
			{
				if (!string.IsNullOrEmpty(response.Text))
					Console.WriteLine(response.Text);
				return;
			}

			if (!string.IsNullOrEmpty(response.Text))
				Console.WriteLine("Response: " + response.Text);
		}
	}
}
