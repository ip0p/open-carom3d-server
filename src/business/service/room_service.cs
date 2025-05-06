using System;
using business.api;
using business.entity;
using business.game_server;
using business.util;

namespace business
{
    public class RoomService
    {
        private static RoomService g_roomService = new RoomService();

        public static RoomService GetInstance()
        {
            return g_roomService;
        }

        public Room CreateRoom(User user, string title, string password, int maxPlayers, Room.GameInfo gameInfo)
        {
            Room newRoom;
            user.Server.CreateRoom(title, user, maxPlayers, gameInfo, out newRoom);
            if (newRoom == null)
                return null;

            newRoom.SetPassword(password);

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
                        slotStatesLayout = DEATH_MATCH_SLOT_LAYOUT;
                    else if (gameType == GameType.GAME_CARDBALL_NORMAL ||
                             gameType == GameType.GAME_CARDBALL_HIGH)
                        slotStatesLayout = CARD_BALL_SLOT_LAYOUT;
                    else
                        slotStatesLayout = NORMAL_GAME_SLOT_LAYOUT;
                    break;

                case MatchType.MATCH_CHALLENGE:
                    slotStatesLayout = CHALLENGE_ROOM_SLOT_LAYOUT;
                    break;

                case MatchType.MATCH_PRACTICE:
                    slotStatesLayout = NORMAL_GAME_SLOT_LAYOUT;
                    newRoom.SetHidden(true);
                    break;

                default:
                    break;
            }

            newRoom.SetSlotStates((Room.SlotState[])slotStatesLayout);

            int listIndex = newRoom.InsertUser(user);
            user.SetSpot(newRoom);
            newRoom.SetUserToSlot(listIndex, ROOM_MASTER_SLOT);

            user.SendAction(RoomCreationActionTemplate.Create(newRoom).Data);
            // TODO: room creation fail

            ResetRoom(newRoom);

            RoomPlayerItemData item = new RoomPlayerItemData();
            ActionData playerItemInfosActionData = new ActionData(0x71, item, sizeof(RoomPlayerItemData));
            ActionDispatcher.Prepare()
                .Action(RoomPlayerJoinActionTemplate.Create(user.Player, listIndex).Data)
                .Action(playerItemInfosActionData)
                .Send(UserDestination.Create(user));

            ActionData playerListEnd = new ActionData(0x63);
            ActionDispatcher.Prepare().Action(playerListEnd).Send(UserDestination.Create(user));

            if (!newRoom.Hidden)
                NotifyServerOfRoomCreation(newRoom.Server, newRoom);

            return newRoom;
        }

        public void InsertUserIntoRoom(Room room, User user)
        {
            int listIndex = room.InsertUser(user);
            user.SetSpot(room);

            UpdateRoom(room);
            user.SendAction(RoomJoinActionTemplate.Create(room).Data);

            foreach (int userListIndex in room.UserQueue)
            {
                User userIn = room.UserInListIndex(userListIndex);
                if (userIn == user)
                    continue;

                RoomPlayerItemData item = new RoomPlayerItemData();
                ActionData playerItemInfosActionData = new ActionData(0x71, item, sizeof(RoomPlayerItemData));

                ActionDispatcher.Prepare().Action(RoomPlayerJoinActionTemplate.Create(userIn.Player, userListIndex).Data).Send(UserDestination.Create(user));
                ActionDispatcher.Prepare().Action(playerItemInfosActionData).Send(UserDestination.Create(user));
            }

            RoomPlayerItemData item2 = new RoomPlayerItemData();
            ActionData playerItemInfosActionData2 = new ActionData(0x71, item2, sizeof(RoomPlayerItemData));
            ActionDispatcher.Prepare()
                .Action(RoomPlayerJoinActionTemplate.Create(user.Player, listIndex).Data)
                .Action(playerItemInfosActionData2)
                .Send(RoomDestination.Create(room));

            ActionData playerListEnd = new ActionData(0x63, null, 0);
            ActionDispatcher.Prepare().Action(playerListEnd).Send(UserDestination.Create(user));

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
            user.SetSpot(null);
            User newRoomMaster = room.RoomMaster;
            UpdateRoom(room);

            ActionData exitRoomAction = new ActionData(0x26);
            ActionDispatcher.Prepare().Action(exitRoomAction).Send(UserDestination.Create(user));

            if (room.UsersInCount != 0)
            {
                if (currentRoomMaster != newRoomMaster)
                {
                    if (!room.InGame)
                        UpdateSlotInfo(room);

                    ActionData changeRoomMasterAction = new ActionData(0x32, newRoomMaster.Player.Name, (PLAYER_NAME_MAX_LEN + 1) * 2);
                    ActionDispatcher.Prepare().Action(changeRoomMasterAction).Send(RoomDestination.Create(room));
                    NotifyServerOfRoomMasterChange(room.Server, room);
                }

                int listIndex = userListIndex;
                ActionData userExitedRoomAction = new ActionData(0x28, listIndex, 4);
                ActionDispatcher.Prepare().Action(userExitedRoomAction).Send(RoomDestination.Create(room));

                NotifyServerOfRoomPlayerCountUpdate(room.Server, room);
            }
            else
            {
                int roomId = room.Id;

                GameServer server = room.Server;
                server.DestroyRoom(room);

                ActionData destroyRoomAction = new ActionData(0x2D, roomId, 4);
                ActionDispatcher.Prepare().Action(destroyRoomAction).Send(ServerDestination.Create(server, 1));
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
                ActionData playerJoinSlotAction = new ActionData(0x4D, modificationResultData, sizeof(SlotModificationResultData));
                userIn.SendAction(playerJoinSlotAction);
            }
        }

        public void SetSlotState(Room room, int slot, Room.SlotState state)
        {
            room.SetSlotState(slot, state);

            int listIndex = room.GetSlotUserListIndex(slot);
            int slotState = (int)room.GetSlotState(slot);
            SlotModificationResultData modificationResultData = new SlotModificationResultData { ListId = listIndex, SlotNumber = slot, Function = slotState };
            ActionData slotStateActionData = new ActionData(0x4D, modificationResultData, sizeof(SlotModificationResultData));
            ActionDispatcher.Prepare().Action(slotStateActionData).Send(RoomDestination.Create(room));
        }

        public void UpdateSlotInfo(Room room)
        {
            RoomSlotInfoData slotInfoData = new RoomSlotInfoData();
            Room.SlotInfos slotInfos = room.SlotInfos;
            for (int i = 0; i < 30; ++i)
            {
                slotInfoData.SlotState[i] = slotInfos.State[i];
                slotInfoData.PlayerListIndex[i] = slotInfos.PlayerListIndex[i];
            }
            ActionData slotInfoActionData = new ActionData(0x51, slotInfoData, sizeof(RoomSlotInfoData));
            ActionDispatcher.Prepare().Action(slotInfoActionData).Send(RoomDestination.Create(room));
        }

        public void ChangeRoomState(Room room, bool open)
        {
            room.SetClosed(!open);
        }

        public void SendMessageToRoom(Room room, User sender, string message)
        {
            int userListIndex = room.GetUserListIndex(sender);
            if (userListIndex >= 0)
            {
                RoomUserMessage roomUserMessageData = new RoomUserMessage { UserListIndex = userListIndex, Message = message };
                ActionData roomUserMessageActionData = new ActionData(0x55, roomUserMessageData, 4 + (message.Length + 1) * 2);
                ActionDispatcher.Prepare().Action(roomUserMessageActionData).Send(RoomDestination.Create(room));
            }
        }

        public void StartMatch(Room room)
        {
            if (room.InGame)
                return;
            room.StartMatch();

            Random random = new Random();
            int startRandomSeed = random.Next();
            ActionData matchStartedActionData = new ActionData(0x33, startRandomSeed, 4);
            ActionDispatcher.Prepare().Action(matchStartedActionData).Send(RoomDestination.Create(room));

            NotifyServerOfRoomStateUpdate(room.Server, room);
        }

        public void UserFinishedPlaying(Room room, User user)
        {
            room.SetUserFinishedPlaying(user);

            if (room.PlayingUserCount == 0)
            {
                room.SetState(Room.RoomState.IDLE);

                int state = (int)Room.RoomState.IDLE;
                ActionData setRoomStateActionData = new ActionData(0x50, state, 4);
                ActionDispatcher.Prepare().Action(setRoomStateActionData).Send(RoomDestination.Create(room));

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
                .Action(RoomCreationNotifyActionTemplate.Create(room).Data)
                .Send(ServerDestination.Create(server, 1));
        }

        public void NotifyServerOfRoomMasterChange(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, RoomMaster = room.RoomMaster?.Player.Name ?? "" };
            ActionData action = new ActionData(0x2E, updateInfo, sizeof(RoomUpdateInfo));
            ActionDispatcher.Prepare().Action(action).Send(ServerDestination.Create(server, 1));
        }

        public void NotifyServerOfRoomPlayerCountUpdate(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, PlayerCount = room.UsersInCount };
            ActionData action = new ActionData(0x2F, updateInfo, sizeof(RoomUpdateInfo));
            ActionDispatcher.Prepare().Action(action).Send(ServerDestination.Create(server, 1));
        }

        public void NotifyServerOfRoomStateUpdate(GameServer server, Room room)
        {
            RoomUpdateInfo updateInfo = new RoomUpdateInfo { RoomId = room.Id, State = room.State };
            ActionData action = new ActionData(0x30, updateInfo, sizeof(RoomUpdateInfo));
            ActionDispatcher.Prepare().Action(action).Send(ServerDestination.Create(server, 1));
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

            bool needsUserUpdateNotification = room.GetSlotUserListIndex(ROOM_MASTER_SLOT) == -1;

            room.SetUserToSlot(room.RoomMasterListIndex, ROOM_MASTER_SLOT);
            if (room.GameInfo.MatchType == MatchType.MATCH_CHALLENGE)
                needsUserUpdateNotification |= UpdateChallenge(room);

            if (needsUserUpdateNotification)
                UpdateSlotInfo(room);
        }

        private bool UpdateChallenge(Room room)
        {
            var userQueue = room.UserQueue;
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
