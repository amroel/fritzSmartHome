using FluentAssertions;
using FritzSmartHome.FritzBox.Security;
using Xunit;

namespace FritzSmartHome.FritzBox.Tests.Login
{
	public class ChallengeResponseHashCalculation
	{
		[Fact(Skip = "figure out how to test this. Hashes differ from documentation 'https://avm.de/fileadmin/user_upload/Global/Service/Schnittstellen/AVM%20Technical%20Note%20-%20Session%20ID_EN%20-%20Nov2020.pdf'")]
		public void Challenge_is_PBKDF2_calculates_PKDF2_hash_response()
		{
			// PBKDF2 is used if Challenge starts with 2$
			var sessionId = new SessionId { Challenge = "2$10000$5A1711$2000$5A1722" };
			var challengeResponse = sessionId.CalculateChallengeResponse(password: "1example!");

			challengeResponse.ToLower().Should().Be("5A1722$4e9d50a1cbf1ed3b389320e9364483f270183865ad9e1c7693108e23aa7a0655");
		}

		[Fact]
		public void Challenge_is_NOT_PBKDF2_should_fall_back_to_MD5_hash_response()
		{
			var sessionId = new SessionId { Challenge = "1234567z" };

			var challengeResponse = sessionId.CalculateChallengeResponse("äbc");

			challengeResponse.ToLower().Should().Be("1234567z-9e224a41eeefa284df7bb0f26c2913e2");
		}
	}
}
