using System;
using System.Collections.Generic;
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


            // 测试文件信息

            // 随便生成个文件名
            string filename = "/" + Guid.NewGuid().ToString() + ".txt";

            // 随便生成个文件数据
            string fileContents = Guid.NewGuid().ToString() + "\r\n" + Guid.NewGuid().ToString();
            byte[] fileContentsBytes = System.Text.Encoding.UTF8.GetBytes(fileContents);

            // 计算一下长度待会儿用
            int filesize = fileContentsBytes.Length;


            ////////////////////////////////////////////////////////////////////////////////////////////////
            List<string> list;


            // 测试文件上传
            TestUpload(client, filename, fileContentsBytes);

            // 测试目录列表
            list = TestList(client, "/");
            //TODO 检查文件是否上传成功

            // 测试文件下载
            byte[] downloadedFile = TestDownload(client, filename);
            Assert.AreEqual(System.Text.Encoding.UTF8.GetString(downloadedFile), fileContents);

            // 测试文件删除
            TestDelete(client, filename);

            // 测试文件列表
            TestList(client, "/");
            //TODO 检查文件是否删除成功
        }

        private void TestUpload(Client client, string filename, byte[] fileContentsBytes)
        {
            client.Upload(filename, fileContentsBytes);
        }

        private byte[] TestDownload(Client client, string filename)
        {
            return client.Download(filename);
        }

        private List<string> TestList(Client client, string s)
        {
            return client.List(s);
        }

        private void TestDelete(Client client, string filename)
        {
            client.Delete(filename);
        }
    }
}