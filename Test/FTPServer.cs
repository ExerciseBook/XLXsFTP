using System;
using System.IO;
using System.Threading;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace Test
{
    public class FTPServer
    {
        private static int _status = 0;

        static async private void StartService()
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

        static public void StartServiceThread()
        {
            if (_status == 0)
            {
                _status = 1;
                ThreadStart childref = new ThreadStart(StartService);
                Thread childThread = new Thread(childref);
                childThread.Start();
            }
        }
    }
}