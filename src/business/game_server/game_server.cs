using System;
using System.Collections.Generic;
using core;

namespace business
{
    public class GameServer : Carom3DServer
    {
        private string qualifiedName;
        private List<Room> rooms;
        public GameServerConfig gsConfig;

        public GameServer(GameServerConfig config) : base(config)
        {
            gsConfig = config;
            qualifiedName = config.serverName + " - " + ResolveTableTypeName(config.tableType);
            rooms = new List<Room>();
        }

        public override void OnClientDisconnection(ClientSession client)
        {
            User user = (User)client;
            UserService.GetInstance().LogoutUser(user);
            base.OnClientDisconnection(client);
        }

        public override MessagingProtocol MessagingProtocol()
        {
            return new GameServerProtocol();
        }

        public int CreateRoom(string title, User user, int maxPlayers, Room.GameInfo gameInfo, out Room pRetRoom)
        {
            pRetRoom = new Room(this, (uint)rooms.Count, title, user, maxPlayers, gameInfo);
            rooms.Add(pRetRoom);
            return 0;
        }

        public Room GetRoom(string title)
        {
            foreach (var room in rooms)
            {
                if (room.Name == title)
                    return room;
            }
            return null;
        }

        public int DestroyRoom(Room room)
        {
            if (rooms.Remove(room))
                return 0;
            return 1;
        }

        public string QualifiedName()
        {
            return qualifiedName;
        }

        public List<Room> Rooms()
        {
            return rooms;
        }

        private static string ResolveTableTypeName(int tableType)
        {
            switch (tableType)
            {
                case 0:
                    return "No Pocket";
                case 1:
                    return "Pocket";
                case 2:
                    return "Snooker";
                default:
                    return "All";
            }
        }
    }

    public class GameServerProtocol : Carom3DProtocol
    {
        public GameServerProtocol()
        {
            SetUserActionMap(new Dictionary<int, Action>
            {
                { 0x01, new LoginAction() },
                { 0x03, new JoinChannelAction() },
                { 0x05, new CreateRoomAction() },
                { 0x06, new JoinRoomAction() },
                { 0x6D, new RoomSlotModificationAction() }
            });
        }

        public override ClientSession CreateSession(ntConnection ntClient, Server server)
        {
            return new User(ntClient, server);
        }

        public override void OnUnhandledUserAction(Carom3DUserSession session, ActionData actionData)
        {
            User user = (User)session;
            if (user.Player == null)
            {
                base.OnUnhandledUserAction(session, actionData);
            }
            else
            {
                Console.WriteLine($"Unhandled client action: {user.Player.Name} - {actionData.Id()} - {actionData.Data().Count}");
                Console.WriteLine("Data: ");
                foreach (byte b in actionData.Data())
                {
                    Console.Write($"{b:X2} ");
                }
                Console.WriteLine();
                foreach (byte b in actionData.Data())
                {
                    Console.Write($"{(b >= 0x20 && b <= 0x7F ? (char)b : ' ')} ");
                }
                Console.WriteLine();
            }
        }
    }
}
