using System.Threading.Tasks;

namespace DevSharp.Messaging
{
    public interface IEventStreamWriter
    {
        Task StoreEventsAsync(string identifier, UncommittedAggregateEvents events, object snapshot = null);
    }
}