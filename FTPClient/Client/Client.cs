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

        public bool Connected { get; private set; } = false;

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

            this.Connected = true;
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
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);
            string dataConnection = line;

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);
            
            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(dataConnection);

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
            string line;
            int status;

            // 使用被动模式
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);
            string dataConnection = line;

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 分段读取文件字节
            byte[] buff = new byte[1024];
            long buffsize = buff.Length;
            long start = offset;
            try
            {
                FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                while (start < fs.Length)
                {
                    // 设置当前读取的起点
                    fs.Position = start;

                    // 读取一块文件
                    int tot = fs.Read(buff, 0, (int)Math.Min(buffsize, fs.Length - start));

                    byte[] tmp = new byte[1];
                    // 上传
                    for (long i = 0; i < tot; i++)
                    {
                        tmp[0] = buff[i];
                        dataSocket.Send(tmp);
                    }

                    // 更新下一次读取的起点
                    start += tot;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // 关闭socket连接
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

            // PASV => 227;
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);
            string dataConnection = line;

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);

            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(dataConnection);

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
            int status;
            string line;

            // PASV => 227;
            _commandHelper.Writeln("PASV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 227) throw new FTPClientException(status, line);
            string dataConnection = line;

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 350) throw new FTPClientException(status, line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);
            
            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(dataConnection);

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
            string dataConnection = line;

            // 执行指令
            _commandHelper.Writeln("LIST " + path);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150) throw new FTPClientException(status, line);
            
            // 创建数据连接
            Socket dataSocket = CommandHelper.AddressParserAndConnect(dataConnection);
            SocketHelper dataHelper = new SocketHelper(dataSocket);

            // 列出信息
            List<FileInfo> ret = new List<FileInfo>();
            do
            {
                line = System.Text.Encoding.UTF8.GetString(dataHelper.Readln());
                if (line.Length > 0) ret.Add(new FileInfo(line));
            } while (line.Length != 0);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
            dataSocket.Close();

            return ret;
        }
    }
}
