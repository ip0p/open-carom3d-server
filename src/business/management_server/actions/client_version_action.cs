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
    }
}
