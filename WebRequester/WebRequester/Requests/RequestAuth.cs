using System;
using System.Text;

namespace WebRequester.Requests
{
	public sealed class RequestAuth
	{
		public string Username { get; }
		public string Password { get; }

		public RequestAuth(string username, string password)
		{
			Username = username ?? throw new ArgumentNullException(nameof(username));
			Password = password ?? string.Empty;
		}

		public static RequestAuth Basic(string username, string password)
		{
			return new RequestAuth(username, password);
		}

		internal string ToAuthorizationHeader()
		{
			string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(Username + ":" + Password));
			return "Basic " + credentials;
		}
	}
}
