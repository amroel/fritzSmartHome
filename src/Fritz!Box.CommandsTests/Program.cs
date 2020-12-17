using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FritzSmartHome.FritzBox;

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
			using var tokenSource = new CancellationTokenSource();
			var cancellationToken = tokenSource.Token;
			try
			{
				var sessionId = await LogIn(boxUrl, user, pass, cancellationToken);
				Console.WriteLine($"Successfull login for user: {user}");
				Console.WriteLine($"Session Id: {sessionId}");
			}
			catch(WebException e)//occures if boxUrl is not reachable!
			{
				Console.WriteLine($"Something went wrong: {e}");
			}
		}

		private static async Task<SessionInfo> LogIn(string boxUrl, 
			string user, 
			string pass, 
			CancellationToken cancellationToken = default)
		{
			//Get a sid by solving the PBKDF2 (or MD5) challenge-response process.
			var url = new Uri(boxUrl + SID_ROUTE);
			var logInRequest = WebRequest.Create(url);
			using var logInResponse = await logInRequest.GetResponseAsync().ConfigureAwait(false);
			SessionInfo sessionInfo = null;
			await Task.Run(() =>
			{
				var logInResponseSerializer = new XmlSerializer(typeof(SessionInfo));
				sessionInfo = (SessionInfo)logInResponseSerializer.Deserialize(logInResponse.GetResponseStream());
			}, cancellationToken);
			return sessionInfo;
		}
	}
}
