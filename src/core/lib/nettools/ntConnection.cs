using System;
using System.Net;
using System.Net.Sockets;

namespace nettools
{
    public enum NT_ERROR
    {
        NTERR_SUCCESS,
        NTERR_WOULDBLOCK,
        NTERR_ADDRINUSE,
        NTERR_CONNREFUSED,
        NTERR_OTHER
    }

    public class NtConnection
    {
        protected Socket _socket;
        protected IPEndPoint _address;
        protected NtEventHandler _eventHandler;
        protected object _bindedData;

        public NtConnection()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetBlockingMode(false);
        }

        public NtConnection(Socket socket, IPEndPoint address)
        {
            _socket = socket;
            _address = address;
        }

        public NT_ERROR SetBlockingMode(bool blocking)
        {
            _socket.Blocking = blocking;
            return NT_ERROR.NTERR_SUCCESS;
        }

        public NT_ERROR SetOption()
        {
            // Implement setting socket options if needed
            return NT_ERROR.NTERR_SUCCESS;
        }

        public NT_ERROR GetOption()
        {
            // Implement getting socket options if needed
            return NT_ERROR.NTERR_SUCCESS;
        }

        public NT_ERROR Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
                return NT_ERROR.NTERR_SUCCESS;
            }
            catch (SocketException ex)
            {
                return WsaToNtError(ex.SocketErrorCode);
            }
        }

        public NT_ERROR Recv(byte[] data, out int recvLen)
        {
            try
            {
                recvLen = _socket.Receive(data);
                return NT_ERROR.NTERR_SUCCESS;
            }
            catch (SocketException ex)
            {
                recvLen = 0;
                return WsaToNtError(ex.SocketErrorCode);
            }
        }

        public virtual NT_ERROR Poll()
        {
            return NT_ERROR.NTERR_SUCCESS;
        }

        public virtual NT_ERROR Close()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            return NT_ERROR.NTERR_SUCCESS;
        }

        public void SetEventHandler(NtEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public NtEventHandler GetEventHandler()
        {
            return _eventHandler;
        }

        public void BindData(object data)
        {
            _bindedData = data;
        }

        public object GetBindedData()
        {
            return _bindedData;
        }

        private NT_ERROR WsaToNtError(SocketError socketError)
        {
            switch (socketError)
            {
                case SocketError.WouldBlock:
                    return NT_ERROR.NTERR_WOULDBLOCK;
                case SocketError.AddressAlreadyInUse:
                    return NT_ERROR.NTERR_ADDRINUSE;
                case SocketError.ConnectionRefused:
                    return NT_ERROR.NTERR_CONNREFUSED;
                default:
                    return NT_ERROR.NTERR_OTHER;
            }
        }

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
