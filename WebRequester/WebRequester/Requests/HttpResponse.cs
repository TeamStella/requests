using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace WebRequester.Requests
{
	public sealed class HttpResponse : IDisposable
	{
		private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
		private readonly byte[] _content;
		private string _text;

		public HttpStatusCode StatusCode { get; }
		public string Reason { get; }
		public string Url { get; }
		public IDictionary<string, string> Headers { get; }
		public IDictionary<string, string> Cookies { get; }
		public TimeSpan Elapsed { get; }
		public bool Success { get; }
		public string ErrorMessage { get; }
		public Exception Exception { get; }
		public bool Ok => Success;

		public string Text
		{
			get
			{
				if (_text == null)
					_text = _content != null ? Encoding.UTF8.GetString(_content) : string.Empty;
				return _text;
			}
		}

		public byte[] Content => _content ?? Array.Empty<byte>();

		internal HttpResponse(
			HttpStatusCode statusCode,
			string reason,
			string url,
			IDictionary<string, string> headers,
			IDictionary<string, string> cookies,
			byte[] content,
			TimeSpan elapsed,
			bool success,
			string errorMessage = null,
			Exception exception = null)
		{
			StatusCode = statusCode;
			Reason = reason ?? string.Empty;
			Url = url ?? string.Empty;
			Headers = headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Cookies = cookies ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_content = content;
			Elapsed = elapsed;
			Success = success;
			ErrorMessage = errorMessage;
			Exception = exception;
		}

		internal static HttpResponse FromException(string url, Exception exception, TimeSpan elapsed)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			return new HttpResponse(
				0,
				"Connection Failed",
				url,
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
				Array.Empty<byte>(),
				elapsed,
				success: false,
				errorMessage: exception.Message,
				exception: exception);
		}

		public T Json<T>()
		{
			if (string.IsNullOrWhiteSpace(Text))
				throw new InvalidOperationException("Response body is empty.");

			return JsonSerializer.Deserialize<T>(Text);
		}

		public Dictionary<string, object> Json()
		{
			if (string.IsNullOrWhiteSpace(Text))
				throw new InvalidOperationException("Response body is empty.");

			return JsonSerializer.Deserialize<Dictionary<string, object>>(Text);
		}

		public HttpResponse RaiseForStatus()
		{
			if (!Success)
				throw new HttpRequestException(this);
			return this;
		}

		public override string ToString()
		{
			if (Success)
				return "[SUCCESS] HTTP " + (int)StatusCode + " " + Reason;

			string code = (int)StatusCode > 0 ? ((int)StatusCode).ToString() : "—";
			string message = string.IsNullOrEmpty(ErrorMessage) ? Reason : ErrorMessage;
			return "[FAILED] HTTP " + code + " — " + message;
		}

		public void Dispose()
		{
		}
	}
}
