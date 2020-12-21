using System.Threading.Tasks;
using FluentAssertions;
using FritzSmartHome.Domain;
using FritzSmartHome.Domain.Commands.DeviceListing;
using NSubstitute;
using Xunit;

namespace FritzSmartHome.FritzBox.Tests.Commands
{
	public class ListingDevices
	{
		[Fact]
		public async Task Should_return_device_list()
		{
			var command = new ListDevices();
			var fritzBox = Substitute.For<IFritzBox>();
			fritzBox.ListDevices().Returns(new DeviceList());

			var commandHandler = new ListDevicesHandler(fritzBox);
			var response = await commandHandler.HandleAsync(command);

			response.Should().NotBeNull();
			response.Should().BeOfType<DeviceList>();
		}
	}
}
