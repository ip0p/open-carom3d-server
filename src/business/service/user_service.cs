using System;
using System.Collections.Generic;
using System.Threading;
using business.api;
using business.entity;
using business.game_server;
using business.util;
using core.server;

namespace business
{
    public class UserService
    {
        private static UserService g_userService = new UserService();
        private Dictionary<int, User> m_clientsUsers = new Dictionary<int, User>();
        private Dictionary<Account, User> m_usersAccounts = new Dictionary<Account, User>();

        public static UserService GetInstance()
        {
            return g_userService;
        }

        public User GetUser(ClientSession clientSession)
        {
            return m_clientsUsers[clientSession.SessionId];
        }

        public User FindUser(string userName)
        {
            Account account = AccountService.GetInstance().FindAccount(userName);
            if (account == null)
                return null;

            return m_usersAccounts[account];
        }

        private void SendClientVersion(User user)
        {
            string version = "5.31";
            ActionData action = new ActionData(0x16, System.Text.Encoding.ASCII.GetBytes(version), version.Length);
            user.SendAction(action);
        }

        private void SendLoginResult(User user)
        {
            LoginResult loginResult = new LoginResult
            {
                result = 0,
                playerName = "",
                accountNumber = (uint)user.Account.Id,
                accountLevel = 100
            };
            loginResult.playerName = user.Player.Name.Substring(0, Math.Min(user.Player.Name.Length, 20));

            ActionData loginResultAction = new ActionData(0x18, System.Text.Encoding.Unicode.GetBytes(loginResult.playerName), loginResult.playerName.Length * 2);
            user.SendAction(loginResultAction);
        }

        private void SendBlackBoardMessage(User user, string message)
        {
            ActionData blackBoardMessageAction = new ActionData(0x56, System.Text.Encoding.Unicode.GetBytes(message), (message.Length + 1) * 2);
            user.SendAction(blackBoardMessageAction);
        }

        private void SendElectronicPanelMessage(User user, string message)
        {
            ActionData action = new ActionData(0x54, System.Text.Encoding.Unicode.GetBytes(message), (message.Length + 1) * 2);
            user.SendAction(action);
        }

        public void LoginUser(User user, string accountId, string accountPassword, string preferredLanguage)
        {
            Account account = AccountService.GetInstance().LogAccount(accountId, accountPassword);
            if (account == null)
                return;

            User loggedUser;
            if (m_usersAccounts.TryGetValue(account, out loggedUser))
            {
                ActionData disconnectAction = new ActionData(0x35);
                ActionDispatcher.Prepare().Action(disconnectAction).Send(new UserDestination(loggedUser));

                loggedUser.Server.DisconnectClient(loggedUser);
            }

            m_usersAccounts[account] = user;

            Console.WriteLine("Player logged in: {0}", account.Name);
            user.Account = account;
            user.Player = account.Player;

            SendClientVersion(user);

            int waitValue = 0;
            ActionData notifyRemainAction = new ActionData(0x60, BitConverter.GetBytes(waitValue), 4);
            user.SendAction(notifyRemainAction);

            Thread.Sleep(300);
            SendLoginResult(user);

            SendBlackBoardMessage(user, "Welcome to Open Carom3D (v 0.2.0 Alpha)");
            SendElectronicPanelMessage(user, "Hello World");

            PlayerStats playerStats = new PlayerStats
            {
                country = new char[] { 'B', 'R' }
            };
            ActionData playerStatsAction = new ActionData(0x68, System.Text.Encoding.Unicode.GetBytes(new string(playerStats.country)), playerStats.country.Length * 2);
            user.SendAction(playerStatsAction);

            SendNotifyMessage(user, "Welcome to Open Carom3D Server\n");

            string generatedChannelName = "Carom " + preferredLanguage + "-1";

            JoinChannel(user, generatedChannelName);
        }

        public void LogoutUser(User user)
        {
            RemoveUserFromCurrentSpot(user);
            if (user.Account != null)
                m_usersAccounts[user.Account] = null;
            m_clientsUsers.Remove(user.SessionId);
        }

        public void SendNotifyMessage(User user, string message)
        {
            ActionData greenMessage = new ActionData(0x1C, System.Text.Encoding.Unicode.GetBytes(message), (message.Length + 1) * 2);
            user.SendAction(greenMessage);
        }

        public Channel JoinChannel(User user, string channelName)
        {
            RemoveUserFromCurrentSpot(user);
            Channel channel = ChannelService.GetInstance().MoveUserToChannel(user, channelName, true);
            UpdateUserWithAllServerRooms(user);
            return channel;
        }

        public Room CreateRoom(User user, CreateRoomActionData createRoomActionData)
        {
            RemoveUserFromCurrentSpot(user);

            Room.GameInfo gameInfo = new Room.GameInfo
            {
                gameType = createRoomActionData.gameType,
                matchType = createRoomActionData.matchType,
                roomType = createRoomActionData.roomType,
                difficulty = createRoomActionData.difficulty,
                caneys = createRoomActionData.caneys
            };
            Room createdRoom = RoomService.GetInstance().CreateRoom(user, createRoomActionData.roomTitle, createRoomActionData.roomPassword, 10, gameInfo);
            return createdRoom;
        }

        public Room JoinRoom(User user, string roomTitle, string roomPassword)
        {
            Room room = RoomService.GetInstance().GetRoom(user.Server, roomTitle);
            if (room == null)
            {
                return null;
            }

            RemoveUserFromCurrentSpot(user);
            RoomService.GetInstance().InsertUserIntoRoom(room, user);
            return room;
        }

        public void ExitRoom(User user)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;
            JoinChannel(user, user.LastChannelName);
        }

        public void JoinRoomSlot(User user, int slot)
        {
            Room room = user.RoomIn;
            RoomService.GetInstance().SetUserToSlot(room, slot, user);
        }

        public void ExitRoomSlot(User user)
        {
            Room room = user.RoomIn;
            int userSlot = room.GetUserSlot(user);
            RoomService.GetInstance().SetUserToSlot(room, userSlot, null);
        }

        public void SetRoomSlotState(User user, int slot, int state)
        {
            Room room = user.RoomIn;
            RoomService.GetInstance().SetSlotState(room, slot, (Room.SlotState)state);
        }

        public void ChangeRoomState(User user, bool open)
        {
            Room room = user.RoomIn;
            if (room == null || room.RoomMaster != user)
                return;
            RoomService.GetInstance().ChangeRoomState(room, open);
        }

        public void KickUserFromRoom(User user, int userListIndex)
        {
            Room room = user.RoomIn;
            if (room == null || room.RoomMaster != user)
                return;

            User userToKick = room.UserInListIndex(userListIndex);
            if (userToKick == null)
                return;

            JoinChannel(userToKick, userToKick.LastChannelName);
        }

        public void SendMessageToRoom(User user, string message)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            if (message.Contains("-s"))
            {
                StartMatch(user);
            }

            RoomService.GetInstance().SendMessageToRoom(room, user, message);
        }

        public void StartMatch(User user)
        {
            Room room = user.RoomIn;
            if (room == null || room.RoomMaster != user)
                return;
            RoomService.GetInstance().StartMatch(room);
        }

        public void MatchFinished(User user)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            RoomService.GetInstance().UserFinishedPlaying(room, user);
        }

        public void RequestPlayerProfile(User user, string playerName)
        {
            User targetUser = FindUser(playerName);
            if (targetUser == null)
            {
                return;
            }

            user.SendAction(PlayerProfileActionTemplate(targetUser.Player).Data);
        }

        public void SendPrivateMessageToUser(User user, string userName, string message)
        {
            User targetUser = FindUser(userName);
            uint result;
            if (targetUser == null)
                result = 1;
            else if (targetUser.InCover(CoverType.Message))
                result = 2;
            else
                result = 0;

            if (result == 0)
                targetUser.SendAction(UserPrivateMessageActionTemplate(user.Player.Name, message).Data);
            user.SendAction(new ActionData(0x3D, BitConverter.GetBytes(result), sizeof(uint)));
        }

        public void RequestUserSpot(User user, string userName)
        {
            User targetUser = FindUser(userName);
            if (targetUser == null || targetUser.Spot == null)
            {
                user.SendAction(new ActionData(0x3E, System.Text.Encoding.Unicode.GetBytes(userName), (Player.NameMaxLen + 1) * 2));
                return;
            }

            string channelName = "";
            string roomName = "";
            string serverName = "";

            UserSpot spot = targetUser.Spot;
            if (spot.IsOfType(0))
            {
                channelName = spot.Name;
            }
            else if (spot.IsOfType(1))
            {
                roomName = spot.Name;
            }

            serverName = targetUser.Server.QualifiedName;

            ActionBuilder builder = new ActionBuilder(0x3F);
            builder.Add(targetUser.Player.Name, (Player.NameMaxLen + 1) * 2);
            builder.Add(channelName, 31 * 2);
            builder.Add(roomName, (40 + 1) * 2);
            builder.Add(serverName, (40 + 1) * 2);
            user.SendAction(builder.Build());
        }

        public void RequestGuildProfile(User user, string guildName)
        {
        }

        public void SendGuildMessage(User user, string message)
        {
        }

        public void RequestGuildUserSpots(User user, string guildName)
        {
        }

        public void SetCoverStates(User user, int[] states)
        {
            user.SetCoverStates(states);
        }

        public void SendMatchEventInfo(User user, byte[] data, uint dataSize)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            uint first4 = BitConverter.ToUInt32(data, 0);

            ActionData matchEventInfoAction = new ActionData(0x47, data, dataSize);
            foreach (User userIn in room.Users)
            {
                if (userIn != user)
                {
                    ActionDispatcher.Prepare()
                        .Action(matchEventInfoAction)
                        .Send(new UserDestination(userIn));
                }
            }
        }

        public void SendMatchEventInfo2(User user, byte[] data, uint dataSize)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            uint enteringUserListIndex = BitConverter.ToUInt32(data, 0);
            User enteringUser = room.UserInListIndex((int)enteringUserListIndex);

            if (enteringUser != null)
            {
                ActionData matchEventInfoAction2 = new ActionData(0x49, data, dataSize);
                ActionDispatcher.Prepare().Action(matchEventInfoAction2).Send(new UserDestination(enteringUser));
            }
        }

        public void RequestMatchMakerScreen(User user)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            RoomService.GetInstance().SetUserOutOfGameScreen(room, user);
        }

        public void InviteUserToRoom(User user, string userName)
        {
            Room room = user.RoomIn;
            if (room == null)
                return;

            uint result;
            User targetUser = FindUser(userName);
            if (targetUser == null || targetUser.Server != user.Server)
                result = 1;
            else if (targetUser.InCover(CoverType.In))
                result = 2;
            else if (targetUser.InRoom)
                result = 3;
            else
                result = 0;

            if (result == 0)
                targetUser.SendAction(UserInviteActionTemplate(user, user.RoomIn).Data);

            ActionBuilder builder = new ActionBuilder(0x4C);
            builder.Add(result);
            user.SendAction(builder.Build());
        }

        public void JoinUserRoom(User user, string userName)
        {
            Channel channel = user.ChannelIn;
            if (channel == null)
                return;

            uint result;
            Room room;

            User targetUser = FindUser(userName);
            if (targetUser == null || targetUser.Server != user.Server)
                result = 2;
            else
            {
                room = targetUser.RoomIn;
                if (room == null)
                    result = 3;
                else if (!string.IsNullOrEmpty(room.Password))
                    result = 4;
                else
                    result = 0;
            }

            if (result == 0)
            {
                RemoveUserFromCurrentSpot(user);
                RoomService.GetInstance().InsertUserIntoRoom(room, user);
            }

            ActionBuilder builder = new ActionBuilder(0x72);
            builder.Add(result);
            user.SendAction(builder.Build());
        }

        public void UpdateUserWithAllServerRooms(User user)
        {
            GameServer gameServer = user.Server;
            foreach (Room room in gameServer.Rooms)
            {
                if (room.Hidden)
                    continue;
                ActionDispatcher.Prepare()
                    .Action(ExistingRoomsNotificationActionTemplate(room).Data)
                    .Send(new UserDestination(user));
            }

            ActionData roomInfoEndAction = new ActionData(0x62);
            ActionDispatcher.Prepare().Action(roomInfoEndAction).Send(new UserDestination(user));
        }

        public void RemoveUserFromCurrentSpot(User user)
        {
            if (user.Spot == null)
                return;

            if (user.Spot.IsOfType(0))
                ChannelService.GetInstance().RemoveUserFromChannel((Channel)user.Spot, user);
            else if (user.Spot.IsOfType(1))
                RoomService.GetInstance().RemoveUserFromRoom((Room)user.Spot, user);
        }
    }
}
