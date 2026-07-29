using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRequester
{
    public class WebRequester
    {
        public static void Main(string[] args)
        {
			// Before Start, This library is not perfect (because i wrote this for only an hour...)
			// so if you find any bugs or issues, please add github issue or pull request to fix it.
			// I will be very happy to fix it and make it better for you and others Thanks.

			// This Library is inspired by the Python requests library, and I tried to make it as similar as possible to the Python requests library.

			// Example usage of the WebRequester library
			// Make a GET request to a sample URL
			// Note: Check Requests/RequestSample.cs for more examples of how to use the library
			var response = Requests.Requests.Get("https://example.com/data");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Body: {response.Text}");
            Console.WriteLine($"Response: {response}");
		}
	}
}
