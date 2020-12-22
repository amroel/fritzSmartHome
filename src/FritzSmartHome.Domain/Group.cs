using System.Xml.Serialization;

namespace FritzSmartHome.Domain
{
	[XmlType(TypeName = "group")]
	public record Group : Device
	{

	}
}