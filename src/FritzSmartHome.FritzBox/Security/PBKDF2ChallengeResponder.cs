using System;

namespace FritzSmartHome.FritzBox.Security
{
	public class PBKDF2ChallengeResponder : ICreateChallengeResponse
	{
		private const string PDKDF2_TOKEN = "$";
		private readonly string _challenge;

		public PBKDF2ChallengeResponder(string challenge) => _challenge = challenge;

		public string CreateResponse(string password)
		{
			var challengeParts = _challenge.Split(PDKDF2_TOKEN);

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
	}
}
