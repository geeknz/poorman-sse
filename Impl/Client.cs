using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoorMan.SSE.Models;

namespace PoorMan.SSE.Impl {

    internal class Client : Interface.IClient
    {
        private readonly string _id;

        private readonly BlockingCollection<ServerSentEvent> _queue;

        internal Client(string id) {
            _id = id;
            _queue = new BlockingCollection<ServerSentEvent>();
        }

        public string Id => _id;

        internal BlockingCollection<ServerSentEvent> Queue => _queue;

        public void SendEvent(object data)
        {
            SendEvent(null, data);
        }

        public void SendEvent(string evt, object obj)
        {
            // Serialize the data
            var data = JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            // Add it to the event queue
            _queue.Add(new ServerSentEvent {
                Event = evt,
                Data = data
            });
        }
    }
}
