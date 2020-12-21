using System.Threading;
using System.Threading.Tasks;

namespace FritzSmartHome.Domain.Commands.DeviceListing
{
	public class ListDevicesHandler : IHandleCommand<ListDevices, DeviceList>
	{
		private readonly IFritzBox _box;

		public ListDevicesHandler(IFritzBox box) => _box = box;

		public Task<DeviceList> HandleAsync(ListDevices request, CancellationToken cancellationToken = default) =>
			_box.ListDevices(cancellationToken);
	}
}
