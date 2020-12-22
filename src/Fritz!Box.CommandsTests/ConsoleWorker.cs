using System.Threading;
using System.Threading.Tasks;
using FritzSmartHome.Domain;
using Microsoft.Extensions.Hosting;

namespace FritzBox.CommandsTests
{
	public class ConsoleWorker : BackgroundService
	{
		private readonly IFritzBox _fritzBox;

		public ConsoleWorker(IFritzBox fritzBox) => _fritzBox = fritzBox;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
			=> await _fritzBox.ListDevices(stoppingToken);
	}
}
