using core.server.carom3d;
using business.entity;
using business.util;
using business.service;

namespace business
{
    public class ChannelService
    {
        private static ChannelService g_lobbyService = new ChannelService();
        private List<Channel> m_channels = new List<Channel>();

        public static ChannelService GetInstance()
        {
            return g_lobbyService;
        }

        public Channel CreateChannel(string channelName)
        {
            Channel channel = GetChannel(channelName);
            if (channel != null)
                return channel;
            channel = new Channel(channelName);
            m_channels.Add(channel);
            return channel;
        }

        public Channel GetChannel(string channelName)
        {
            foreach (var channel in m_channels)
            {
                if (channel.Name == channelName)
                    return channel;
            }
            return null;
        }

        public Channel MoveUserToChannel(User user, string channelName, bool createIfNotExists)
        {
            UserService.GetInstance().RemoveUserFromCurrentSpot(user);

            Channel channel = GetChannel(channelName);
            if (channel == null)
            {
                if (!createIfNotExists)
                    return null;
                channel = CreateChannel(channelName);
            }
            InsertUserIntoChannel(channel, user);
            return channel;
        }

        public void InsertUserIntoChannel(Channel channel, User user)
        {
            //TODO: check if channel is not full
            channel.InsertUser(user);
            user.SetSpot(channel);

            //TODO: modularize
            ChannelPlayer channelPlayer = new ChannelPlayer();
            ActionData joinChannelAction = new ActionData(0x1D, System.Text.Encoding.Unicode.GetBytes(channel.Name));
            user.SendAction(joinChannelAction);

            channelPlayer.AccountNumber = user.Player.Id;
            channelPlayer.PlayerName = user.Player.Name;
            channelPlayer.Level = user.Player.Level;

            foreach (var userIn in channel.UsersIn)
            {
                ChannelPlayer cp = new ChannelPlayer();
                cp.AccountNumber = userIn.Player.Id;
                cp.Level = userIn.Player.Level;
                cp.PlayerName = userIn.Player.Name;
                ActionData channelPlayerAction = new ActionData(0x1E, cp.ToByteArray());
                user.SendAction(channelPlayerAction);
            }

            foreach (var userIn in channel.UsersIn)
            {
                if (userIn == user) continue;
                ActionData channelUserAction = new ActionData(0x1E, channelPlayer.ToByteArray());
                userIn.SendAction(channelUserAction);
            }
        }

        public void RemoveUserFromChannel(Channel channel, User user)
        {
            channel.RemoveUser(user);
            user.SetSpot(null);

            //TODO: modularize
            uint accountNumber = user.Account.Id;
            ActionData userLeftChannelAction = new ActionData(0x1F, BitConverter.GetBytes(accountNumber));
            ActionDispatcher.Prepare().Action(userLeftChannelAction).Send(new ChannelDestination(channel));
        }

        public void SendUserMessage(Channel channel, User user, string message)
        {
            byte[] data = new byte[(message.Length + 1) * 2 + 4];
            BitConverter.GetBytes(user.Account.Id).CopyTo(data, 0);
            System.Text.Encoding.Unicode.GetBytes(message).CopyTo(data, 4);

            ActionData actionData = new ActionData(0x21, data);
            ActionDispatcher.Prepare().Action(actionData).Send(new ChannelDestination(channel));
        }
    }
}
