using System.Threading;
using System.Threading.Tasks;
using FritzSmartHome.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FritzBox.CommandsTests
{
	public class ConsoleWorker : BackgroundService
	{
		private readonly ILogger _logger;
		private readonly IHostApplicationLifetime _appLifetime;
		private readonly IFritzBox _fritzBox;

		public ConsoleWorker(ILogger<ConsoleWorker> logger, IHostApplicationLifetime appLifetime, IFritzBox fritzBox)
		{
			_logger = logger;
			_appLifetime = appLifetime;
			_fritzBox = fritzBox;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _fritzBox.LoginAsync(stoppingToken);
		}
	}
}
