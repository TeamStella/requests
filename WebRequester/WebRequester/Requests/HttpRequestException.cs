using System;
using System.Net;

namespace WebRequester.Requests
{
	public sealed class HttpRequestException : Exception
	{
		public HttpStatusCode StatusCode { get; }
		public HttpResponse Response { get; }

		public HttpRequestException(HttpResponse response)
			: base(BuildMessage(response))
		{
			Response = response;
			StatusCode = response != null ? response.StatusCode : 0;
		}

		private static string BuildMessage(HttpResponse response)
		{
			if (response == null)
				return "HTTP request failed.";

			if (response.Exception != null)
				return "Connection failed for " + response.Url + ": " + response.Exception.Message;

			return (int)response.StatusCode + " " + response.Reason + " for " + response.Url;
		}
	}
}
