using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using business;
using business.management_server;
using core;
using core.server.http;

namespace server_starter
{
    class Program
    {
        private const string DEFAULT_HOSTNAME = "127.0.0.1";
        private static List<GameServerConfig> g_gameServerConfigs = new List<GameServerConfig>();
        private static int g_loadedGameServerCount = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Open Carom3D Server...");

            string hostName = File.ReadAllText("resources/host.txt").Trim();
            if (string.IsNullOrEmpty(hostName))
                hostName = DEFAULT_HOSTNAME;

            LoadGameServerConfigs(hostName, g_gameServerConfigs);
            Thread servers = new Thread(() => RunManagementServer(hostName));
            servers.Start();

            while (g_loadedGameServerCount < g_gameServerConfigs.Count)
                Thread.Sleep(300);

            Thread httpServers = new Thread(() => RunHTTPServer(hostName));
            httpServers.Start();

            Console.WriteLine("Open Carom3D Server initialized successfully.");

            string command;
            while ((command = Console.ReadLine()) != null)
            {
                if (command.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;
            }
        }

        private static void LoadGameServerConfigs(string hostname, List<GameServerConfig> gameServerConfigs)
        {
            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9883),
                GameServerEventType.Normal, "Novice", 4, 200,
                new GameServerTemplate(ServerTemplateId.Novice, 1.0f, 0, 1000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9884),
                GameServerEventType.Normal, "Advanced", 4, 400,
                new GameServerTemplate(ServerTemplateId.Advanced, 1.0f, 1000, 1000000000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9885),
                GameServerEventType.Normal, "Expert", 4, 500,
                new GameServerTemplate(ServerTemplateId.Expert, 1.0f, 10000, 1000000000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9886),
                GameServerEventType.Normal, "Professional", 4, 600,
                new GameServerTemplate(ServerTemplateId.Professional, 1.0f, 100000, 1000000000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9887),
                GameServerEventType.Normal, "Free", 4, 100,
                new GameServerTemplate(ServerTemplateId.Free, 1.0f, 0, 1000000000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9888),
                GameServerEventType.Normal, "Ranking", 4, 800,
                new GameServerTemplate(ServerTemplateId.Ranking, 1.0f, 1000, 1000000000)
            ));

            gameServerConfigs.Add(new GameServerConfig(
                new ServerConfig(hostname, 9889),
                GameServerEventType.Normal, "Training", 4, 0,
                new GameServerTemplate(ServerTemplateId.Training, 0.0f, 0, 0)
            ));
        }

        private static void RunManagementServer(string hostname)
        {
            foreach (var config in g_gameServerConfigs)
            {
                Console.WriteLine($"Initializing {config.ServerName} game server ({config.HostInfo.Hostname}:{config.HostInfo.Port})... ");
                ServerService.GetInstance().StartGameServer(config);
                Console.WriteLine("Done.");
                Interlocked.Increment(ref g_loadedGameServerCount);
            }

            ServerConfig managementServerConfig = new ServerConfig(hostname, 9882);
            ManagementServer managementServer = new ManagementServer(managementServerConfig);
            managementServer.Run();
        }

        private static void RunHTTPServer(string hostname)
        {
            HTTPServer httpServer = new HTTPServer(new ServerConfig(hostname, 80));
            httpServer.Run();
        }
    }
}
