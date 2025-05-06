using System;

namespace business.entity
{
    public class User
    {
        private Account account;
        private Player player;
        private UserSpot spot;
        private string lastChannelName;
        private int[] coverStates;

        public User()
        {
            coverStates = new int[3];
            coverStates[0] = 1;
            coverStates[1] = 1;
            coverStates[2] = 1;
        }

        public void SetAccount(Account account)
        {
            this.account = account;
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public void SetSpot(UserSpot spot)
        {
            if (spot != null && spot.IsOfType(0))
            {
                lastChannelName = spot.Name;
            }
            this.spot = spot;
        }

        public void SetCoverStates(int[] coverStates)
        {
            this.coverStates[0] = coverStates[0];
            this.coverStates[1] = coverStates[1];
            this.coverStates[2] = coverStates[2];
        }

        public GameServer Server { get; set; }
        public Account Account => account;
        public Player Player => player;
        public UserSpot Spot => spot;
        public string LastChannelName => lastChannelName;

        public bool InChannel => spot != null && spot.IsOfType(0);
        public bool InRoom => spot != null && spot.IsOfType(1);

        public Channel ChannelIn => InChannel ? (Channel)spot : null;
        public Room RoomIn => InRoom ? (Room)spot : null;

        public bool InCover(uint state) => coverStates[state] == 0;
    }
}
