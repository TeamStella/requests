using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WebRequester.Requests
{
	public sealed class HttpSession : IDisposable
	{
		private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

		private readonly HttpClientHandler _handler;
		private readonly HttpClient _client;
		private bool _disposed;

		public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public TimeSpan? Timeout { get; set; }
		public bool AllowRedirects { get; set; } = true;
		public bool VerifySsl { get; set; } = true;
		public RequestAuth Auth { get; set; }

		public HttpSession()
		{
			_handler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				UseCookies = true,
				CookieContainer = new CookieContainer()
			};
			_client = new HttpClient(_handler);
		}

		public HttpResponse Get(string url, RequestOptions options = null) => Request("GET", url, options);
		public HttpResponse Post(string url, RequestOptions options = null) => Request("POST", url, options);
		public HttpResponse Put(string url, RequestOptions options = null) => Request("PUT", url, options);
		public HttpResponse Delete(string url, RequestOptions options = null) => Request("DELETE", url, options);
		public HttpResponse Patch(string url, RequestOptions options = null) => Request("PATCH", url, options);
		public HttpResponse Head(string url, RequestOptions options = null) => Request("HEAD", url, options);
		public HttpResponse Options(string url, RequestOptions options = null) => Request("OPTIONS", url, options);

		public Task<HttpResponse> GetAsync(string url, RequestOptions options = null) => RequestAsync("GET", url, options);
		public Task<HttpResponse> PostAsync(string url, RequestOptions options = null) => RequestAsync("POST", url, options);
		public Task<HttpResponse> PutAsync(string url, RequestOptions options = null) => RequestAsync("PUT", url, options);
		public Task<HttpResponse> DeleteAsync(string url, RequestOptions options = null) => RequestAsync("DELETE", url, options);
		public Task<HttpResponse> PatchAsync(string url, RequestOptions options = null) => RequestAsync("PATCH", url, options);
		public Task<HttpResponse> HeadAsync(string url, RequestOptions options = null) => RequestAsync("HEAD", url, options);
		public Task<HttpResponse> OptionsAsync(string url, RequestOptions options = null) => RequestAsync("OPTIONS", url, options);

		public HttpResponse Request(string method, string url, RequestOptions options = null)
		{
			return RequestAsync(method, url, options).GetAwaiter().GetResult();
		}

		public async Task<HttpResponse> RequestAsync(string method, string url, RequestOptions options = null)
		{
			if (string.IsNullOrWhiteSpace(method))
				throw new ArgumentException("HTTP method is required.", nameof(method));
			if (string.IsNullOrWhiteSpace(url))
				throw new ArgumentException("URL is required.", nameof(url));

			options = options ?? new RequestOptions();
			ApplySessionSettings(options);

			var stopwatch = Stopwatch.StartNew();

			try
			{
				using (var request = BuildRequestMessage(method, url, options))
				using (var response = await _client.SendAsync(request, GetCompletionOption(method)).ConfigureAwait(false))
				{
					byte[] content = method.Equals("HEAD", StringComparison.OrdinalIgnoreCase)
						? Array.Empty<byte>()
						: await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

					stopwatch.Stop();
					return BuildResponse(response, request.RequestUri != null ? request.RequestUri.ToString() : url, content, stopwatch.Elapsed);
				}
			}
			catch (Exception ex) when (IsTransportError(ex))
			{
				stopwatch.Stop();
				return HttpResponse.FromException(url, ex, stopwatch.Elapsed);
			}
		}

		private static bool IsTransportError(Exception ex)
		{
			return ex is System.Net.Http.HttpRequestException
				|| ex is WebException
				|| ex is SocketException
				|| ex is IOException
				|| ex is TaskCanceledException;
		}

		private void ApplySessionSettings(RequestOptions options)
		{
			_handler.AllowAutoRedirect = options.AllowRedirects;
			if (options.VerifySsl)
				_handler.ServerCertificateCustomValidationCallback = null;
			else
				_handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;

			if (Timeout.HasValue)
				_client.Timeout = Timeout.Value;
			else if (options.Timeout.HasValue)
				_client.Timeout = options.Timeout.Value;
		}

		private HttpRequestMessage BuildRequestMessage(string method, string url, RequestOptions options)
		{
			var request = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), BuildUri(url, options.Params));
			ApplyHeaders(request, options);
			ApplyCookies(request, options);
			ApplyAuth(request, options);
			ApplyBody(request, options);
			return request;
		}

		private void ApplyHeaders(HttpRequestMessage request, RequestOptions options)
		{
			foreach (var header in Headers)
				TryAddHeader(request, header.Key, header.Value);

			if (options.Headers != null)
			{
				foreach (var header in options.Headers)
					TryAddHeader(request, header.Key, header.Value);
			}
		}

		private void TryAddHeader(HttpRequestMessage request, string name, string value)
		{
			if (string.IsNullOrWhiteSpace(name) || value == null)
				return;

			if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
				return;

			if (!request.Headers.TryAddWithoutValidation(name, value))
				request.Content?.Headers.TryAddWithoutValidation(name, value);
		}

		private void ApplyCookies(HttpRequestMessage request, RequestOptions options)
		{
			var cookiePairs = new List<string>();

			foreach (var cookie in Cookies)
				cookiePairs.Add(cookie.Key + "=" + Uri.EscapeDataString(cookie.Value));

			if (options.Cookies != null)
			{
				foreach (var cookie in options.Cookies)
					cookiePairs.Add(cookie.Key + "=" + Uri.EscapeDataString(cookie.Value));
			}

			if (cookiePairs.Count > 0)
				request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookiePairs));
		}

		private void ApplyAuth(HttpRequestMessage request, RequestOptions options)
		{
			var auth = options.Auth ?? Auth;
			if (auth != null)
				request.Headers.TryAddWithoutValidation("Authorization", auth.ToAuthorizationHeader());
		}

		private void ApplyBody(HttpRequestMessage request, RequestOptions options)
		{
			if (options.Files != null && options.Files.Any())
			{
				request.Content = BuildMultipartContent(options);
				return;
			}

			if (options.Json != null)
			{
				string json = JsonSerializer.Serialize(options.Json);
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
				return;
			}

			if (options.Data == null)
				return;

			if (options.Data is byte[] bytes)
			{
				request.Content = new ByteArrayContent(bytes);
				return;
			}

			if (options.Data is string text)
			{
				request.Content = new StringContent(text, Encoding.UTF8);
				return;
			}

			if (options.Data is IDictionary<string, string> form)
			{
				request.Content = new FormUrlEncodedContent(form);
				return;
			}

			if (options.Data is IEnumerable<KeyValuePair<string, string>> pairs)
			{
				request.Content = new FormUrlEncodedContent(pairs);
				return;
			}

			throw new ArgumentException("Unsupported data type. Use string, byte[], or IDictionary<string, string>.");
		}

		private HttpContent BuildMultipartContent(RequestOptions options)
		{
			var form = new MultipartFormDataContent();

			if (options.Data is IDictionary<string, string> fields)
			{
				foreach (var field in fields)
					form.Add(new StringContent(field.Value ?? string.Empty), field.Key);
			}
			else if (options.Data is IEnumerable<KeyValuePair<string, string>> pairs)
			{
				foreach (var field in pairs)
					form.Add(new StringContent(field.Value ?? string.Empty), field.Key);
			}

			foreach (var file in options.Files)
			{
				if (file == null || file.Data == null || file.Data.Length == 0)
					continue;

				var fileContent = new ByteArrayContent(file.Data);
				if (!string.IsNullOrEmpty(file.ContentType))
					fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

				form.Add(fileContent, file.Name ?? "file", file.FileName ?? "file.bin");
			}

			return form;
		}

		private static string BuildUri(string url, IDictionary<string, string> query)
		{
			if (query == null || query.Count == 0)
				return url;

			var builder = new StringBuilder(url);
			builder.Append(url.Contains("?") ? "&" : "?");

			bool first = true;
			foreach (var pair in query)
			{
				if (!first)
					builder.Append("&");
				builder.Append(Uri.EscapeDataString(pair.Key));
				builder.Append("=");
				builder.Append(Uri.EscapeDataString(pair.Value ?? string.Empty));
				first = false;
			}

			return builder.ToString();
		}

		private static HttpCompletionOption GetCompletionOption(string method)
		{
			return method.Equals("HEAD", StringComparison.OrdinalIgnoreCase)
				? HttpCompletionOption.ResponseHeadersRead
				: HttpCompletionOption.ResponseContentRead;
		}

		private HttpResponse BuildResponse(HttpResponseMessage response, string url, byte[] content, TimeSpan elapsed)
		{
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var header in response.Headers)
				headers[header.Key] = string.Join(", ", header.Value);

			if (response.Content != null)
			{
				foreach (var header in response.Content.Headers)
					headers[header.Key] = string.Join(", ", header.Value);
			}

			var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
			{
				foreach (var setCookie in setCookieHeaders)
					ParseSetCookie(setCookie, cookies);
			}

			bool success = response.IsSuccessStatusCode;

			return new HttpResponse(
				response.StatusCode,
				response.ReasonPhrase,
				url,
				headers,
				cookies,
				content,
				elapsed,
				success: success,
				errorMessage: success ? null : response.ReasonPhrase);
		}

		private static void ParseSetCookie(string setCookie, IDictionary<string, string> cookies)
		{
			if (string.IsNullOrWhiteSpace(setCookie))
				return;

			string pair = setCookie.Split(';')[0];
			int index = pair.IndexOf('=');
			if (index <= 0)
				return;

			string name = pair.Substring(0, index).Trim();
			string value = pair.Substring(index + 1).Trim();
			cookies[name] = value;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_client.Dispose();
			_handler.Dispose();
			_disposed = true;
		}
	}
}
