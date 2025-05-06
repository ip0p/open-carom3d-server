using core.server.carom3d;

namespace core
{
    public class Carom3DServer : Server
    {
        public Carom3DServer(ServerConfig config) : base(config)
        {
        }

        public override MessagingProtocol MessagingProtocol()
        {
            return new Carom3DProtocol();
        }
    }
}
