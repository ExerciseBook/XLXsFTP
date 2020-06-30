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

        async private void StartService()
        {
            // Setup dependency injection
            var services = new ServiceCollection();

            // use %TEMP%/TestFtpServer as root folder
            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            // Add FTP server services
            // DotNetFileSystemProvider = Use the .NET file system functionality
            // AnonymousMembershipProvider = allow only anonymous logins
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            // Configure the FTP server
             services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

            // Build the service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                // Start the FTP server
                await ftpServerHost.StartAsync();

                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                Console.ReadLine();

                // Stop the FTP server
                await ftpServerHost.StopAsync();
            }
        }

        public void StartServiceThread()
        {
            ThreadStart childref = new ThreadStart(StartService);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }


        /// <summary>
        /// ≤‚ ‘ Readln
        /// </summary>
        [TestMethod]
        public void TestReadln()
        {
            StartServiceThread();

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

                    String line = System.Text.Encoding.UTF8.GetString(s.Readln());
                    

                    Assert.AreEqual(line, "220 FTP Server Ready");
                    return;
                }

                
            }

            Assert.IsTrue(false);
        }

    }
}
