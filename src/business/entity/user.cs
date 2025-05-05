using System;

namespace business.entity
{
    public class User
    {
        private Account _account;
        private Player _player;
        private UserSpot _spot;
        private string _lastChannelName;
        private int[] _coverStates;

        public User()
        {
            _coverStates = new int[3];
            _coverStates[0] = 1;
            _coverStates[1] = 1;
            _coverStates[2] = 1;
        }

        public void SetAccount(Account account)
        {
            _account = account;
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }

        public void SetSpot(UserSpot spot)
        {
            if (spot != null && spot.IsOfType(0))
            {
                _lastChannelName = spot.Name;
            }
            _spot = spot;
        }

        public void SetCoverStates(int[] coverStates)
        {
            _coverStates[0] = coverStates[0];
            _coverStates[1] = coverStates[1];
            _coverStates[2] = coverStates[2];
        }

        public GameServer Server { get; set; }
        public Account Account => _account;
        public Player Player => _player;
        public UserSpot Spot => _spot;
        public string LastChannelName => _lastChannelName;

        public bool InChannel => _spot != null && _spot.IsOfType(0);
        public bool InRoom => _spot != null && _spot.IsOfType(1);

        public Channel ChannelIn => InChannel ? (Channel)_spot : null;
        public Room RoomIn => InRoom ? (Room)_spot : null;

        public bool InCover(uint state) => _coverStates[state] == 0;
    }
}
