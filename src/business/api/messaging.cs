namespace business
{
    public struct LoginActionData
    {
        public string Username;
        public string Password;
        public string Country;
        public byte[] Unk;
        public byte[] Unk2;
        public uint Server;
    }

    public struct CreateRoomActionData
    {
        public string RoomTitle;
        public string RoomPassword;
        public uint Unk0x0A;
        public uint RoomType;
        public uint Unk0x00;
        public uint Difficulty;
        public uint LevelLimit;
        public uint GameType;
        public uint MatchType;
        public int Caneys;
    }

    public struct JoinRoomActionData
    {
        public string RoomTitle;
        public string RoomPassword;
    }

    public struct RoomSlotModificationActionData
    {
        public int SlotNumber;
        public uint Function;
    }
}
