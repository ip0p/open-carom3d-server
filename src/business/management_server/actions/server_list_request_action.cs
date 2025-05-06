using System;
using System.Collections.Generic;
using business.entity;
using business.game_server;
using business.service;
using core;
using core.server.carom3d.util;

namespace business.management_server.actions
{
    public class ServerListRequestAction : ManagementServerAction<object>
    {
        public override bool Validate(ActionData action)
        {
            return true;
        }

        public override void Execute(ActionData action, User user, object unused)
        {
            ServerInfo serverInfo = new ServerInfo
            {
                ServerType = 0,
                ServerId = 0,
                ServerName = "",
                ServerNumber = 0x3,
                ServerHost = "",
                Port = 0,
                MaxPlayers = 500,
                MinPoints = 0,
                MaxPoints = 500,
                Unk1 = 0,
                Unk2 = 100,
                PlayersConnected = 0,
                ServerState = 1
            };

            ServerInfoUpdate updateInfo = new ServerInfoUpdate
            {
                ServerState = 1
            };

            List<Server> servers = ServerService.GetInstance().GetServerList();
            foreach (Server server in servers)
            {
                GameServerConfig config = ((GameServer)server).GsConfig;
                serverInfo.ServerType = (uint)config.EventType;
                serverInfo.ServerId = (uint)config.ServerId;
                serverInfo.ServerNumber = (uint)config.ServerTemplate.TemplateId;
                serverInfo.MinPoints = (uint)config.ServerTemplate.MinPoints;
                serverInfo.MaxPoints = (uint)config.ServerTemplate.MaxPoints;
                serverInfo.ServerName = ((GameServer)server).QualifiedName();
                serverInfo.ServerHost = config.HostInfo.Hostname;
                serverInfo.Port = (uint)config.HostInfo.Port;
                serverInfo.PlayersConnected = (uint)server.ClientsConnectedCount();

                ActionData serverInfoAction = new ActionData(0x01, serverInfo.ToByteArray());
                user.SendAction(serverInfoAction);

                ActionData serverListEnd = new ActionData(0x04, null);
                user.SendAction(serverListEnd);
            }
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public struct ServerInfo
    {
        public uint ServerType;
        public uint ServerId;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 41)]
        public string ServerName;
        public uint ServerNumber;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ServerHost;
        public uint Port;
        public uint MaxPlayers;
        public uint MinPoints;
        public uint MaxPoints;
        public uint Unk1;
        public uint Unk2;
        public uint PlayersConnected;
        public uint ServerState;

        public byte[] ToByteArray()
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(this);
            byte[] arr = new byte[size];
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            System.Runtime.InteropServices.Marshal.StructureToPtr(this, ptr, true);
            System.Runtime.InteropServices.Marshal.Copy(ptr, arr, 0, size);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public struct ServerInfoUpdate
    {
        public uint ServerId;
        public uint PlayersConnected;
        public uint ServerState;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ServerIp;
    }
}
