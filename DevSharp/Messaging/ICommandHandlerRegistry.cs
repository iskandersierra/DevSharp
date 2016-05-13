using System.Threading;
using System.Threading.Tasks;

namespace DevSharp.Domain
{
    public interface ICommandHandlerRegistry
    {
        Task<FindCommandHandlerResult> FindCommandHandlerAsync(MessageDescription description, CancellationToken token = default(CancellationToken));
    }
}