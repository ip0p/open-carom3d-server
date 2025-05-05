using System;
using System.Collections.Generic;
using core;

namespace business.service
{
    public class UserService
    {
        private static UserService _instance;
        private Dictionary<int, User> _clientsUsers;
        private Dictionary<Account, User> _usersAccounts;

        private UserService()
        {
            _clientsUsers = new Dictionary<int, User>();
            _usersAccounts = new Dictionary<Account, User>();
        }

        public static UserService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new UserService();
            }
            return _instance;
        }

        public void LoginUser(User user, string accountId, string accountPassword, string preferredLanguage)
        {
            Account account = AccountService.GetInstance().LogAccount(accountId, accountPassword);
            if (account == null)
                return;

            User loggedUser = _usersAccounts[account];
            if (loggedUser != null)
            {
                // Send message to user
                ActionData disconnectAction = new ActionData(0x35);
                ActionDispatcher.Prepare().Action(disconnectAction).Send(new UserDestination(loggedUser));

                loggedUser.Server.DisconnectClient(loggedUser);
            }

            _usersAccounts[account] = user;

            Console.WriteLine($"Player logged in: {account.Name}");
            user.SetAccount(account);
            user.SetPlayer(account.Player);

            SendClientVersion(user);

            int waitValue = 0;
            ActionData notifyRemainAction = new ActionData(0x60, BitConverter.GetBytes(waitValue));
            user.SendAction(notifyRemainAction);

            // TODO: sleeping is bad. But without it, messages aren't processed correctly on client
            // Find a better way to do it
            System.Threading.Thread.Sleep(300);
            SendLoginResult(user);

            // TODO: process login result
            // If login result == 0
            SendBlackBoardMessage(user, "Welcome to Open Carom3D (v 0.2.0 Alpha)");
            SendElectronicPanelMessage(user, "Hello World");

            PlayerStats playerStats = new PlayerStats { Country = new char[] { 'B', 'R' } };
            ActionData playerStatsAction = new ActionData(0x68, playerStats.ToByteArray());
            user.SendAction(playerStatsAction);

            SendNotifyMessage(user, "Welcome to Open Carom3D Server\n");

            string generatedChannelName = "Carom " + preferredLanguage + "-1";

            JoinChannel(user, generatedChannelName);
        }

        public void LogoutUser(User user)
        {
            RemoveUserFromCurrentSpot(user);
            if (user.Account != null)
                _usersAccounts[user.Account] = null;
            _clientsUsers.Remove(user.SessionId);
        }

        public void SendNotifyMessage(User user, string message)
        {
            ActionData greenMessage = new ActionData(0x1C, System.Text.Encoding.Unicode.GetBytes(message));
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
                GameType = createRoomActionData.GameType,
                MatchType = createRoomActionData.MatchType,
                RoomType = createRoomActionData.RoomType,
                Difficulty = createRoomActionData.Difficulty,
                Caneys = createRoomActionData.Caneys
            };
            Room createdRoom = RoomService.GetInstance().CreateRoom(user, createRoomActionData.RoomTitle, createRoomActionData.RoomPassword, 10, gameInfo);
            return createdRoom;
        }

        private void SendClientVersion(User user)
        {
            byte[] version = System.Text.Encoding.ASCII.GetBytes("5.31");
            ActionData action = new ActionData(0x16, version);
            user.SendAction(action);
        }

        private void SendLoginResult(User user)
        {
            LoginResult loginResult = new LoginResult
            {
                Result = 0,
                PlayerName = user.Player.Name,
                AccountNumber = (uint)user.Account.Id,
                AccountLevel = 100
            };
            ActionData loginResultAction = new ActionData(0x18, loginResult.ToByteArray());
            user.SendAction(loginResultAction);
        }

        private void SendBlackBoardMessage(User user, string message)
        {
            ActionData blackBoardMessageAction = new ActionData(0x56, System.Text.Encoding.Unicode.GetBytes(message));
            user.SendAction(blackBoardMessageAction);
        }

        private void SendElectronicPanelMessage(User user, string message)
        {
            ActionData action = new ActionData(0x54, System.Text.Encoding.Unicode.GetBytes(message));
            user.SendAction(action);
        }

        private void RemoveUserFromCurrentSpot(User user)
        {
            if (user.Spot == null)
                return;

            if (user.Spot.IsOfType(0))
                ChannelService.GetInstance().RemoveUserFromChannel((Channel)user.Spot, user);
            else if (user.Spot.IsOfType(1))
                RoomService.GetInstance().RemoveUserFromRoom((Room)user.Spot, user);
        }

        private void UpdateUserWithAllServerRooms(User user)
        {
            GameServer gameServer = user.Server;
            foreach (Room room in gameServer.Rooms())
            {
                if (room.Hidden)
                    continue;
                ActionDispatcher.Prepare()
                    .Action(new ExistingRoomsNotificationActionTemplate(room).Data())
                    .Send(new UserDestination(user));
            }

            ActionData roomInfoEndAction = new ActionData(0x62);
            ActionDispatcher.Prepare().Action(roomInfoEndAction).Send(new UserDestination(user));
        }
    }
}
