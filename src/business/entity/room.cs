using System;
using System.Collections.Generic;
using System.Linq;

namespace business.entity
{
    public class Room : UserSpot
    {
        public enum SlotState
        {
            DISABLED = 0,
            OPEN,
            CLOSED
        }

        public enum RoomState
        {
            IDLE = 1,
            IN_GAME = 2
        }

        public struct SlotInfos
        {
            public SlotState[] state;
            public int[] playerListIndex;
        }

        public struct RoomUser
        {
            public User user;
            public int slot;
            public bool inGameScreen;
            public bool playing;
        }

        public struct GameInfo
        {
            public int roomType;
            public int gameType;
            public int difficulty;
            public int matchType;
            public int caneys;
        }

        private GameServer _server;
        private uint _id;
        private string _title;
        private string _password;
        private List<RoomUser> _roomUsers;
        private SlotInfos _slots;
        private int _usersIn;
        private int _maxUsers;
        private GameInfo _gameInfo;
        private int _straightWins;
        private RoomState _state;
        private bool _closed;
        private bool _hidden;
        private Match _matchInfo;
        private int _playingUserCount;
        private int _inGameScreenUserCount;
        private User _creator;
        private int _roomMasterListIndex;
        private List<int> _userQueue;
        private List<User> _users;

        public Room(GameServer server, uint id, string title, User creator, int maxPlayers, GameInfo gameInfo)
        {
            _server = server;
            _id = id;
            _title = title;
            _creator = creator;
            _maxUsers = maxPlayers;
            _gameInfo = gameInfo;
            _closed = false;
            _hidden = false;
            _matchInfo = null;
            _straightWins = 0;
            _usersIn = 0;
            _state = RoomState.IDLE;
            _playingUserCount = 0;
            _inGameScreenUserCount = 0;
            _roomUsers = new List<RoomUser>(maxPlayers);
            _slots = new SlotInfos
            {
                state = new SlotState[30],
                playerListIndex = new int[30]
            };
            _userQueue = new List<int>(maxPlayers);
            _users = new List<User>(maxPlayers);

            for (int i = 0; i < 30; i++)
            {
                _slots.state[i] = SlotState.DISABLED;
                _slots.playerListIndex[i] = -1;
            }
        }

        public override bool IsOfType(int type)
        {
            return type == 1;
        }

        public override string Description => "Room";

        public override string Name => _title;

        public override int InsertUser(User user)
        {
            if (_usersIn >= _maxUsers)
                return -1;

            int userIndex = GetFreeListIndex();
            _roomUsers[userIndex] = new RoomUser { user = user, slot = -1, inGameScreen = _state == RoomState.IN_GAME, playing = false };
            _usersIn++;
            _userQueue.Add(userIndex);
            _users.Add(user);

            if (_state == RoomState.IN_GAME)
                _inGameScreenUserCount++;

            UpdateRoomMaster();
            return userIndex;
        }

        public override void RemoveUser(User user)
        {
            int userIndex = GetUserListIndex(user);
            RemoveUser(userIndex);
        }

        public void RemoveUser(int userListIndex)
        {
            if (userListIndex == -1)
                return;

            if (_roomMasterListIndex == userListIndex)
                _roomMasterListIndex = -1;

            RoomUser roomUser = _roomUsers[userListIndex];
            User user = roomUser.user;
            if (_state == RoomState.IN_GAME)
            {
                if (roomUser.playing)
                {
                    roomUser.playing = false;
                    _playingUserCount--;
                }
                roomUser.inGameScreen = false;
                _inGameScreenUserCount--;
            }

            if (roomUser.slot >= 0)
                SetUserToSlot(-1, roomUser.slot);
            roomUser.slot = -1;
            roomUser.user = null;
            _usersIn--;

            _userQueue.Remove(userListIndex);
            _users.Remove(user);

            UpdateRoomMaster();
        }

        public bool IsUserIn(string userName)
        {
            return _users.Any(user => user != null && user.Player.Name == userName);
        }

        public int UserCount => _usersIn;

        public int UpdateRoomMaster()
        {
            if (_roomMasterListIndex != -1)
                return _roomMasterListIndex;

            int next = GetNextRoomMaster();
            if (next != -1)
                _roomMasterListIndex = next;
            return next;
        }

        public int SetUserToSlot(int listIndex, int slot)
        {
            if (listIndex == -1)
            {
                int userListIndex = _slots.playerListIndex[slot];
                if (userListIndex != -1)
                    _roomUsers[userListIndex].slot = -1;
                _slots.playerListIndex[slot] = -1;
                return 0;
            }

            if (_slots.playerListIndex[slot] != -1)
                return 1;
            if (_slots.state[slot] == SlotState.DISABLED)
                return 2;

            int userSlot = _roomUsers[listIndex].slot;
            if (userSlot != -1)
                _slots.playerListIndex[userSlot] = -1;
            _roomUsers[listIndex].slot = slot;
            _slots.playerListIndex[slot] = listIndex;
            return 0;
        }

        public int SetUserToSlot(User user, int slot)
        {
            int listIndex = GetUserListIndex(user);
            if (listIndex >= 0)
                return SetUserToSlot(listIndex, slot);
            return 1;
        }

        public int SetSlotState(int slot, SlotState state)
        {
            if (_slots.state[slot] == state)
                return 0;
            _slots.playerListIndex[slot] = -1;
            _slots.state[slot] = state;
            return 0;
        }

        private int GetFreeListIndex()
        {
            for (int i = 0; i < _roomUsers.Count; i++)
            {
                if (_roomUsers[i].user == null)
                    return i;
            }
            return -1;
        }

        private int GetUserListIndex(User user)
        {
            for (int i = 0; i < _roomUsers.Count; i++)
            {
                if (_roomUsers[i].user == user)
                    return i;
            }
            return -1;
        }

        private int GetNextRoomMaster()
        {
            return _userQueue.Count == 0 ? -1 : _userQueue[0];
        }
    }
}
