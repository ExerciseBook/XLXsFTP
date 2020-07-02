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
        /// ���� Readln
        /// </summary>
        [TestMethod]
        public void TestReadln()
        {
            FTPServer.StartServiceThread();
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

                    // ����
                    int status;
                    String line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 220);
                    Assert.AreEqual(line, "220 FTP Server Ready");
                    
                    // �����û���
                    s.Writeln("USER anonymous");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 331);
                    Assert.AreEqual(line, "331 User anonymous logged in, needs password");

                    // ��������
                    s.Writeln("PASS anonymous@example.com");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 230);
                    Assert.AreEqual(line, "230 Password ok, FTP server ready");

                    // ����ģʽ
                    s.Writeln("PASV");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 227);
                    //Assert.AreEqual(line, "227 Entering Passive Mode (127,0,0,1,211,82).");
                    //��ַ:127.0.0.1 �˿�:211*256+82

                    // ������������
                    IPEndPoint dataIPE = CommandHelper.AddressParser(line);
                    Socket dataSocket = new Socket(dataIPE.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        tempSocket.Connect(ipe);
                    }
                    catch (Exception e)
                    {
                        tempSocket.Close();
                        continue;
                    }

                    // �����г�Ŀ¼
                    s.Writeln("LIST");
                    line = System.Text.Encoding.UTF8.GetString(s.Readln(out status));
                    Assert.AreEqual(status, 150);
                    Assert.AreEqual(line, "150 Opening data connection.");

                    return;
                }
            }
            Assert.IsTrue(false);
        }

    }
}
