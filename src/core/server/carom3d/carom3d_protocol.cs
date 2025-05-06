using System;
using System.Collections.Generic;
using core.server.carom3d;

namespace core
{
    public class Carom3DProtocol : MessagingProtocol
    {
        protected Carom3DMessageParser m_messageParser;
        protected Dictionary<int, Action> m_userActionMap;

        public Carom3DProtocol()
        {
            m_userActionMap = null;
        }

        public override ClientSession CreateSession(nettools.ntConnection ntClient, Server server)
        {
            return new Carom3DUserSession(ntClient, server);
        }

        public override void OnMessageReceived(ClientSession session)
        {
            Carom3DUserSession user = (Carom3DUserSession)session;

            uint dataLen = user.PendingReadDataSize();
            ParsedDataResultInfo parsedData = m_messageParser.ParseMessageData(user.InDataCryptoCtx(), user.PendingReadData(), user.PendingReadDataSize());

            uint parsedLen = parsedData.ParsedTotalLen;
            user.DiscardReadPendingData(parsedLen);

            user.AppendActions(parsedData.ParsedActions);
            ProcessUserActions(user);
        }

        public override void OnMessageSent(ClientSession session)
        {
            Carom3DUserSession user = (Carom3DUserSession)session;

            uint dataLen = user.PendingReadDataSize();
            ParsedDataResultInfo parsedData = m_messageParser.ParseMessageData(user.InDataCryptoCtx(), user.PendingReadData(), user.PendingReadDataSize());

            uint parsedLen = parsedData.ParsedTotalLen;
            user.DiscardReadPendingData(parsedLen);

            user.AppendActions(parsedData.ParsedActions);
            ProcessUserActions(user);
        }

        public override void CloseSession(ClientSession session)
        {
            // In C#, the garbage collector will handle memory management, so no need to delete the session.
        }

        public void SetUserActionMap(Dictionary<int, Action> actionMap)
        {
            m_userActionMap = actionMap;
        }

        protected void ProcessUserActions(Carom3DUserSession user)
        {
            if (m_userActionMap == null)
                return;

            List<ActionData> actions = user.PendingActions();
            if (actions.Count == 0)
                return;

            foreach (ActionData actionData in actions)
            {
                OnUserAction(user, actionData);
            }
            user.ClearPendingActions();
        }

        protected void OnUserAction(Carom3DUserSession session, ActionData actionData)
        {
            if (m_userActionMap.TryGetValue(actionData.Id(), out Action action))
            {
                if (action.Validate(actionData))
                {
                    action.Execute(actionData, session);
                }
            }
            else
            {
                OnUnhandledUserAction(session, actionData);
            }
        }

        protected void OnUnhandledUserAction(Carom3DUserSession session, ActionData actionData)
        {
            // TODO: Invalid Action
            int a = actionData.Id();
            Console.WriteLine($"Unhandled action: {a:X} - {actionData.Data().Count}");
        }
    }
}
