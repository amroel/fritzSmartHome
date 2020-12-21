using System.Threading;
using System.Threading.Tasks;

namespace FritzSmartHome.Domain.Commands
{
	public interface IHandleCommand<in TCommand, TResponse>
		where TCommand : ICommand<TResponse>
	{
		Task<TResponse> HandleAsync(TCommand request, CancellationToken cancellationToken = default);
	}
}
