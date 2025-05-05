using System;
using System.Collections.Generic;
using core;

namespace core
{
    public class MessagingProtocol : IMessagingProtocol
    {
        private Carom3DMessageParser m_messageParser;
        private Dictionary<int, Action> m_userActionMap;

        public MessagingProtocol()
        {
            m_userActionMap = new Dictionary<int, Action>();
        }

        public void SetUserActionMap(Dictionary<int, Action> actionMap)
        {
            m_userActionMap = actionMap;
        }

        public ClientSession CreateSession(ntConnection ntClient, Server server)
        {
            return new Carom3DUserSession(ntClient, server);
        }

        public void OnMessageReceived(ClientSession session)
        {
            Carom3DUserSession user = (Carom3DUserSession)session;

            uint dataLen = user.PendingReadDataSize();
            ParsedDataResultInfo parsedData = m_messageParser.ParseMessageData(user.InDataCryptoCtx(), user.PendingReadData(), user.PendingReadDataSize());

            uint parsedLen = parsedData.ParsedTotalLen;
            user.DiscardReadPendingData(parsedLen);

            user.AppendActions(parsedData.ParsedActions);
            ProcessUserActions(user);
        }

        public void OnMessageSent(ClientSession session)
        {
            Carom3DUserSession user = (Carom3DUserSession)session;

            uint dataLen = user.PendingReadDataSize();
            ParsedDataResultInfo parsedData = m_messageParser.ParseMessageData(user.InDataCryptoCtx(), user.PendingReadData(), user.PendingReadDataSize());

            uint parsedLen = parsedData.ParsedTotalLen;
            user.DiscardReadPendingData(parsedLen);

            user.AppendActions(parsedData.ParsedActions);
            ProcessUserActions(user);
        }

        public void CloseSession(ClientSession session)
        {
            session.Dispose();
        }

        private void ProcessUserActions(Carom3DUserSession user)
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

        private void OnUserAction(Carom3DUserSession session, ActionData actionData)
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

        private void OnUnhandledUserAction(Carom3DUserSession session, ActionData actionData)
        {
            Console.WriteLine($"Unhandled action: {actionData.Id():X} - {actionData.Data().Count}");
        }
    }
}
