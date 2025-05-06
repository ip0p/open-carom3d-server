using System;
using System.Collections.Generic;
using System.Threading;
using nettools;

namespace core
{
    public class EventHandler : ntEventHandler
    {
        private Server m_server;

        public EventHandler(Server server)
        {
            m_server = server;
        }

        public override void OnClientConnection(ntClient ntClient)
        {
            ClientSession client = m_server.MessagingProtocol().CreateSession(ntClient, m_server);
            ntClient.BindData(client);
            m_server.OnClientConnection(client);
        }

        public override void OnClientDisconnection(ntClient ntClient)
        {
            ClientSession client = m_server.Client(ntClient.Socket());
            m_server.OnClientDisconnection(client);
            m_server.MessagingProtocol().CloseSession(client);
        }

        public override void OnClientDataReceived(ntClient ntClient, byte[] data, uint dataLen)
        {
            ClientSession client = m_server.Client(ntClient.Socket());
            client.AppendReceivedData(data, dataLen);
            m_server.MessagingProtocol().OnMessageReceived(client);
        }

        public override void OnClientDataSent(ntClient ntClient, byte[] data, uint len)
        {
            ClientSession client = m_server.Client(ntClient.Socket());
            // TODO
        }
    }

    public abstract class Server
    {
        protected ServerConfig m_config;
        protected ntServer m_ntServer;
        protected Dictionary<uint, ClientSession> m_clients;

        public Server(ServerConfig config)
        {
            m_config = config;
            m_ntServer = new ntServer();
            m_ntServer.SetEventHandler(new EventHandler(this));
            m_ntServer.Listen(config.HostInfo.Port);
            m_clients = new Dictionary<uint, ClientSession>();
        }

        public virtual void Poll()
        {
            m_ntServer.Poll();
        }

        public virtual void Run()
        {
            while (true)
            {
                m_ntServer.Poll();
                Thread.Sleep(10);
            }
        }

        public virtual void AddClient(ClientSession client)
        {
            m_clients[client.SessionId()] = client;
        }

        public virtual void RemoveClient(ClientSession client)
        {
            m_clients.Remove(client.SessionId());
        }

        public virtual void DisconnectClient(ClientSession client)
        {
            m_ntServer.Disconnect(client.NtClient());
        }

        public virtual void OnClientConnection(ClientSession client)
        {
            AddClient(client);
        }

        public virtual void OnClientDataReceived(ClientSession client)
        {
            // TODO
        }

        public virtual void OnClientDataSent(ClientSession client)
        {
            // TODO
        }

        public virtual void OnClientDisconnection(ClientSession client)
        {
            RemoveClient(client);
        }

        public ServerConfig Config()
        {
            return m_config;
        }

        public ClientSession Client(uint sessionId)
        {
            return m_clients.ContainsKey(sessionId) ? m_clients[sessionId] : null;
        }

        public Dictionary<uint, ClientSession> Clients()
        {
            return m_clients;
        }

        public uint ClientsConnectedCount()
        {
            return (uint)m_clients.Count;
        }

        public abstract MessagingProtocol MessagingProtocol();

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
