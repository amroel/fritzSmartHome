using System;
using System.Threading;
using System.Threading.Tasks;

namespace FritzSmartHome.Domain
{
	public interface IFritzBox
	{
		Task LoginAsync(CancellationToken cancellationToken = default);
		Task<DeviceList> ListDevices(CancellationToken cancellationToken = default);
	}
}
