using System;
using System.Net;
using System.Net.Sockets;
using FTPClient.Helpers;

namespace FTPClient.Client
{
    public class Client
    {
        private readonly Socket _socketConnection;
        private readonly IPEndPoint _serverIpe;
        private readonly Authorization _authorization;
        private readonly SocketHelper _socketHelper;

        /// <summary>
        /// 创建客户端对象
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Client(IPEndPoint server, string username, string password)
        {
            this._serverIpe = server;

            _socketConnection = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socketHelper = new SocketHelper(_socketConnection);

            this._authorization = new Authorization(username, password, _socketHelper);
        }

        /// <summary>
        /// 尝试连接服务器
        /// </summary>
        public void Connect()
        {
            _socketConnection.Connect(_serverIpe);

            int status;
            String line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 220) throw new FTPClientException(status, line);

            _authorization.Login();
        }

    }
}
