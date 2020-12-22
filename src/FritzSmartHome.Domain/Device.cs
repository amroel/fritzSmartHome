using System.Xml.Serialization;

namespace FritzSmartHome.Domain
{
	[XmlType(TypeName = "device")]
	[XmlInclude(typeof(Group))]
	public record Device
	{
		[XmlAttribute("identifier")]
		public string Identifier { get; init; }

		[XmlAttribute("id")]
		public string Id { get; init; }

		[XmlAttribute("fwversion")]
		public string FWVersion { get; init; }

		[XmlAttribute("manufacturer")]
		public string Manufacturer { get; init; }

		[XmlAttribute("productname")]
		public string ProductName { get; init; }

		[XmlAttribute("functionbitmask")]
		public int FunctionBitMask { get; init; }

		[XmlElement("present")]
		public int Present { get; init; }

		[XmlIgnore]
		public bool IsPresent => Present == 1;
	}
}