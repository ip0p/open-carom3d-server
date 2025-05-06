namespace nettools
{
    public class NtEventHandler
    {
        public virtual void OnClientConnection(NtClient client) { }
        public virtual void OnClientDisconnection(NtClient client) { }
        public virtual void OnClientDataReceived(NtClient client, byte[] data, uint dataLen) { }
        public virtual void OnClientDataSent(NtClient client, byte[] data, uint dataLen) { }
    }
}
