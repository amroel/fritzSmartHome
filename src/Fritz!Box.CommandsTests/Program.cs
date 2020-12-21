using System.Threading.Tasks;
using FritzSmartHome.Domain;
using FritzSmartHome.FritzBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FritzBox.CommandsTests
{
	class Program
	{
		static async Task Main(string[] args) => 
			await CreateHostBuilder(args)
				.RunConsoleAsync();

		private static IHostBuilder CreateHostBuilder(string[] args) => 
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<BoxSettings>(hostContext.Configuration.GetSection("LocalFritzBox"));
					services.AddHostedService<ConsoleWorker>();
					services.AddSingleton<IFritzBox, Box>();
				});
	}
}
