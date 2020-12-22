using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FritzSmartHome.Domain;
using FritzSmartHome.FritzBox.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FritzSmartHome.FritzBox
{
	public class Box : IFritzBox
	{
		private static readonly HttpClient CLIENT = new HttpClient(new HttpClientHandler
		{
			//commands are issues against https:// (ssl protcol) so
			//ignore ssl errors for the moment. Need to investigate this further
			ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
		});
		private const string SID_ROUTE = "/login_sid.lua?version=2";
		private const string COMMANDS_ROUTE = "/webservices/homeautoswitch.lua";

		private readonly BoxSettings _settings;
		private readonly ILogger _logger;
		private readonly Uri _boxUrl;
		private SessionId _sessionId;

		public Box(IOptions<BoxSettings> options, ILogger<Box> logger)
		{
			_settings = options.Value;
			_boxUrl = new Uri($"{_settings.StartUrl}{SID_ROUTE}");
			_logger = logger;
		}

		public async Task<DeviceList> ListDevices(CancellationToken cancellationToken)
		{
			if (NeedToLogin())
				await LoginAsync(cancellationToken);

			HttpResponseMessage response = null;
			DeviceList result = null;
			// Try to read device info page
			try
			{
				var requestUri = new Uri($"{_settings.CommandsUrl}{COMMANDS_ROUTE}?sid={_sessionId.SID}&switchcmd=getdevicelistinfos");
				response = await CLIENT.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError($"Could not load uri. {ex}");
			}
			catch (TaskCanceledException)
			{
				_logger.LogError("Http GET timed out.");
			}
			catch (OperationCanceledException)
			{
				_logger.LogError("Http GET timed out.");
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogError($"URI appears to be wrong. {ex}");
			}

			// If retrieving was successfull, get response
			if (response?.StatusCode == HttpStatusCode.OK)
			{
				try
				{
					await Task.Run(async () =>
					{
						var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						_logger.LogDebug($"Received response: {responseString}");
						var responseSerializer = new XmlSerializer(typeof(DeviceList));
						using var stream = new StringReader(responseString);
						result = (DeviceList)responseSerializer.Deserialize(stream);
					},
					cancellationToken)
						.ConfigureAwait(false);
				}
				catch (InvalidOperationException ex)
				{
					_logger.LogError($"Something went wrong parsing xml response: {ex}");
				}

				_logger.LogInformation($"Got the following device list: {result}");
				response?.Dispose();
			}
			return result;
		}

		private bool NeedToLogin() => _sessionId == default ||
			string.IsNullOrWhiteSpace(_sessionId.SID) ||
			long.Parse(_sessionId.SID) == 0;

		public async Task LoginAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var sessionId = await GetSessionId(cancellationToken);
				var challengeResponse = sessionId.CalculateChallengeResponse(_settings.Password);
				if (sessionId.IsBlocked())
				{
					_logger.LogInformation($"Waiting for {sessionId.BlockTime} seconds...");
					await Task.Delay(TimeSpan.FromSeconds(sessionId.BlockTime), cancellationToken);
				}
				_sessionId = await RespondToChallenge(challengeResponse, cancellationToken);
				_logger.LogInformation($"Successfull login for user: {_settings.User}");
				_logger.LogInformation($"Session Id: {_sessionId}");
			}
			catch (WebException e)//occures if boxUrl is not reachable!
			{
				_logger.LogError($"Something went wrong: {e}");
			}
		}

		private async Task<SessionId> GetSessionId(CancellationToken cancellationToken)
		{
			var challengeRequest = new HttpRequestMessage(HttpMethod.Get, _boxUrl);
			challengeRequest.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/xml");
			SessionId result = null;
			try
			{
				var challengeResponse = await CLIENT.SendAsync(challengeRequest, cancellationToken)
					.ConfigureAwait(false);
				await Task.Run(async () =>
				{
					var responseSerializer = new XmlSerializer(typeof(SessionId));
					using var stream = await challengeResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
					try
					{
						result = (SessionId)responseSerializer.Deserialize(stream);
					}
					catch (Exception e)
					{
						_logger.LogError($"Something went wrong: {e}");
					}
				},
				cancellationToken)
					.ConfigureAwait(false);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError($"Could not load uri: {ex}");
				return null;
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogError($"Task canceled. {ex}");
				return null;
			}
			catch (OperationCanceledException ex)
			{
				_logger.LogError($"Operation canceled: {ex}");
			}

			return result;
		}

		private async Task<SessionId> RespondToChallenge(string challengeResponse, CancellationToken cancellationToken)
		{
			var content = new Dictionary<string, string>
			{
				{ "username", _settings.User },
				{ "response", challengeResponse }
			};
			var encodedContent = new FormUrlEncodedContent(content);
			SessionId result = null;
			try
			{
				var response = await CLIENT.PostAsync(_boxUrl, encodedContent, cancellationToken).ConfigureAwait(false);
				await Task.Run(async () =>
				{
					var responseSerializer = new XmlSerializer(typeof(SessionId));
					using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
					try
					{
						result = (SessionId)responseSerializer.Deserialize(stream);
					}
					catch (Exception e)
					{
						_logger.LogError($"Something went wrong: {e}");
					}
				},
				cancellationToken)
					.ConfigureAwait(false);
				return result;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError($"Could not load uri: {ex}");
				return null;
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogError($"Task canceled. {ex}");
				return null;
			}
			catch (OperationCanceledException ex)
			{
				_logger.LogError($"Operation canceled: {ex}");
			}

			return result;
		}
	}
}
