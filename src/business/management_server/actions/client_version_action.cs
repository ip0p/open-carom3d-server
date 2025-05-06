using System;
using business.entity;

namespace business.management_server.actions
{
    public class ClientVersionAction : ManagementServerAction<string>
    {
        public override bool Validate(ActionData action)
        {
            // TODO: validate version
            return true;
        }

        public override void Execute(ActionData action, User user, string data)
        {
            Console.WriteLine(data);
        }

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running client version action server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling client version action project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
