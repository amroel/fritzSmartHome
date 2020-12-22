using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace FritzSmartHome.Domain
{
	[XmlRoot("devicelist")]
	public class DeviceList : List<Device>
	{
		public override string ToString()
		{
			if (Count == 0)
				return base.ToString();

			return string.Join(", ", this.Select(x => x.ToString()));
		}
	}
}
