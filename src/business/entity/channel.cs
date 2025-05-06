using System;
using System.Collections.Generic;
using System.Linq;

namespace business.entity
{
    public class Channel : UserSpot
    {
        private string _channelName;
        private List<User> _users;

        public Channel(string channelName)
        {
            _channelName = channelName;
            _users = new List<User>();
        }

        public override bool IsOfType(int type)
        {
            return type == 0;
        }

        public override string Description => "Channel";

        public override string Name => _channelName;

        public override int InsertUser(User user)
        {
            _users.Add(user);
            return _users.Count - 1;
        }

        public override void RemoveUser(User user)
        {
            _users.Remove(user);
        }

        public override bool IsUserIn(string userName)
        {
            return _users.Any(user => user.Player.Name == userName);
        }

        public override int UserCount => _users.Count;

        public List<User> UsersIn => _users;
    }
}
