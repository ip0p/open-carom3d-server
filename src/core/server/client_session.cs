using System;
using System.Collections.Generic;
using nettools;

namespace core
{
    public class ClientSession
    {
        protected Server m_server;
        protected ntConnection m_ntClient;
        protected uint m_sessionId;
        protected List<byte> m_pendingDataToRead;
        protected List<byte> m_pendingDataToSend;

        public ClientSession(ntConnection ntClient, Server server)
        {
            m_ntClient = ntClient;
            m_server = server;
            m_sessionId = ntClient.Socket();
            m_pendingDataToRead = new List<byte>();
            m_pendingDataToSend = new List<byte>();
        }

        public void AppendReceivedData(byte[] data, uint dataLen)
        {
            m_pendingDataToRead.AddRange(data);
        }

        public void AppendDataToSend(byte[] data, uint dataLen)
        {
            m_pendingDataToSend.AddRange(data);
        }

        public void DiscardReadPendingData(uint dataLen)
        {
            m_pendingDataToRead.RemoveRange(0, (int)dataLen);
        }

        public void DiscardSendPendingData(uint dataLen)
        {
            m_pendingDataToSend.RemoveRange(0, (int)dataLen);
        }

        public byte[] PendingReadData()
        {
            return m_pendingDataToRead.ToArray();
        }

        public byte[] PendingSendData()
        {
            return m_pendingDataToSend.ToArray();
        }

        public uint PendingReadDataSize()
        {
            return (uint)m_pendingDataToRead.Count;
        }

        public uint PendingSendDataSize()
        {
            return (uint)m_pendingDataToSend.Count;
        }

        public Server Server()
        {
            return m_server;
        }

        public ntConnection NtClient()
        {
            return m_ntClient;
        }

        public uint SessionId()
        {
            return m_sessionId;
        }
    }
}
