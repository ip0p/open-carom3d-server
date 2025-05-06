using System;

namespace business
{
    public class Account
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public Player Player { get; private set; }

        public Account(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
            Player = null;
        }

        public void SetPlayer(Player player)
        {
            Player = player;
        }
    }
}
