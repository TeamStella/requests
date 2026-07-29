using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebRequester.Requests
{
	public static class Requests
	{
		private static readonly HttpSession DefaultSession = new HttpSession();

		public static HttpResponse Get(
			string url,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Get(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl));
		}

		public static HttpResponse Post(
			string url,
			object data = null,
			object json = null,
			IEnumerable<RequestFile> files = null,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Post(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl, data, json, files));
		}

		public static HttpResponse Put(
			string url,
			object data = null,
			object json = null,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Put(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl, data, json));
		}

		public static HttpResponse Delete(
			string url,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Delete(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl));
		}

		public static HttpResponse Patch(
			string url,
			object data = null,
			object json = null,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Patch(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl, data, json));
		}

		public static HttpResponse Head(
			string url,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Head(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl));
		}

		public static HttpResponse Options(
			string url,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.Options(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl));
		}

		public static HttpResponse Request(string method, string url, RequestOptions options = null)
		{
			return DefaultSession.Request(method, url, options);
		}

		public static Task<HttpResponse> GetAsync(
			string url,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.GetAsync(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl));
		}

		public static Task<HttpResponse> PostAsync(
			string url,
			object data = null,
			object json = null,
			IEnumerable<RequestFile> files = null,
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true)
		{
			return DefaultSession.PostAsync(url, BuildOptions(queryParams, headers, auth, timeout, allowRedirects, verifySsl, data, json, files));
		}

		public static Task<HttpResponse> RequestAsync(string method, string url, RequestOptions options = null)
		{
			return DefaultSession.RequestAsync(method, url, options);
		}

		public static HttpSession Session()
		{
			return new HttpSession();
		}

		private static RequestOptions BuildOptions(
			IDictionary<string, string> queryParams = null,
			IDictionary<string, string> headers = null,
			RequestAuth auth = null,
			TimeSpan? timeout = null,
			bool allowRedirects = true,
			bool verifySsl = true,
			object data = null,
			object json = null,
			IEnumerable<RequestFile> files = null)
		{
			return new RequestOptions
			{
				Params = queryParams,
				Headers = headers,
				Auth = auth,
				Timeout = timeout,
				AllowRedirects = allowRedirects,
				VerifySsl = verifySsl,
				Data = data,
				Json = json,
				Files = files
			};
		}
	}
}
