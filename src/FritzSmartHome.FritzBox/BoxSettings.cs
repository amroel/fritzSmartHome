namespace FritzSmartHome.FritzBox
{
	public record BoxSettings
	{
		public string StartUrl { get; init; }
		public string CommandsUrl { get; init; }
		public string User { get; init; }
		public string Password { get; init; }
	}
}
