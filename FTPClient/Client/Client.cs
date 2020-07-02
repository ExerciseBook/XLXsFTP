using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using FTPClient.Helpers;

namespace FTPClient.Client
{
    /// <summary>
    /// FTP 客户端封装
    /// </summary>
    public class Client
    {
        //TODO 来个好哥哥把整个类里的东西都写成支持异步

        private readonly Socket _commandConnection;
        private readonly IPEndPoint _serverIpe;
        private readonly Authorization _authorization;
        private readonly SocketHelper _commandHelper;

        /// <summary>
        /// 创建客户端对象
        /// </summary>
        /// <param name="server"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Client(IPEndPoint server, string username, string password)
        {
            this._serverIpe = server;

            _commandConnection = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _commandHelper = new SocketHelper(_commandConnection);

            this._authorization = new Authorization(username, password, _commandHelper);
        }

        /// <summary>
        /// 尝试连接服务器
        /// </summary>
        public void Connect()
        {
            _commandConnection.Connect(_serverIpe);

            int status;
            String line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 220) throw new FTPClientException(status, line);

            _authorization.Login();
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContentsBytes"></param>
        public void Upload(string filename, byte[] fileContentsBytes)
        {
            string line;
            int status;

            // 使用被动模式
            _commandHelper.Writeln("PASV");

            // 读取服务器的响应信息
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));

            // 判断状态码是否正常，不正常就进异常处理
            if (status != 227) throw new FTPClientException(status, line);

            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(line);

            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 上传
            dataSocket.Send(fileContentsBytes);
            dataSocket.Close();

            // 226 or 250
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="remotePath"></param>
        public void Upload(string localPath, string remotePath)
        {
            //TODO 来个好哥哥实现一下按照路径上传
            //就按照上面的那个函数依葫芦画瓢玩儿就好辣
            //请注意：在文件上传模式下，数据连接 和 指令连接 应当同时监听，在数据连接上传数据的时候，要监听指令连接是否有发来信息
            //文档 https://tools.ietf.org/html/rfc959 第 51 面
            //当然，Receive 的时候还要记得判断一下 Receive 的返回值是否大于零
            throw new NotImplementedException();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public byte[] Download(string filename)
        {
            int status;
            string line;

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // PASV => 227;
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);

            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 根据文件大小获取信息
            byte[] file = new byte[fileSize];
            dataSocket.Receive(file);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
            dataSocket.Close();

            return file;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="remotePath"></param>
        public void Download(string localPath, string remotePath)
        {
            //TODO 来个好哥哥实现一下按照路径下载
            //就按照上面的那个函数依葫芦画瓢玩儿就好辣
            //请注意：在文件上传模式下，数据连接 和 指令连接 应当同时监听，在数据连接上传数据的时候，要监听指令连接是否有发来信息
            //文档 https://tools.ietf.org/html/rfc959 第 51 面
            //当然，Receive 的时候还要记得判断一下 Receive 的返回值是否大于零
            throw new NotImplementedException();
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        public void Delete(string path)
        {
            int status;
            string line;

            _commandHelper.Writeln("DELE " + path);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 250) throw new FTPClientException(status, line);
        }

        public List<string> List(string path)
        {
            int status;
            string line;

            // PASV => 227;
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);


            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(line);
            SocketHelper dataHelper = new SocketHelper(dataSocket);

            _commandHelper.Writeln("LIST " + path);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            List<string> ret = new List<string>();
            do
            {
                //TODO 来个好哥哥把这个 List<string> 更换为可读性更高的版本吧
                line = System.Text.Encoding.UTF8.GetString(dataHelper.Readln());
                if (line.Length > 0) ret.Add(line);
            } while (line.Length != 0);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if  (status != 250) throw new FTPClientException(status, line);
            dataSocket.Close();

            return ret;
        }
    }
}
