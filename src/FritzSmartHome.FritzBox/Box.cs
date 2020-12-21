using System;
using System.Collections.Generic;
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
		private static readonly HttpClient CLIENT = new HttpClient();

		private const string SID_ROUTE = "/login_sid.lua?version=2";
		private readonly BoxSettings _settings;
		private readonly ILogger _logger;
		private readonly Uri _boxUrl;
		private SessionId _sessionId;

		public Box(IOptions<BoxSettings> options, ILogger<Box> logger)
		{
			_settings = options.Value;
			_boxUrl = new Uri($"{_settings.Url}{SID_ROUTE}");
			_logger = logger;
		}

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
