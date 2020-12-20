using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FritzSmartHome.Domain;
using FritzSmartHome.FritzBox.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FritzBox.CommandsTests
{
	public class ConsoleWorker : BackgroundService
	{
		private const string BOX_URL = "http://fritz.box";
		private const string SID_ROUTE = "/login_sid.lua?version=2";
		private readonly ILogger _logger;
		private readonly IHostApplicationLifetime _appLifetime;
		private readonly IFritzBox _fritzBox;

		public ConsoleWorker(ILogger<ConsoleWorker> logger, IHostApplicationLifetime appLifetime, IFritzBox fritzBox)
		{
			_logger = logger;
			_appLifetime = appLifetime;
			_fritzBox = fritzBox;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _fritzBox.LoginAsync(stoppingToken);
			//var args = new List<string>
			//{
			//	BOX_URL
			//};

			//var configUser = _config["LocalFritzBox:user"];
			//if (configUser != null)
			//	args.Add(configUser);

			//var configPass = _config["LocalFritzBox:pass"];
			//if (configPass != null)
			//	args.Add(configPass);

			//if (args.Count < 3)
			//{
			//	Console.WriteLine("usage: Fritz!BoxURL(http://fritz.box) username password");
			//	_appLifetime.StopApplication();
			//	return;
			//}

			//_logger.LogDebug($"Starting with arguments: {string.Join(" ", args)}");

			//try
			//{
			//	var sessionId = await GetSessionId(args[0], args[1], args[2], stoppingToken);
			//	Console.WriteLine($"Successfull login for user: {args[1]}");
			//	Console.WriteLine($"Session Id: {sessionId}");
			//}
			//catch (WebException e)//occures if boxUrl is not reachable!
			//{
			//	Console.WriteLine($"Something went wrong: {e}");
			//}
			//finally
			//{
			//	_appLifetime.StopApplication();
			//}
		}

		private static async Task<SessionId> GetSessionId(string boxUrl,
			string user,
			string pass,
			CancellationToken cancellationToken = default)
		{
			//Get a sid by solving the PBKDF2 (or MD5) challenge-response process.
			var url = new Uri(boxUrl + SID_ROUTE);
			var logInRequest = WebRequest.Create(url);
			using var logInResponse = await logInRequest.GetResponseAsync().ConfigureAwait(false);
			SessionId sessionInfo = null;
			await Task.Run(() =>
			{
				var logInResponseSerializer = new XmlSerializer(typeof(SessionId));
				sessionInfo = (SessionId)logInResponseSerializer.Deserialize(logInResponse.GetResponseStream());
			}, cancellationToken);
			return sessionInfo;
		}
	}
}
