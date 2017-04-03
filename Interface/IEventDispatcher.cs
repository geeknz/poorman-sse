using System.Collections.Generic;

namespace PoorMan.SSE.Interface {

    public interface IEventDispatcher
    {
        IEnumerable<IClient> Clients { get; }
    }
}
