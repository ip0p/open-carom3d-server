using System;
using core;
using business.service;
using business.entity;

namespace business.game_server
{
    public class CreateRoomAction : GameServerAction<CreateRoomActionData>
    {
        public override void Execute(ActionData action, User user, CreateRoomActionData data)
        {
            UserService.GetInstance().CreateRoom(user, data);
        }
    }

    public class JoinRoomAction : GameServerAction<JoinRoomActionData>
    {
        public override void Execute(ActionData action, User user, JoinRoomActionData data)
        {
            UserService.GetInstance().JoinRoom(user, data.RoomTitle, data.RoomPassword);
        }
    }

    public class RoomSlotModificationAction : GameServerAction<RoomSlotModificationActionData>
    {
        public override void Execute(ActionData action, User user, RoomSlotModificationActionData data)
        {
            switch (data.Function)
            {
                case 0:
                    if (data.SlotNumber >= 0)
                        UserService.GetInstance().JoinRoomSlot(user, data.SlotNumber);
                    else
                        UserService.GetInstance().ExitRoomSlot(user);
                    break;

                case 1:
                case 2:
                    UserService.GetInstance().SetRoomSlotState(user, data.SlotNumber, data.Function);
                    break;

                default:
                    Console.WriteLine($"Invalid slot modification function: {data.Function}");
                    break;
            }
        }
    }

    public class LoginAction : GameServerAction<LoginActionData>
    {
        public override void Execute(ActionData action, User user, LoginActionData data)
        {
            UserService.GetInstance().LoginUser(user, data.Username, data.Password, data.Country);
        }
    }

    public class JoinChannelAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string data)
        {
            UserService.GetInstance().JoinChannel(user, data);
        }
    }

    public class ChannelMessageAction : GameServerAction<string>
    {
        public override bool Validate(ActionData action)
        {
            return true;
        }

        public override void Execute(ActionData action, User user, string message)
        {
            UserSpot spot = user.Spot;
            if (spot == null || !spot.IsOfType(0))
                return;
            ChannelService.GetInstance().SendUserMessage((Channel)spot, user, message);
        }
    }

    public class ExitRoomAction : GameServerAction<object>
    {
        public override void Execute(ActionData action, User user, object data)
        {
            UserService.GetInstance().ExitRoom(user);
        }
    }

    public class StartMatchAction : GameServerAction<object>
    {
        public override void Execute(ActionData action, User user, object data)
        {
            UserService.GetInstance().StartMatch(user);
        }
    }

    public class EndMatchAction : GameServerAction<object>
    {
        public override void Execute(ActionData action, User user, object data)
        {
            UserService.GetInstance().MatchFinished(user);
        }
    }

    public class PlayerProfileRequestAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string playerName)
        {
            UserService.GetInstance().RequestPlayerProfile(user, playerName);
        }
    }

    public class UserPrivateMessageAction : GameServerAction<UserPrivateMessage>
    {
        public override void Execute(ActionData action, User user, UserPrivateMessage privateMessage)
        {
            UserService.GetInstance().SendPrivateMessageToUser(user, privateMessage.PlayerName, privateMessage.Message);
        }
    }

    public class UserSpotRequestAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string playerName)
        {
            UserService.GetInstance().RequestUserSpot(user, playerName);
        }
    }

    public class GuildProfileRequestAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string guildName)
        {
            UserService.GetInstance().RequestGuildProfile(user, guildName);
        }
    }

    public class GuildMessageAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string playerName)
        {
            UserService.GetInstance().SendGuildMessage(user, playerName);
        }
    }

    public class GuildUserSpotsRequestAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string guildName)
        {
            UserService.GetInstance().RequestGuildUserSpots(user, guildName);
        }
    }

    public class SetCoverStatesAction : GameServerAction<int[]>
    {
        public override void Execute(ActionData action, User user, int[] states)
        {
            UserService.GetInstance().SetCoverStates(user, states);
        }
    }

    public class MatchEventInfoAction : GameServerAction<byte[]>
    {
        public override void Execute(ActionData action, User user, byte[] data)
        {
            UserService.GetInstance().SendMatchEventInfo(user, data, action.Data.Length);
        }
    }

    public class MatchEventInfoAction2 : GameServerAction<byte[]>
    {
        public override void Execute(ActionData action, User user, byte[] data)
        {
            UserService.GetInstance().SendMatchEventInfo2(user, data, action.Data.Length);
        }
    }

    public class InviteUserToRoomAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string playerName)
        {
            UserService.GetInstance().InviteUserToRoom(user, playerName);
        }
    }

    public class RoomKickPlayerAction : GameServerAction<int>
    {
        public override void Execute(ActionData action, User user, int data)
        {
            UserService.GetInstance().KickUserFromRoom(user, data);
        }
    }

    public class MatchMakerScreenRequestAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string data)
        {
            UserService.GetInstance().RequestMatchMakerScreen(user);
        }
    }

    public class RoomMessageAction : GameServerAction<string>
    {
        public override bool Validate(ActionData action)
        {
            return action.Data.Length < (151 * sizeof(char));
        }

        public override void Execute(ActionData action, User user, string data)
        {
            UserService.GetInstance().SendMessageToRoom(user, data);
        }
    }

    public class JoinIdAction : GameServerAction<string>
    {
        public override void Execute(ActionData action, User user, string data)
        {
            UserService.GetInstance().JoinUserRoom(user, data);
        }
    }
}
