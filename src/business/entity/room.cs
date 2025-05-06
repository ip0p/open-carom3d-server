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

        private GameServer server;
        private uint id;
        private string title;
        private string password;
        private List<RoomUser> roomUsers;
        private SlotInfos slots;
        private int usersIn;
        private int maxUsers;
        private GameInfo gameInfo;
        private int straightWins;
        private RoomState state;
        private bool closed;
        private bool hidden;
        private Match matchInfo;
        private int playingUserCount;
        private int inGameScreenUserCount;
        private User creator;
        private int roomMasterListIndex;
        private List<int> userQueue;
        private List<User> users;

        public Room(GameServer server, uint id, string title, User creator, int maxPlayers, GameInfo gameInfo)
        {
            this.server = server;
            this.id = id;
            this.title = title;
            this.creator = creator;
            this.maxUsers = maxPlayers;
            this.gameInfo = gameInfo;
            this.closed = false;
            this.hidden = false;
            this.matchInfo = null;
            this.straightWins = 0;
            this.usersIn = 0;
            this.state = RoomState.IDLE;
            this.playingUserCount = 0;
            this.inGameScreenUserCount = 0;
            this.roomUsers = new List<RoomUser>(maxPlayers);
            this.slots = new SlotInfos
            {
                state = new SlotState[30],
                playerListIndex = new int[30]
            };
            this.userQueue = new List<int>(maxPlayers);
            this.users = new List<User>(maxPlayers);

            for (int i = 0; i < 30; i++)
            {
                this.slots.state[i] = SlotState.DISABLED;
                this.slots.playerListIndex[i] = -1;
            }
        }

        public override bool IsOfType(int type)
        {
            return type == 1;
        }

        public override string Description => "Room";

        public override string Name => title;

        public override int InsertUser(User user)
        {
            if (usersIn >= maxUsers)
                return -1;

            int userIndex = GetFreeListIndex();
            roomUsers[userIndex] = new RoomUser { user = user, slot = -1, inGameScreen = state == RoomState.IN_GAME, playing = false };
            usersIn++;
            userQueue.Add(userIndex);
            users.Add(user);

            if (state == RoomState.IN_GAME)
                inGameScreenUserCount++;

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

            if (roomMasterListIndex == userListIndex)
                roomMasterListIndex = -1;

            RoomUser roomUser = roomUsers[userListIndex];
            User user = roomUser.user;
            if (state == RoomState.IN_GAME)
            {
                if (roomUser.playing)
                {
                    roomUser.playing = false;
                    playingUserCount--;
                }
                roomUser.inGameScreen = false;
                inGameScreenUserCount--;
            }

            if (roomUser.slot >= 0)
                SetUserToSlot(-1, roomUser.slot);
            roomUser.slot = -1;
            roomUser.user = null;
            usersIn--;

            userQueue.Remove(userListIndex);
            users.Remove(user);

            UpdateRoomMaster();
        }

        public bool IsUserIn(string userName)
        {
            return users.Any(user => user != null && user.Player.Name == userName);
        }

        public int UserCount => usersIn;

        public int UpdateRoomMaster()
        {
            if (roomMasterListIndex != -1)
                return roomMasterListIndex;

            int next = GetNextRoomMaster();
            if (next != -1)
                roomMasterListIndex = next;
            return next;
        }

        public int SetUserToSlot(int listIndex, int slot)
        {
            if (listIndex == -1)
            {
                int userListIndex = slots.playerListIndex[slot];
                if (userListIndex != -1)
                    roomUsers[userListIndex].slot = -1;
                slots.playerListIndex[slot] = -1;
                return 0;
            }

            if (slots.playerListIndex[slot] != -1)
                return 1;
            if (slots.state[slot] == SlotState.DISABLED)
                return 2;

            int userSlot = roomUsers[listIndex].slot;
            if (userSlot != -1)
                slots.playerListIndex[userSlot] = -1;
            roomUsers[listIndex].slot = slot;
            slots.playerListIndex[slot] = listIndex;
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
            if (slots.state[slot] == state)
                return 0;
            slots.playerListIndex[slot] = -1;
            slots.state[slot] = state;
            return 0;
        }

        private int GetFreeListIndex()
        {
            for (int i = 0; i < roomUsers.Count; i++)
            {
                if (roomUsers[i].user == null)
                    return i;
            }
            return -1;
        }

        private int GetUserListIndex(User user)
        {
            for (int i = 0; i < roomUsers.Count; i++)
            {
                if (roomUsers[i].user == user)
                    return i;
            }
            return -1;
        }

        private int GetNextRoomMaster()
        {
            return userQueue.Count == 0 ? -1 : userQueue[0];
        }

        public int GetUserSlot(User user)
        {
            int listIndex = GetUserListIndex(user);
            return listIndex >= 0 ? roomUsers[listIndex].slot : -1;
        }

        public User UserInListIndex(int listIndex)
        {
            return roomUsers[listIndex].user;
        }

        public uint StartMatch()
        {
            if (state != RoomState.IDLE)
                return 1;

            matchInfo = new Match(0, this, 0); // TODO: implement
            state = RoomState.IN_GAME;

            playingUserCount = 0;

            foreach (var user in roomUsers)
            {
                if (user.user != null)
                {
                    user.inGameScreen = true;
                    user.playing = user.slot != -1;
                    if (user.playing)
                        playingUserCount++;
                }
            }

            inGameScreenUserCount = userQueue.Count;
            return 0;
        }

        public uint EndMatch()
        {
            if (state != RoomState.IN_GAME)
                return 1;

            foreach (var roomUser in roomUsers)
            {
                if (roomUser.user != null)
                {
                    roomUser.playing = false;
                }
            }
            playingUserCount = 0;
            state = RoomState.IDLE;
            return 0;
        }

        public void SetUserFinishedPlaying(User user)
        {
            int listIndex = GetUserListIndex(user);
            if (listIndex == -1)
                return;
            roomUsers[listIndex].playing = false;
            playingUserCount--;
        }

        public void SetUserOutOfGameScreen(User user)
        {
            int listIndex = GetUserListIndex(user);
            if (listIndex == -1)
                return;
            roomUsers[listIndex].inGameScreen = false;
            inGameScreenUserCount--;
        }

        public int ResetUserFromSlot(int userListIndex)
        {
            int slot = roomUsers[userListIndex].slot;
            if (slot != -1)
                slots.playerListIndex[slot] = -1;
            roomUsers[userListIndex].slot = -1;
            return 0;
        }

        public void ResetUserFromSlots()
        {
            for (int i = 0; i < maxUsers; i++)
                ResetUserFromSlot(i);
        }

        public void SetTitle(string title)
        {
            this.title = title;
        }

        public void SetPassword(string password)
        {
            this.password = password;
        }

        public void SetState(RoomState state)
        {
            this.state = state;
        }

        public void SetClosed(bool closed)
        {
            this.closed = closed;
        }

        public void SetHidden(bool hidden)
        {
            this.hidden = hidden;
        }

        public GameServer Server => server;
        public uint Id => id;
        public string Password => password;
        public User RoomMaster => roomMasterListIndex != -1 ? roomUsers[roomMasterListIndex].user : null;
        public int RoomMasterListIndex => roomMasterListIndex;
        public int UsersInCount => usersIn;
        public int MaxPlayers => maxUsers;
        public GameInfo GameInfo => gameInfo;
        public List<int> UserQueue => userQueue;
        public List<User> Users => users;
        public int StraightWins => straightWins;
        public RoomState State => state;
        public SlotInfos SlotInfos => slots;
        public bool Closed => closed;
        public bool Hidden => hidden;
        public bool InGame => state == RoomState.IN_GAME;
        public int PlayingUserCount => playingUserCount;
        public int InGameScreenUserCount => inGameScreenUserCount;
    }
}
