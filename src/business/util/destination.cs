using System.Collections.Generic;
using core;

namespace business
{
    public class ServerDestination : Destination
    {
        private readonly GameServer _server;
        private readonly int _to;

        public ServerDestination(GameServer server, int to = 0)
        {
            _server = server;
            _to = to;
        }

        public override void Send(List<ActionData> actions)
        {
            var clients = _server.Clients();
            foreach (var client in clients)
            {
                var user = (User)client.Value;
                int to = _to;
                if (to != 0)
                {
                    var spot = user.Spot;
                    if (spot == null)
                        continue;
                    if ((to & 1) != 0 && !spot.IsOfType(0))
                        continue;
                    if ((to & 2) != 0 && !spot.IsOfType(1))
                        continue;
                }

                foreach (var action in actions)
                    user.SendAction(action);
            }
        }
    }

    public class ChannelDestination : Destination
    {
        private readonly Channel _channel;

        public ChannelDestination(Channel channel)
        {
            _channel = channel;
        }

        public override void Send(List<ActionData> actions)
        {
            foreach (var user in _channel.UsersIn())
            {
                foreach (var action in actions)
                    user.SendAction(action);
            }
        }
    }

    public class RoomDestination : Destination
    {
        private readonly Room _room;

        public RoomDestination(Room room)
        {
            _room = room;
        }

        public override void Send(List<ActionData> actions)
        {
            foreach (var user in _room.Users())
            {
                foreach (var action in actions)
                    user.SendAction(action);
            }
        }
    }

    public class UserDestination : Destination
    {
        private readonly User _user;

        public UserDestination(User user)
        {
            _user = user;
        }

        public override void Send(List<ActionData> actions)
        {
            foreach (var action in actions)
                _user.SendAction(action);
        }
    }
}
