namespace FritzSmartHome.FritzBox
{
	public record BoxSettings
	{
		public string Url { get; init; }
		public string User { get; init; }
		public string Password { get; init; }
	}
}
