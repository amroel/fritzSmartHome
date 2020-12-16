using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FritzBox.CommandsTests
{
	class Program
	{
		private const string SID_ROUTE = "/login_sid.lua?version=2";

		static async Task Main(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("usage: Fritz!BoxURL(http://fritz.box) username password");
				Environment.Exit(1);
			}
			var boxUrl = args[0];
			var user = args[1];
			var pass = args[2];
			try
			{
				var sessionId = await LogIn(boxUrl, user, pass);
				Console.WriteLine($"Successfull login for user: {user}");
				Console.WriteLine($"Session Id: {sessionId}");
			}
			catch(WebException e)//occures if boxUrl is not reachable!
			{
				Console.WriteLine($"Something went wrong: {e}");
			}
		}

		private static async Task<string> LogIn(string boxUrl, string user, string pass)
		{
			//Get a sid by solving the PBKDF2 (or MD5) challenge-response process.
			var url = new Uri(boxUrl + SID_ROUTE);
			var logInRequest = WebRequest.Create(url);
			using var logInResponse = await logInRequest.GetResponseAsync().ConfigureAwait(false);
			using var responseStream = logInResponse.GetResponseStream();
			using var reader = new StreamReader(responseStream);
			var content = await reader.ReadToEndAsync(); 
			return content;
		}
	}
}
