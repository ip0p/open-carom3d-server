using System;
using core;

namespace business
{
    public class GameServerConfig : ServerConfig
    {
        public GameServerEventType EventType { get; set; }
        public string ServerName { get; set; }
        public int TableType { get; set; }
        public int ServerId { get; set; }
        public GameServerTemplate ServerTemplate { get; set; }

        public GameServerConfig(ServerConfig basicConfig, GameServerEventType eventType, string serverName, int tableType, int serverId, GameServerTemplate serverTemplate)
            : base(basicConfig)
        {
            EventType = eventType;
            ServerName = serverName;
            TableType = tableType;
            ServerId = serverId;
            ServerTemplate = serverTemplate;
        }
    }

    public enum GameServerEventType
    {
        Normal = 1
        // TODO: complete
    }

    public struct GameServerTemplate
    {
        public ServerTemplateId TemplateId { get; set; }
        public float BaseCoefficient { get; set; }
        public int MinPoints { get; set; }
        public int MaxPoints { get; set; }
    }

    public struct GameServerGeneralConfig
    {
        public int InitialRating { get; set; }
        public int BasePoints { get; set; }
        public int ArcadeDifficultyMultiplier { get; set; }
        public int SimuDifficultyMultiplier { get; set; }
        public int[] GameMultiplier { get; set; }

        public GameServerGeneralConfig(int initialRating, int basePoints, int arcadeDifficultyMultiplier, int simuDifficultyMultiplier, int[] gameMultiplier)
        {
            InitialRating = initialRating;
            BasePoints = basePoints;
            ArcadeDifficultyMultiplier = arcadeDifficultyMultiplier;
            SimuDifficultyMultiplier = simuDifficultyMultiplier;
            GameMultiplier = gameMultiplier;
        }
    }
}
