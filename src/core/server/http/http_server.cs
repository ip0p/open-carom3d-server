using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using core.server;
using nettools;

namespace core
{
    public class HTTPProtocol : MessagingProtocol
    {
        public HTTPProtocol()
        {
        }

        public override ClientSession CreateSession(ntConnection ntClient, Server server)
        {
            return new ClientSession(ntClient, server);
        }

        private List<string> Split(string source, string delimiter)
        {
            List<string> ret = new List<string>();

            int startPos = 0, endPos;
            while ((endPos = source.IndexOf(delimiter, startPos)) != -1)
            {
                ret.Add(source.Substring(startPos, endPos - startPos));
                startPos = endPos + delimiter.Length;
            }
            ret.Add(source.Substring(startPos));
            return ret;
        }

        private List<byte> ReadFile(string path)
        {
            List<byte> ret = new List<byte>();
            if (File.Exists(path))
            {
                ret.AddRange(File.ReadAllBytes(path));
            }
            return ret;
        }

        private string GenerateProfileData(string userId)
        {
            List<byte> content = ReadFile("resources/user-profile-template.html");
            string s = Encoding.UTF8.GetString(content.ToArray());
            int is_ = 0;
            string userProperty = "${user.account.name}";
            while ((is_ = s.IndexOf(userProperty, is_)) != -1)
            {
                s = s.Remove(is_, userProperty.Length).Insert(is_, userId);
                is_ += userId.Length;
            }
            return s;
        }

        public override void OnMessageReceived(ClientSession user)
        {
            byte[] data = user.PendingReadData();
            int size = (int)user.PendingReadDataSize();
            string requestData = Encoding.UTF8.GetString(data, 0, size);
            Console.WriteLine(requestData);
            Console.WriteLine();

            List<string> requestLines = Split(requestData, "\r\n");
            if (requestLines.Count < 2)
                return;

            string firstLine = requestLines[0];
            List<string> request_ = Split(firstLine, " ");
            if (request_.Count < 3)
                return;

            string methodStr = request_[0];
            string uri = request_[1];
            string httpVersion = request_[2];

            if (requestLines[requestLines.Count - 1] != "")
                return;

            string header = "";
            List<byte> content = new List<byte>();
            if (uri.Contains("/blog/blog.asp"))
            {
                int idPos = uri.IndexOf("memberid=");
                if (idPos != -1)
                {
                    int idPosEnd = uri.IndexOf("&", idPos);
                    string userId = uri.Substring(idPos + "memberid=".Length, idPosEnd - idPos - "memberid=".Length);
                    string profileData = GenerateProfileData(userId);
                    content.AddRange(Encoding.UTF8.GetBytes(profileData));
                }
                header += "Content-Type: text/html\r\n";
            }
            else if (uri.Contains("/blog/getpp.asp"))
            {
                content = ReadFile("resources/open.png");
                header += "Content-Type: image/png\r\n";
            }
            else if (uri.Contains("/_banner/banner.asp"))
            {
                header += "Content-Type: text/html\r\n";
                content = ReadFile("resources/banner.html");
            }
            else if (uri.Contains("/spotlight/slide.asp"))
            {
                header += "Content-Type: text/html\r\n";
                content = ReadFile("resources/spotlight.html");
            }
            else
            {
                header += "Content-Type: text/html\r\n";
                string html = "<html><body style=\"background-color:#000000;\"></body></html>";
                content.AddRange(Encoding.UTF8.GetBytes(html));
            }

            string responseS = "HTTP/1.1 200 OK\r\n";
            responseS += header;
            responseS += "Cache-Control: no-cache\r\n";
            responseS += "Content-Length: " + content.Count + "\r\n";
            responseS += "Connection: Close\r\n";
            responseS += "\r\n";

            List<byte> response = new List<byte>(Encoding.UTF8.GetBytes(responseS));
            if (content.Count > 0)
                response.AddRange(content);

            user.DiscardReadPendingData(user.PendingReadDataSize());
            user.NtClient().Send(user.SessionId(), response.ToArray(), (uint)response.Count);
        }

        public override void OnMessageSent(ClientSession user)
        {
        }

        public override void CloseSession(ClientSession user)
        {
            user = null;
        }
    }

    public class HTTPServer : Server
    {
        private static HTTPProtocol g_httpProtocol = new HTTPProtocol();

        public HTTPServer(ServerConfig config)
            : base(config)
        {
        }

        public override MessagingProtocol MessagingProtocol()
        {
            return g_httpProtocol;
        }

        public override void Run()
        {
            while (true)
            {
                m_ntServer.Poll();
                Thread.Sleep(100);
            }
        }

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running HTTP server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling HTTP server project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
