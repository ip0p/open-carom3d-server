using System;

namespace business
{
    public class Player
    {
        private Account _account;
        private uint _id;
        private string _playerName;
        private int _points;
        private int _level;

        public Player(Account account)
        {
            _account = account;
            _id = account.Id; // TODO: change in the future
            _playerName = account.Name;
            _points = 0;
            Random random = new Random();
            _level = 1 + random.Next(54);
        }

        public Account Account => _account;
        public uint Id => _id;
        public string Name => _playerName;
        public int Points => _points;
        public int Level => _level;
    }
}
