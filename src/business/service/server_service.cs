using System;
using System.Collections.Generic;
using System.Threading;
using business.game_server;
using core;

namespace business
{
    public class ServerService
    {
        private static ServerService g_serverService = new ServerService();
        private static List<Thread> m_threads = new List<Thread>();
        private List<Server> m_servers = new List<Server>();

        public static ServerService GetInstance()
        {
            return g_serverService;
        }

        public Server StartGameServer(GameServerConfig serverConfig)
        {
            GameServer server = new GameServer(serverConfig);
            Thread t = new Thread(() => RunServer(server));
            t.Start();
            m_threads.Add(t);
            m_servers.Add(server);
            return server;
        }

        private void RunServer(Server gameServer)
        {
            while (true)
            {
                gameServer.Poll();
                Thread.Sleep(10);
            }
        }

        public void Poll()
        {
            foreach (var server in m_servers)
            {
                server.Poll();
            }
        }

        public List<Server> GetServerList()
        {
            return m_servers;
        }
    }
}
