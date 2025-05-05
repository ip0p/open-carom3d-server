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
}
