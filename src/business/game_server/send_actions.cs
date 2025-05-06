using System;
using core;
using business.entity;

namespace business.game_server
{
    public static class SendActions
    {
        public static void FillRoomInfo(RoomInfoActionData roomInfo, Room room)
        {
            Room.GameInfo game = room.GameInfo;
            roomInfo.RoomId = room.Id;
            roomInfo.Difficulty = game.Difficulty;
            roomInfo.RoomName = room.Name;
            roomInfo.PlayersIn = room.UsersInCount;
            roomInfo.MaxPlayers = room.MaxPlayers;
            roomInfo.U = 3;
            roomInfo.LevelLimit = 0;
            roomInfo.GameType = game.GameType;
            roomInfo.RoomType = game.RoomType;
            roomInfo.MatchType = game.MatchType;
            roomInfo.RoomState = room.State;
            roomInfo.RoomMaster = room.RoomMaster != null ? room.RoomMaster.Player.Name : "";
            roomInfo.StraightWins = room.StraightWins;
            roomInfo.Caneys = game.Caneys;
        }

        public class RoomJoinActionTemplate : ActionTemplate
        {
            private readonly Room _room;

            public RoomJoinActionTemplate(Room room)
            {
                _room = room;
            }

            public override ActionData Data()
            {
                Room room = _room;
                User roomMaster = room.RoomMaster;

                JoinedRoomData roomData = new JoinedRoomData
                {
                    RoomTitle = room.Name,
                    RoomMaster = roomMaster == null ? "" : roomMaster.Player.Name,
                    Unk52 = 2,
                    State = room.State,
                    RoomType = room.GameInfo.RoomType,
                    GameType = room.GameInfo.GameType,
                    MatchType = room.GameInfo.MatchType,
                    Difficulty = room.GameInfo.Difficulty
                };

                Room.SlotInfos slotInfos = room.SlotInfos;
                for (int i = 0; i < 30; ++i)
                {
                    roomData.SlotInfos.SlotState[i] = slotInfos.State[i];
                    roomData.SlotInfos.PlayerListIndex[i] = slotInfos.PlayerListIndex[i];
                }

                return new ActionData(0x24, roomData.ToByteArray());
            }
        }

        public class RoomCreationActionTemplate : ActionTemplate
        {
            private readonly Room _room;

            public RoomCreationActionTemplate(Room room)
            {
                _room = room;
            }

            public override ActionData Data()
            {
                Room room = _room;
                User roomMaster = room.RoomMaster;

                CreatedRoomData roomData = new CreatedRoomData
                {
                    RoomTitle = room.Name,
                    RoomMaster = roomMaster == null ? "" : roomMaster.Player.Name,
                    Unk52 = 2,
                    RoomType = room.GameInfo.RoomType,
                    GameType = room.GameInfo.GameType,
                    MatchType = room.GameInfo.MatchType,
                    Difficulty = room.GameInfo.Difficulty
                };

                Room.SlotInfos slotInfos = room.SlotInfos;
                for (int i = 0; i < 30; ++i)
                {
                    roomData.SlotInfos.SlotState[i] = slotInfos.State[i];
                    roomData.SlotInfos.PlayerListIndex[i] = slotInfos.PlayerListIndex[i];
                }

                return new ActionData(0x25, roomData.ToByteArray());
            }
        }

        public class RoomPlayerJoinActionTemplate : ActionTemplate
        {
            private readonly Player _player;
            private readonly int _listIndex;

            public RoomPlayerJoinActionTemplate(Player player, int listIndex)
            {
                _player = player;
                _listIndex = listIndex;
            }

            public override ActionData Data()
            {
                Player player = _player;

                RoomPlayerData roomPlayer = new RoomPlayerData
                {
                    Id = player.Name,
                    Country = "BR",
                    AccountNumber = player.Id,
                    Level = player.Level,
                    ListIndex = _listIndex,
                    CueId = 3000,
                    Unk130 = 1,
                    Power = 150,
                    PowerRange = 150,
                    Chalks = 0,
                    Control = 150,
                    BackSpin = 54,
                    TopSpin = 54,
                    SideSpin = 54,
                    Unk84 = 0,
                    Unk6D = 0,
                    Unk120 = 0,
                    Unk121 = 0,
                    Unk130 = 0,
                    Unk145 = 0,
                    CharGender = 1,
                    Unused99 = 0,
                    Unk6D = 0,
                    UnusedA1 = 0,
                    UnkA5 = 0,
                    UnusedA9 = 0,
                    UnkA5 = 0
                };

                return new ActionData(0x27, roomPlayer.ToByteArray());
            }
        }

        public class PlayerProfileActionTemplate : ActionTemplate
        {
            private readonly Player _player;

            public PlayerProfileActionTemplate(Player player)
            {
                _player = player;
            }

            public override ActionData Data()
            {
                PlayerProfileData data = new PlayerProfileData
                {
                    PlayerNumber = _player.Id,
                    PlayerName = _player.Name,
                    PlayerGuild = "",
                    LoggedServerName = "",
                    ServerId = 0,
                    Country = "BR",
                    Wins = 0 * 10,
                    Level = _player.Level
                };

                return new ActionData(0x3A, data.ToByteArray());
            }
        }

        public class UserPrivateMessageActionTemplate : ActionTemplate
        {
            private readonly string _userName;
            private readonly string _message;

            public UserPrivateMessageActionTemplate(string userName, string message)
            {
                _userName = userName;
                _message = message;
            }

            public override ActionData Data()
            {
                return new ActionBuilder(0x3B)
                    .Add(_userName, (Player.NameMaxLength + 1) * 2)
                    .Add(_message)
                    .Build();
            }
        }

        public class UserInviteActionTemplate : ActionTemplate
        {
            private readonly User _user;
            private readonly Room _room;

            public UserInviteActionTemplate(User user, Room room)
            {
                _user = user;
                _room = room;
            }

            public override ActionData Data()
            {
                UserInvite userInvite = new UserInvite
                {
                    PlayerName = _user.Player.Name
                };
                FillRoomInfo(userInvite.RoomInfo, _room);
                userInvite.RoomPassword = _room.Password;

                return new ActionData(0x4B, userInvite.ToByteArray());
            }
        }

        public class ExistingRoomsNotificationActionTemplate : ActionTemplate
        {
            private readonly Room _room;

            public ExistingRoomsNotificationActionTemplate(Room room)
            {
                _room = room;
            }

            public override ActionData Data()
            {
                RoomInfoActionData actionData = new RoomInfoActionData();
                FillRoomInfo(actionData, _room);
                return new ActionData(0x2B, actionData.ToByteArray());
            }
        }

        public class RoomCreationNotifyActionTemplate : ActionTemplate
        {
            private readonly Room _room;

            public RoomCreationNotifyActionTemplate(Room room)
            {
                _room = room;
            }

            public override ActionData Data()
            {
                RoomInfoActionData actionData = new RoomInfoActionData();
                FillRoomInfo(actionData, _room);
                return new ActionData(0x2A, actionData.ToByteArray());
            }
        }

        public static void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running SendActions using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public static void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling SendActions project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
