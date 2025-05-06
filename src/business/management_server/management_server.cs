using System;
using System.Collections.Generic;
using business.entity;
using core;
using core.server.carom3d;

namespace business.management_server
{
    public class ManagementServer : Carom3DServer
    {
        public ManagementServer(ServerConfig config) : base(config)
        {
        }

        public override MessagingProtocol MessagingProtocol()
        {
            return new ManagementServerProtocol();
        }

        public override void OnClientConnection(ClientSession client)
        {
            base.OnClientConnection(client);

            string version = "5.31";
            ActionData versionAction = new ActionData(0x00, System.Text.Encoding.ASCII.GetBytes(version));
            ((User)client).SendAction(versionAction);
        }

        public override void OnClientDisconnection(ClientSession client)
        {
            // TODO: Handle client disconnection
        }
    }

    public class ManagementServerProtocol : Carom3DProtocol
    {
        private static readonly Dictionary<int, Action> actions = new Dictionary<int, Action>
        {
            { 0x00, new ServerListRequestAction() },
            { 0x01, new LoginAction() },
            { 0x65, new ClientVersionAction() }
        };

        public ManagementServerProtocol()
        {
            SetUserActionMap(actions);
        }

        public override ClientSession CreateSession(ntConnection ntClient, Server server)
        {
            return new User(ntClient, server);
        }

        public override void OnUnhandledUserAction(Carom3DUserSession session, ActionData actionData)
        {
            User user = (User)session;
            if (user.Player() == null)
            {
                base.OnUnhandledUserAction(session, actionData);
            }
            else
            {
                Console.WriteLine($"Unhandled client action: {user.Player().Name()} - {actionData.Id()} - {actionData.Data().Count}");
            }
        }
    }
}
