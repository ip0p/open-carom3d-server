namespace nettools
{
    public class NtEventHandler
    {
        public virtual void OnClientConnection(NtClient client) { }
        public virtual void OnClientDisconnection(NtClient client) { }
        public virtual void OnClientDataReceived(NtClient client, byte[] data, uint dataLen) { }
        public virtual void OnClientDataSent(NtClient client, byte[] data, uint dataLen) { }

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
