using System.Xml.Serialization;

namespace FritzSmartHome.FritzBox.Security
{
	[XmlRoot("SessionInfo")]
	public record SessionId
	{
		private const string PDKDF2_START = "2$";

		public string SID { get; init; }

		public string Challenge { get; init; }

		public int BlockTime { get; init; }

		public bool IsBlocked() => BlockTime > 0;

		public string CalculateChallengeResponse(string password) 
			=> CreateChallengeResponder().CreateResponse(password);

		private ICreateChallengeResponse CreateChallengeResponder() => IsPBKDF2() ?
			new PBKDF2ChallengeResponder(Challenge) :
			new MD5ChallengeResponder(Challenge);

		private bool IsPBKDF2() => Challenge.StartsWith(PDKDF2_START);
	}
}
