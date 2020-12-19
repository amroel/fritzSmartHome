using System;
using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace FritzSmartHome.FritzBox.Tests.Login
{
	public class ChallengeResponseHashCalculation
	{
		[Fact(Skip = "figure out how to test this. Hashes differ from documentation 'https://avm.de/fileadmin/user_upload/Global/Service/Schnittstellen/AVM%20Technical%20Note%20-%20Session%20ID_EN%20-%20Nov2020.pdf'")]
		public void Challenge_is_PBKDF2_calculates_PKDF2_hash_response()
		{
			var iter1 = "10000";
			var salt1 = "5A1711";
			var iter2 = "2000";
			var salt2 = "5A1722";

			// PBKDF2 is used if Challenge starts with 2$
			var sessionInfo = new SessionInfo { Challenge = $"2${iter1}${salt1}${iter2}${salt2}" };
			var challengeResponse = sessionInfo.CalculateChallengeResponse(password: "1example!");

			challengeResponse.ToLower().Should().Be("5A1722$4e9d50a1cbf1ed3b389320e9364483f270183865ad9e1c7693108e23aa7a0655");
		}

		[Fact]
		public void Challenge_is_NOT_PBKDF2_should_fall_back_to_MD5_hash_response()
		{
			var sessionInfo = new SessionInfo { Challenge = "1234567z" };
			var challengeResponse = sessionInfo.CalculateChallengeResponse("äbc");

			challengeResponse.ToLower().Should().Be("1234567z-9e224a41eeefa284df7bb0f26c2913e2");
		}
	}
}
