using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FTPClient.Helpers;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Extensions.DependencyInjection;

namespace Test
{
    [TestClass]
    public class SocketHelperTest
    {

        /// <summary>
        /// 测试 Readln
        /// </summary>
        [TestMethod]
        public void TotalTest()
        {
            FtpServer.StartServiceThread();
            // while (true) { }

            var port = 21;
            var hostEntry = Dns.GetHostEntry("127.0.0.1");
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    tempSocket.Connect(ipe);
                }
                catch (Exception e)
                {
                    continue;
                }

                if (tempSocket.Connected)
                {
                    var s = new SocketHelper(tempSocket);

                    // 连接
                    int status;
                    String line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 220);
                    Assert.AreEqual(line, "220 FTP Server Ready");
                    
                    // 输入用户名
                    s.Writeln("USER anonymous");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 331);
                    Assert.AreEqual(line, "331 User anonymous logged in, needs password");

                    // 输入密码
                    s.Writeln("PASS anonymous@example.com");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 230);
                    Assert.AreEqual(line, "230 Password ok, FTP server ready");

                    s.Writeln("SYST");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    // 215 UNIX Type: A

                    s.Writeln("OPTS UTF8 ON");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    // 200

                    s.Writeln("PWD");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    // 257
                    
                    s.Writeln("TYPE I");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    // 200

                    // 列出目录 CWD; PASV; LIST;

                    // Change working directory.
                    s.Writeln("CWD /");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));

                    // 被动模式
                    s.Writeln("PASV");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 227);
                    //Assert.AreEqual(line, "227 Entering Passive Mode (127,0,0,1,211,82).");
                    //地址:127.0.0.1 端口:211*256+82

                    // 创建数据连接
                    IPEndPoint dataIpe = CommandHelper.AddressParser(line);
                    Socket dataSocket = new Socket(dataIpe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        dataSocket.Connect(dataIpe);
                    }
                    catch (Exception e)
                    {
                        tempSocket.Close();
                        continue;
                    }

                    // 尝试列出目录

                    // s.Writeln("LIST -l");
                    // line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    // Assert.AreEqual(status, 150);
                    // Assert.AreEqual(line, "150 Opening data connection.");

                    // 开始列出目录
                    // 这里应该是要异步调用，然后等待指令通道获得关闭连接的信息
                    // 250 Closing data connection.

                    // while (true) { 
                    //     var d = new SocketHelper(dataSocket);
                    //     line = System.Text.Encoding.UTF8.GetString(d.Readln(out status));
                    //     // 一行一个文件描述
                    // 
                    //     // 比如说这样的
                    //     /*
                    //         drwxrwxrwx 1 owner group 0 Jun 30 23:06 .\r\n
                    //         -rwxrwxrwx 1 owner group 3 Jul 02 11:56 test.txt\r\n
                    //      */
                    //     break;
                    // }

                    dataSocket.Close();

                    // 测试文件信息
                    string filename = "/" + Guid.NewGuid().ToString() + ".txt";
                    string filecontent = Guid.NewGuid().ToString() + "\r\n" + Guid.NewGuid().ToString();
                    byte[] filecontentbyBytes = System.Text.Encoding.UTF8.GetBytes(filecontent);
                    int filesize = filecontentbyBytes.Length;

                    // 尝试上传文件
                    // PASV => 227
                    s.Writeln("PASV");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 227);

                    // 创建数据连接
                    dataIpe = CommandHelper.AddressParser(line);
                    dataSocket = new Socket(dataIpe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        dataSocket.Connect(dataIpe);
                    }
                    catch (Exception e)
                    {
                        tempSocket.Close();
                        continue;
                    }

                    // STOR 路径 => 150
                    s.Writeln("STOR " + filename);
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 150);

                    // 上传
                    dataSocket.Send(filecontentbyBytes);
                    dataSocket.Close();

                    // 226
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 226);



                    // 尝试下载文件
                    // SIZE 路径 => 213 文件大小;
                    s.Writeln("SIZE " + filename);
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 213);
                    int actualFilesize = Int32.Parse(line.Split(' ')[1]);
                    Assert.AreEqual(filesize, actualFilesize);

                    // PASV => 227;
                    s.Writeln("PASV");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status,227);

                    // 创建数据连接
                    dataIpe = CommandHelper.AddressParser(line);
                    dataSocket = new Socket(dataIpe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        dataSocket.Connect(dataIpe);
                    }
                    catch (Exception e)
                    {
                        tempSocket.Close();
                        continue;
                    }

                    // RETR 路径 => 150;
                    s.Writeln("RETR " + filename);
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 150);

                    // 根据文件大小获取信息
                    byte[] file = new byte[actualFilesize];
                    dataSocket.Receive(file);

                    // 226 => 结束数据连接
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 226);
                    dataSocket.Close();

                    Assert.AreEqual(System.Text.Encoding.UTF8.GetString(file), filecontent);

                    s.Writeln("DELE " + filename);
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 250);

                    return;
                }
            }
            Assert.IsTrue(false);
        }

    }
}
