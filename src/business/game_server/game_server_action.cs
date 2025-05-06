using System;
using core;
using business.entity;

namespace business.game_server
{
    public abstract class GameServerAction<T> : Action
    {
        public override bool Validate(ActionData action)
        {
            return true;
        }

        public override void Execute(ActionData action, ClientSession user)
        {
            Execute(action, (User)user, Cast(action));
        }

        protected T Cast(ActionData action)
        {
            return (T)Convert.ChangeType(action.Content, typeof(T));
        }

        public abstract void Execute(ActionData action, User user, T data);

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running GameServerAction using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling GameServerAction project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
