using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FTPClient.Client;
using FTPClient.Helpers;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Extensions.DependencyInjection;

namespace Test
{
    [TestClass]
    public class FTPClientTest
    {

        [TestMethod]
        public void TestFTPClientAll()
        {
            FtpServer.StartServiceThread();


            IPEndPoint server = CommandHelper.AddressParser("(127,0,0,1,0,21)");
            Client client = new Client(server, "anonymous", "anonymous@example.com");
            client.Connect();

        }
    }
}