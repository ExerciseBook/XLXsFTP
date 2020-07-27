/*
 * This file is part of XLXsFTP
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2020 contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
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
        private SystemType _serverSystemType;

        public bool Connected { get; private set; } = false;

        /// <summary>
        /// 是否传输
        /// </summary>
        public bool Working { get; private set; } = true;

        /// <summary>
        /// 终止传输任务
        /// </summary>
        public void TerminateTransmissionTask() => this.Working = false;

        /// <summary>
        /// 终止传输任务
        /// </summary>
        public void ResumeTransmissionTask() => this.Working = true;

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

            _authorization.Login(out this._serverSystemType);

            this.Connected = true;
        }

        /// <summary>
        /// 选择模式并创建数据连接
        /// </summary>
        /// <param name="dataSocket"></param>
        /// <param name="dataHelper"></param>
        public void InitDataConnection(out Socket dataSocket, out SocketHelper dataHelper)
        {
            string line;
            int status;
            // EPSV => 229;
            _commandHelper.Writeln("EPSV");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status == 229)
            {
                string dataConnection = line;
                // 创建数据连接
                dataSocket = CommandHelper.AddressParserAndConnect(1, dataConnection, _serverIpe.Address);
                dataHelper = new SocketHelper(dataSocket);
            }
            else if (status / 100 == 5) // 不支持 EPSV, 使用 PASV
            {
                // PASV => 227;
                _commandHelper.Writeln("PASV");
                line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
                if (status == 227)
                {
                    string dataConnection = line;
                    // 创建数据连接
                    dataSocket = CommandHelper.AddressParserAndConnect(0, dataConnection);
                    dataHelper = new SocketHelper(dataSocket);
                }
                else
                {
                    throw new FTPClientException(status, line);
                }
            }
            else
            {
                throw new FTPClientException(status, line);
            }
        }

        /// <summary>
        /// 建立目录
        /// </summary>
        /// <param name="remotePath"></param>
        public void CreateDirectory(string remotePath)
        {
            string line;
            int status;

            // 建立目录
            string[] folders = remotePath.Split('/');
            bool flag = false;

            for (int i = 0; i < folders.Length; i++)
            {
                if (folders[i].Length == 0) continue;

                if (flag)
                {
                    // MKD 创建目录 => 257
                    _commandHelper.Writeln("MKD " + folders[i]);
                    line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
                    if (status != 257) throw new FTPClientException(status, line);
                }

                // CWD 切换工作区 => 250
                _commandHelper.Writeln("CWD " + folders[i]);
                line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));

                // 路径不存在
                if (status != 250)
                {
                    flag = true;
                    i--;
                }
            }

            // CWD 切换工作区至根目录
            _commandHelper.Writeln("CWD /");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="remotePath"></param>
        public void DeleteDirectory(string remotePath)
        {
            int status;
            string line;

            string[] folders = remotePath.Split('/');

            // 切换工作区
            for (int i = 0; i < folders.Length - 1; i++)
            {
                if (folders[i].Length == 0) continue;

                // CWD 切换工作区 => 250
                _commandHelper.Writeln("CWD " + folders[i]);
                line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));

                // 路径不存在
                if (status != 250) throw new FTPClientException(status, line);
            }

            // 删除目录 => 250
            _commandHelper.Writeln("RMD " + folders[folders.Length - 1]);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 250) throw new FTPClientException(status, line);

            // CWD 切换工作区至根目录
            _commandHelper.Writeln("CWD /");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
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
            Socket dataSocket;
            SocketHelper dataHelper;

            InitDataConnection(out dataSocket, out dataHelper);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status / 100 == 5) offset = 0;
            if (status != 350 && status / 100 != 5) throw new FTPClientException(status, line);

            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150 && status != 125) throw new FTPClientException(status, line);

            // 上传
            int datasize = 1024;
            byte[] data = new byte[datasize];
            for (int i = 0; i < fileContentsBytes.Length; i++)
            {
                int j = i;
                if (fileContentsBytes.Length - i < datasize)
                {
                    byte[] tmp = new byte[fileContentsBytes.Length - i];
                    while (j < fileContentsBytes.Length)
                    {
                        tmp[j - i] = fileContentsBytes[j];
                        j++;
                    }

                    dataSocket.Send(tmp);
                    break;
                }

                while (j < i + datasize)
                {
                    data[j - i] = fileContentsBytes[j];
                    j++;
                }

                dataSocket.Send(data);
                i = j - 1;
            }

            // 关闭socket连接
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
        public void Upload(string localPath, string remotePath, long offset = 0)
        {
            string line;
            int status;
            Socket dataSocket;
            SocketHelper dataHelper;

            InitDataConnection(out dataSocket, out dataHelper);

            // 处理路径
            string[] folders = remotePath.Split('/');
            string filename = folders[folders.Length - 1];
            bool flag = false;

            for (int i = 0; i < folders.Length - 1; i++)
            {
                if (folders[i].Length == 0) continue;

                if (flag)
                {
                    // MKD 创建目录 => 257
                    _commandHelper.Writeln("MKD " + folders[i]);
                    line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
                    if (status != 257) throw new FTPClientException(status, line);
                }

                // CWD 切换工作区 => 250
                _commandHelper.Writeln("CWD " + folders[i]);
                line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));

                // 路径不存在
                if (status != 250)
                {
                    flag = true;
                    i--;
                }
            }

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status / 100 == 5) offset = 0;
            if (status != 350 && status / 100 != 5) throw new FTPClientException(status, line);

            // STOR 路径 => 150
            _commandHelper.Writeln("STOR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));

            // 分段读取文件字节
            long buffsize = 1048576;
            byte[] buff = new byte[buffsize];
            long start = offset;
            try
            {
                FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                while (start < fs.Length && this.Working)
                {
                    if (ProcessUpdate != null) ProcessUpdate(start, fs.Length);

                    // 设置当前读取的起点
                    fs.Position = start;

                    // 读取一块文件
                    int tot = fs.Read(buff, 0, (int) Math.Min(buffsize, fs.Length - start));

                    // 上传
                    int datasize = 1024;
                    byte[] data = new byte[datasize];
                    for (long i = 0; i < tot; i++)
                    {
                        long j = i;
                        if ((long) tot - i < 1024)
                        {
                            byte[] tmp = new byte[tot - i];
                            while (j < tot)
                            {
                                tmp[j - i] = buff[j];
                                j++;
                            }

                            dataSocket.Send(tmp);
                            break;
                        }

                        while (j < i + datasize)
                        {
                            data[j - i] = buff[j];
                            j++;
                        }

                        dataSocket.Send(data);
                        i = j - 1;
                    }

                    // 更新下一次读取的起点
                    start += tot;
                }

                fs.Close();
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

            // CWD 切换工作区至根目录
            _commandHelper.Writeln("CWD /");
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
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
            Socket dataSocket;
            SocketHelper dataHelper;

            InitDataConnection(out dataSocket, out dataHelper);

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status / 100 == 5) offset = 0;
            if (status != 350 && status / 100 != 5) throw new FTPClientException(status, line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + filename);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150 && status != 125) throw new FTPClientException(status, line);

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
        public void Download(string localPath, string remotePath, long offset = 0)
        {
            int status;
            string line;
            Socket dataSocket;
            SocketHelper dataHelper;

            InitDataConnection(out dataSocket, out dataHelper);

            // 尝试下载文件
            // SIZE 路径 => 213 文件大小;
            _commandHelper.Writeln("SIZE " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 213) throw new FTPClientException(status, line);

            int fileSize = Int32.Parse(line.Split(' ')[1]);

            // REST 续传 => 350
            _commandHelper.Writeln("REST " + offset);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status / 100 == 5) offset = 0;
            if (status != 350 && status / 100 != 5) throw new FTPClientException(status, line);

            // RETR 路径 => 150;
            _commandHelper.Writeln("RETR " + remotePath);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150 && status != 125) throw new FTPClientException(status, line);

            // 获取本地目录
            string localDirectory = Directory.GetParent(localPath).FullName;
            // 判断目录是否存在
            if (!Directory.Exists(localDirectory))
            {
                // 创建目录
                Directory.CreateDirectory(localDirectory);
            }

            // 接收数据
            long start = offset;
            try
            {
                FileStream fs;
                if (offset == 0) fs = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                else fs = new FileStream(localPath, FileMode.Append, FileAccess.Write);

                int buffsize = 1048576;
                byte[] buff = new byte[buffsize];

                while (start < fileSize && this.Working)
                {
                    if (ProcessUpdate != null) ProcessUpdate(start, fileSize);

                    // 接收数据
                    int length = dataSocket.Receive(buff);
                    fs.Write(buff, 0, length);
                    start += length;
                }

                fs.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            dataSocket.Close();

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250) && (status != 550)) throw new FTPClientException(status, line);
            if (status == 550) throw new FTPClientException(status, line);
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
            Socket dataSocket;
            SocketHelper dataHelper;

            InitDataConnection(out dataSocket, out dataHelper);

            // 执行指令
            _commandHelper.Writeln("LIST " + path);
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if (status != 150 && status != 125) throw new FTPClientException(status, line);

            // 列出信息
            List<FileInfo> ret = new List<FileInfo>();
            do
            {
                line = System.Text.Encoding.UTF8.GetString(dataHelper.Readln());
                if (line.Length > 0) ret.Add(new FileInfo(line, this._serverSystemType));
            } while (line.Length != 0);

            // 226 => 结束数据连接
            line = System.Text.Encoding.UTF8.GetString(_commandHelper.Readln(out status));
            if ((status != 226) && (status != 250)) throw new FTPClientException(status, line);
            dataSocket.Close();

            return ret;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            this._commandConnection.Disconnect(false);
        }

        /// <summary>
        /// 进度条更新
        /// </summary>
        /// <param name="done"></param>
        /// <param name="total"></param>
        public delegate void ProcessUpdateDelegate(long done, long total);

        /// <summary>
        /// 进度条更新事件
        /// </summary>
        public event ProcessUpdateDelegate ProcessUpdate;
    }
}