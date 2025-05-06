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

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running Carom3D server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling Carom3D server project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
