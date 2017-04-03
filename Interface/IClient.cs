namespace PoorMan.SSE.Interface {

    public interface IClient {

        string Id { get; }

        void SendEvent(object data);

        void SendEvent(string evt, object data);
    }
}
