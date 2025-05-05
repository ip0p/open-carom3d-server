using System;
using System.Collections.Generic;
using business.entity;

namespace business.service
{
    public class RoomService
    {
        private static RoomService _instance;

        private RoomService() { }

        public static RoomService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RoomService();
            }
            return _instance;
        }

        public Room CreateRoom(User user, string title, string password, int maxPlayers, Room.GameInfo gameInfo)
        {
            Room newRoom;
            user.Server.CreateRoom(title, user, maxPlayers, gameInfo, out newRoom);
            if (newRoom == null)
                return null;

            newRoom.Password = password;

            int[] slotStatesLayout = null;

            int matchType = newRoom.GameInfo.MatchType;
            int gameType = newRoom.GameInfo.GameType;
            switch (matchType)
            {
                case MatchType.MATCH_NORMAL:
                    if (gameType == GameType.GAME_DEATH_MATCH_NORMAL ||
                        gameType == GameType.GAME_DEATH_MATCH_HIGH ||
                        gameType == GameType.GAME_DEATH_MATCH_U ||
                        gameType == GameType.GAME_DEATH_MATCH_U2)
                        slotStatesLayout = Constants.DEATH_MATCH_SLOT_LAYOUT;
                    else if (gameType == GameType.GAME_CARDBALL_NORMAL ||
                             gameType == GameType.GAME_CARDBALL_HIGH)
                        slotStatesLayout = Constants.CARD_BALL_SLOT_LAYOUT;
                    else
                        slotStatesLayout = Constants.NORMAL_GAME_SLOT_LAYOUT;
                    break;

                case MatchType.MATCH_CHALLENGE:
                    slotStatesLayout = Constants.CHALLENGE_ROOM_SLOT_LAYOUT;
                    break;

                case MatchType.MATCH_PRACTICE:
                    slotStatesLayout = Constants.NORMAL_GAME_SLOT_LAYOUT;
                    newRoom.Hidden = true;
                    break;

                default:
                    break;
            }

            newRoom.SetSlotStates(slotStatesLayout);

            int listIndex = newRoom.InsertUser(user);
            user.Spot = newRoom;
            newRoom.SetUserToSlot(listIndex, Constants.ROOM_MASTER_SLOT);

            user.SendAction(new RoomCreationActionTemplate(newRoom).Data);
            ResetRoom(newRoom);

            RoomPlayerItemData item = new RoomPlayerItemData();
            ActionData playerItemInfosActionData = new ActionData(0x71, item.ToByteArray());
            ActionDispatcher.Prepare()
                .Action(new RoomPlayerJoinActionTemplate(user.Player, listIndex).Data)
                .Action(playerItemInfosActionData)
                .Send(new UserDestination(user));

            ActionData playerListEnd = new ActionData(0x63);
            ActionDispatcher.Prepare().Action(playerListEnd).Send(new UserDestination(user));

            if (!newRoom.Hidden)
                NotifyServerOfRoomCreation(newRoom.Server, newRoom);

            return newRoom;
        }

        public void InsertUserIntoRoom(Room room, User user)
        {
            int listIndex = room.InsertUser(user);
            user.Spot = room;

            UpdateRoom(room);
            user.SendAction(new RoomJoinActionTemplate(room).Data);

            foreach (int userListIndex in room.UserQueue)
            {
                User userIn = room.UserInListIndex(userListIndex);
                if (userIn == user)
                    continue;

                RoomPlayerItemData item = new RoomPlayerItemData();
                ActionData playerItemInfosActionData = new ActionData(0x71, item.ToByteArray());

                ActionDispatcher.Prepare().Action(new RoomPlayerJoinActionTemplate(userIn.Player, userListIndex).Data).Send(new UserDestination(user));
                ActionDispatcher.Prepare().Action(playerItemInfosActionData).Send(new UserDestination(user));
            }

            RoomPlayerItemData newItem = new RoomPlayerItemData();
            ActionData newPlayerItemInfosActionData = new ActionData(0x71, newItem.ToByteArray());
            ActionDispatcher.Prepare()
                .Action(new RoomPlayerJoinActionTemplate(user.Player, listIndex).Data)
                .Action(newPlayerItemInfosActionData)
                .Send(new RoomDestination(room));

            ActionData newPlayerListEnd = new ActionData(0x63);
            ActionDispatcher.Prepare().Action(newPlayerListEnd).Send(new UserDestination(user));

            NotifyServerOfRoomPlayerCountUpdate(user.Server, room);
        }

        public Room GetRoom(GameServer server, string roomTitle)
        {
            return server.GetRoom(roomTitle);
        }

        public void RemoveUserFromRoom(Room room, User user)
        {
            int listIndex = room.GetUserListIndex(user);
            if (listIndex >= 0)
            {
                RemoveUserFromRoom(room, listIndex);
            }
        }

        public void RemoveUserFromRoom(Room room, int userListIndex)
        {
            User user = room.UserInListIndex(userListIndex);
            if (user == null)
                return;

            User currentRoomMaster = room.RoomMaster;
            room.RemoveUser(userListIndex);
            user.Spot = null;
            User newRoomMaster = room.RoomMaster;
            UpdateRoom(room);

            ActionData exitRoomAction = new ActionData(0x26);
            ActionDispatcher.Prepare().Action(exitRoomAction).Send(new UserDestination(user));

            if (room.UsersInCount != 0)
            {
                if (currentRoomMaster != newRoomMaster)
                {
                    if (!room.InGame)
                        UpdateSlotInfo(room);

                    ActionData changeRoomMasterAction = new ActionData(0x32, System.Text.Encoding.Unicode.GetBytes(newRoomMaster.Player.Name));
                    ActionDispatcher.Prepare().Action(changeRoomMasterAction).Send(new RoomDestination(room));
                    NotifyServerOfRoomMasterChange(room.Server, room);
                }

                ActionData userExitedRoomAction = new ActionData(0x28, BitConverter.GetBytes(userListIndex));
                ActionDispatcher.Prepare().Action(userExitedRoomAction).Send(new RoomDestination(room));

                NotifyServerOfRoomPlayerCountUpdate(room.Server, room);
            }
            else
            {
                int roomId = room.Id;

                GameServer server = room.Server;
                server.DestroyRoom(room);

                ActionData destroyRoomAction = new ActionData(0x2D, BitConverter.GetBytes(roomId));
                ActionDispatcher.Prepare().Action(destroyRoomAction).Send(new ServerDestination(server, 1));
            }
        }

        public void SetUserToSlot(Room room, int slot, User user)
        {
            int result;
            if (user == null)
                result = room.FreeSlot(slot);
            else
            {
                if (user == room.RoomMaster)
                    return;
                result = room.SetUserToSlot(user, slot);
            }

            if (result == 1)
                return;

            int listIndex = room.GetSlotUserListIndex(slot);
            SlotModificationResultData modificationResultData = new SlotModificationResultData { ListId = listIndex, SlotNumber = slot, Function = 0 };
            foreach (int userIndex in room.UserQueue)
            {
                User userIn = room.UserInListIndex(userIndex);
                ActionData playerJoinSlotAction = new ActionData(0x4D, modificationResultData.ToByteArray());
                userIn.SendAction(playerJoinSlotAction);
            }
        }

        public void SetSlotState(Room room, int slot, Room.SlotState state)
        {
            room.SetSlotState(slot, state);

            int listIndex = room.GetSlotUserListIndex(slot);
            int slotState = (int)room.GetSlotState(slot);
            SlotModificationResultData modificationResultData = new SlotModificationResultData { ListId = listIndex, SlotNumber = slot, Function = slotState };
            ActionData slotStateActionData = new ActionData(0x4D, modificationResultData.ToByteArray());
            ActionDispatcher.Prepare().Action(slotStateActionData).Send(new RoomDestination(room));
        }

        public void UpdateSlotInfo(Room room)
        {
            RoomSlotInfoData slotInfoData = new RoomSlotInfoData();
            Room.SlotInfos slotInfos = room.SlotInfos;
            for (int i = 0; i < 30; i++)
            {
                slotInfoData.SlotState[i] = (int)slotInfos.State[i];
                slotInfoData.PlayerListIndex[i] = slotInfos.PlayerListIndex[i];
            }
            ActionData slotInfoActionData = new ActionData(0x51, slotInfoData.ToByteArray());
            ActionDispatcher.Prepare().Action(slotInfoActionData).Send(new RoomDestination(room));
        }

        public void ChangeRoomState(Room room, bool open)
        {
            room.Closed = !open;
        }

        public void SendMessageToRoom(Room room, User sender, string message)
        {
            int userListIndex = room.GetUserListIndex(sender);
            if (userListIndex >= 0)
            {
                RoomUserMessage roomUserMessageData = new RoomUserMessage { UserListIndex = userListIndex, Message = message };
                ActionData roomUserMessageActionData = new ActionData(0x55, roomUserMessageData.ToByteArray());
                ActionDispatcher.Prepare().Action(roomUserMessageActionData).Send(new RoomDestination(room));
            }
        }

        public void StartMatch(Room room)
        {
            if (room.InGame)
                return;
            room.StartMatch();

            Random random = new Random();
            int startRandomSeed = random.Next();
            ActionData matchStartedActionData = new ActionData(0x33, BitConverter.GetBytes(startRandomSeed));
            ActionDispatcher.Prepare().Action(matchStartedActionData).Send(new RoomDestination(room));

            NotifyServerOfRoomStateUpdate(room.Server, room);
        }

        public void UserFinishedPlaying(Room room, User user)
        {
            room.SetUserFinishedPlaying(user);

            if (room.PlayingUserCount == 0)
            {
                room.State = Room.RoomState.IDLE;

                int state = (int)Room.RoomState.IDLE;
                ActionData setRoomStateActionData = new ActionData(0x50, BitConverter.GetBytes(state));
                ActionDispatcher.Prepare().Action(setRoomStateActionData).Send(new RoomDestination(room));

                NotifyServerOfRoomStateUpdate(user.Server, room);
            }
        }

        public void SetUserOutOfGameScreen(Room room, User user)
        {
            room.SetUserOutOfGameScreen(user);

            ActionData showRoomScreenActionData = new ActionData(0x61);
            user.SendAction(showRoomScreenActionData);

            if (room.InGameScreenUserCount == 0)
            {
                ResetRoom(room);
            }
        }

        public void NotifyServerOfRoomCreation(GameServer server, Room room)
        {
            ActionDispatcher.Prepare()
                .Action(new RoomCreationNotifyActionTemplate(room).Data)
                .Send(new ServerDestination(server, 1));
        }

        public void NotifyServerOfRoomMasterChange(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, RoomMaster = room.RoomMaster.Player.Name };
            ActionData action = new ActionData(0x2E, updateInfo.ToByteArray());
            ActionDispatcher.Prepare().Action(action).Send(new ServerDestination(server, 1));
        }

        public void NotifyServerOfRoomPlayerCountUpdate(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, PlayerCount = room.UsersInCount };
            ActionData action = new ActionData(0x2F, updateInfo.ToByteArray());
            ActionDispatcher.Prepare().Action(action).Send(new ServerDestination(server, 1));
        }

        public void NotifyServerOfRoomStateUpdate(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, State = (int)room.State };
            ActionData action = new ActionData(0x30, updateInfo.ToByteArray());
            ActionDispatcher.Prepare().Action(action).Send(new ServerDestination(server, 1));
        }

        private void ResetRoom(Room room)
        {
            room.ResetUserFromSlots();
            UpdateRoom(room);
        }

        private void UpdateRoom(Room room)
        {
            if (room.State == Room.RoomState.IN_GAME)
                return;

            bool needsUserUpdateNotification = room.GetSlotUserListIndex(Constants.ROOM_MASTER_SLOT) == -1;

            room.SetUserToSlot(room.RoomMasterListIndex, Constants.ROOM_MASTER_SLOT);
            if (room.GameInfo.MatchType == MatchType.MATCH_CHALLENGE)
                needsUserUpdateNotification |= UpdateChallenge(room);

            if (needsUserUpdateNotification)
                UpdateSlotInfo(room);
        }

        private bool UpdateChallenge(Room room)
        {
            List<int> userQueue = room.UserQueue;
            if (userQueue.Count == 0)
                return false;

            bool needsUpdate = room.GetSlotUserListIndex(0) == -1;
            room.SetUserToSlot(userQueue[0], 0);
            if (userQueue.Count > 1)
            {
                room.SetUserToSlot(userQueue[1], 6);
                needsUpdate = true;
            }

            return needsUpdate;
        }
    }
}
