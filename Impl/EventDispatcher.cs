using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PoorMan.SSE.Interface;

namespace PoorMan.SSE.Impl {

    internal class EventDispatcher : IEventDispatcher
    {
        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();

        public IEnumerable<IClient> Clients => _clients.Select(item => item.Value);

        internal bool Subscribe(Client client) {
            return _clients.TryAdd(client.Id, client);
        }

        internal bool Unsubscribe(Client client) {
            return _clients.TryRemove(client.Id, out client);
        }
    }
}
