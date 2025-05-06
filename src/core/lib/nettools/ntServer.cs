using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace nettools
{
    public class NtServer : NtConnection
    {
        private List<NtConnection> _clients;

        public NtServer()
        {
            _clients = new List<NtConnection>();
        }

        public NT_ERROR Listen(ushort port)
        {
            IPEndPoint addr = new IPEndPoint(IPAddress.Any, port);
            NT_ERROR result = _bind(_socket, addr);
            if (result != NT_ERROR.NTERR_SUCCESS)
            {
                _close(_socket);
                return result;
            }
            return _listen(_socket);
        }

        public override NT_ERROR Poll()
        {
            NT_ERROR result;
            NtConnection newClient;
            do
            {
                result = _accept(_socket, out newClient);
                if (newClient != null)
                {
                    _clients.Add(newClient);
                    if (_eventHandler != null)
                    {
                        _eventHandler.OnClientConnection((NtClient)newClient);
                    }
                }
            } while (result == NT_ERROR.NTERR_SUCCESS);

            for (int i = 0; i < _clients.Count; i++)
            {
                NtConnection conn = _clients[i];
                byte[] data = new byte[MAX_PACKET_SIZE];
                int recvd;
                result = _recv(conn.Socket, data, out recvd);
                if (result == NT_ERROR.NTERR_SUCCESS)
                {
                    if (_eventHandler != null)
                    {
                        if (recvd == 0)
                        {
                            _eventHandler.OnClientDisconnection((NtClient)conn);
                            _clients.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            _eventHandler.OnClientDataReceived((NtClient)conn, data, (uint)recvd);
                        }
                    }
                }
            }
            return NT_ERROR.NTERR_SUCCESS;
        }

        public NT_ERROR Disconnect(NtConnection connection)
        {
            if (_clients.Remove(connection))
            {
                _eventHandler.OnClientDisconnection((NtClient)connection);
                return connection.Close();
            }
            return NT_ERROR.NTERR_SUCCESS;
        }

        public override NT_ERROR Close()
        {
            base.Close();
            return NT_ERROR.NTERR_SUCCESS;
        }

        public int ConnectionCount()
        {
            return _clients.Count;
        }
    }
}
