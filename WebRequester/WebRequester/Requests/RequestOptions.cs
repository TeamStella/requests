using System;
using System.Collections.Generic;

namespace WebRequester.Requests
{
	public sealed class RequestOptions
	{
		public IDictionary<string, string> Params { get; set; }
		public object Data { get; set; }
		public object Json { get; set; }
		public IDictionary<string, string> Headers { get; set; }
		public IDictionary<string, string> Cookies { get; set; }
		public IEnumerable<RequestFile> Files { get; set; }
		public RequestAuth Auth { get; set; }
		public TimeSpan? Timeout { get; set; }
		public bool AllowRedirects { get; set; } = true;
		public bool VerifySsl { get; set; } = true;
	}
}
