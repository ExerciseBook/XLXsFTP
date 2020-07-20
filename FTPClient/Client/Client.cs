using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        public async void Connect()
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
        /// <param name="offset"></param>
        public void Upload(string filename, byte[] fileContentsBytes, int offset = 0)
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

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);
            
            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            byte[] tmp = new byte[1];
            // 上传
            for (int i = offset; i < fileContentsBytes.Length; i++)
            {
                tmp[0] = fileContentsBytes[i];
                dataSocket.Send(tmp);
            }
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
        /// <param name="offset"></param>
        public async void Upload(string localPath, string remotePath, long offset = 0)
        {
            //TODO 来个好哥哥实现一下按照路径上传
            //就按照上面的那个函数依葫芦画瓢玩儿就好辣
            //请注意：在文件上传模式下，数据连接 和 指令连接 应当同时监听，在数据连接上传数据的时候，要监听指令连接是否有发来信息
            //文档 https://tools.ietf.org/html/rfc959 第 51 面
            //当然，Receive 的时候还要记得判断一下 Receive 的返回值是否大于零
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

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 读取文件字节
            byte[] fileContentsBytes;
            using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    fileContentsBytes = new byte[fs.Length];
                    fs.Read(fileContentsBytes, 0, (int)fs.Length);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            byte[] tmp = new byte[1];
            // 上传
            for (long i = offset; i < fileContentsBytes.Length; i++)
            {
                tmp[0] = fileContentsBytes[i];
                dataSocket.Send(tmp);
            }
            dataSocket.Close();

            // 226 or 250
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] Download(string filename, int offset = 0)
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

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

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
        /// <param name="offset"></param>
        public async void Download(string localPath, string remotePath, long offset = 0)
        {
            //TODO 来个好哥哥实现一下按照路径下载
            //就按照上面的那个函数依葫芦画瓢玩儿就好辣
            //请注意：在文件上传模式下，数据连接 和 指令连接 应当同时监听，在数据连接上传数据的时候，要监听指令连接是否有发来信息
            //文档 https://tools.ietf.org/html/rfc959 第 51 面
            //当然，Receive 的时候还要记得判断一下 Receive 的返回值是否大于零
            int status;
            string line;

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // PASV => 227;
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);

            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(line);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 根据文件大小获取信息
            byte[] file = new byte[fileSize];
            dataSocket.Receive(file);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
            dataSocket.Close();

            // 写入文件
            if (offset == 0)
            {
                using (FileStream fs = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    try
                    {
                        fs.Write(file, 0, file.Length);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            } else
            {
                using (FileStream fs = new FileStream(localPath, FileMode.Append, FileAccess.Write))
                {
                    try
                    {
                        fs.Write(file, 0, file.Length);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

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

        /// <summary>
        /// 列出目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<FileInfo> List(string path)
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

            List<FileInfo> ret = new List<FileInfo>();
            do
            {
                line = System.Text.Encoding.UTF8.GetString(dataHelper.Readln());
                if (line.Length > 0) ret.Add(new FileInfo(line));
            } while (line.Length != 0);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if  (status != 250) throw new FTPClientException(status, line);
            dataSocket.Close();

            return ret;
        }
    }
}
