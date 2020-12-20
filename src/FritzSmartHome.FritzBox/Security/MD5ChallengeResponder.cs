using System;
using System.Security.Cryptography;
using System.Text;

namespace FritzSmartHome.FritzBox.Security
{
	public class MD5ChallengeResponder : ICreateChallengeResponse
	{
		private readonly string _challenge;

		public MD5ChallengeResponder(string challenge) => _challenge = challenge;

		public string CreateResponse(string password)
		{
			var response = $"{_challenge}-{password}";
			var result = string.Empty;
			using (var md5 = MD5.Create())
			{
				var responseBytes = md5.ComputeHash(Encoding.Unicode.GetBytes(response));
				result = Convert.ToHexString(responseBytes).ToLower();
			}
			return $"{_challenge}-{result}";
		}
	}
}
