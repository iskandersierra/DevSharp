using System.Threading;
using System.Threading.Tasks;

namespace DevSharp.Domain
{
    public interface ICommandHandler
    {
        Task HandleCommandAsync(HandleCommandContext context, CancellationToken token = default(CancellationToken));
    }
}