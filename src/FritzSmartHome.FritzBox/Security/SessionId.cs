using System.Xml.Serialization;

namespace FritzSmartHome.FritzBox.Security
{
	[XmlRoot("SessionInfo")]
	public record SessionId
	{
		private const string PDKDF2_START = "2$";

		[XmlElement("SID")]
		public string SID { get; init; }

		[XmlElement("Challenge")]
		public string Challenge { get; init; }

		[XmlElement("BlockTime")]
		public int BlockTime { get; init; }

		public bool IsBlocked() => BlockTime > 0;

		public ICreateChallengeResponse CreateChallengeResponder() => IsPBKDF2() ? 
			new PBKDF2ChallengeResponder(Challenge) : 
			new MD5ChallengeResponder(Challenge);

		public string CalculateChallengeResponse(string password) 
			=> CreateChallengeResponder().CreateResponse(password);

		private bool IsPBKDF2() => Challenge.StartsWith(PDKDF2_START);
	}
}
