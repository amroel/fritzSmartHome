using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace FritzSmartHome.FritzBox
{
	public record SessionInfo
	{
		private const string PDKDF2_TOKEN = "$";
		private const string PDKDF2_START = "2" + PDKDF2_TOKEN;

		[XmlElement("SID")]
		public string SessionId { get; init; }

		[XmlElement("Challenge")]
		public string Challenge { get; init; }

		[XmlElement("BlockTime")]
		public int BlockTime { get; init; }

		public string CalculateChallengeResponse(string password)
		{
			if (IsPBKDF2())
				return CalculatePBKDF2Response(password);
			else
				return CalculateMD5Response(password);
		}

		private bool IsPBKDF2() => Challenge.StartsWith(PDKDF2_START);

		private string CalculatePBKDF2Response(string password)
		{
			var challengeParts = Challenge.Split(PDKDF2_TOKEN);

			// Extract all necessary values encoded into the challenge (ignoring challengeParts[0] = 2)			
			var iter1 = int.Parse(challengeParts[1]);
			var salt1 = Convert.FromHexString(challengeParts[2]);
			var iter2 = int.Parse(challengeParts[3]);
			var salt2 = Convert.FromHexString(challengeParts[4]);

			byte[] hash1, hash2 = null;
			// Hash twice, once with static salt...
			using (var pbkdf2 = new CustomRfc2898DeriveBytes(password, salt1, iter1))
			{
				hash1 = pbkdf2.GetBytes(32);
			}
			// and once with dynamic salt.
			using (var pbkdf2 = new CustomRfc2898DeriveBytes(Convert.ToHexString(hash1), salt2, iter2))
			{
				hash2 = pbkdf2.GetBytes(32);
			}

			return $"{challengeParts[4]}{PDKDF2_TOKEN}{Convert.ToHexString(hash2)}";
		}

		private string CalculateMD5Response(string password)
		{
			var response = $"{Challenge}-{password}";
			var result = string.Empty;
			using (var md5 = MD5.Create())
			{
				var responseBytes = md5.ComputeHash(Encoding.Unicode.GetBytes(response));
				result = Convert.ToHexString(responseBytes);
			}
			return $"{Challenge}-{result}";
		}
	}
}
