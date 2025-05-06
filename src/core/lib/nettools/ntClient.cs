using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace nettools
{
    public class NtClient : NtConnection
    {
        private IPEndPoint _address;

        public NtClient()
        {
        }

        public NtClient(NtEventHandler eventHandler)
        {
            SetEventHandler(eventHandler);
        }

        public NT_ERROR Connect(string hostname, ushort port)
        {
            _address = new IPEndPoint(IPAddress.Parse(hostname), port);
            return _connect(_socket, _address);
        }

        public NT_ERROR Send(string message)
        {
            return Send(Encoding.UTF8.GetBytes(message));
        }

        public NT_ERROR Send(byte[] data)
        {
            return Send(_socket, data, data.Length);
        }

        public virtual NT_ERROR Poll()
        {
            byte[] data = new byte[MAX_PACKET_SIZE];
            int bytesRecv;
            NT_ERROR result = _recv(_socket, data, out bytesRecv);
            if (result != NT_ERROR.SUCCESS)
            {
                if (bytesRecv == 0)
                {
                    // connection closed
                }
            }
            return result;
        }

        public virtual NT_ERROR Close()
        {
            Close();
            return NT_ERROR.SUCCESS;
        }
    }
}
