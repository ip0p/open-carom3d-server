using System;

namespace business.entity
{
    public class Match
    {
        public struct MatchState
        {
            public int[] CurrentScore;
            public int CurrentPlayer;
            public int Status;
            public uint Result;
        }

        private readonly Room _room;
        private readonly uint _id;
        private readonly uint _randomSeed;
        private readonly int _teamCount;
        private readonly int _playersPerTeam;

        public Match(int matchId, Room room, uint matchRandomSeed)
        {
            _room = room;
            _id = (uint)matchId;
            _randomSeed = matchRandomSeed;
            _teamCount = 0;
            _playersPerTeam = 0;
        }

        public uint Id => _id;
        public uint RandomSeed => _randomSeed;
        public int TeamCount => _teamCount;
        public int PlayersPerTeam => _playersPerTeam;
        public string MatchDescription => ""; // TODO: Implement
        public Room Room => _room;
    }
}
