using System;
using System.Collections.Generic;
using System.Net;
using FTPClient.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Client
{
    using FTPClient.Client;

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
            List<FileInfo> list;

            // 测试文件上传
            TestUpload(client, filename, fileContentsBytes);

            // 测试目录列表
            list = TestList(client, "/");
            int flag = 0;
            foreach (var aFileInfo in list)
            {
                if (aFileInfo.FileName == filename.Substring(1))
                {
                    Assert.AreEqual(filesize, aFileInfo.Size);
                    flag = 1;
                    break;
                }
            }
            Assert.AreEqual(1, flag);

            // 测试文件下载
            byte[] downloadedFile = TestDownload(client, filename);
            Assert.AreEqual(System.Text.Encoding.UTF8.GetString(downloadedFile), fileContents);

            // 测试文件删除
            TestDelete(client, filename);

            // 测试文件列表
            TestList(client, "/");
            flag = 0;
            foreach (var aFileInfo in list)
            {
                if (aFileInfo.FileName == filename)
                {
                    flag = 1;
                    break;
                }
            }
            Assert.AreEqual(0, flag);

        }

        private void TestUpload(Client client, string filename, byte[] fileContentsBytes)
        {
            client.Upload(filename, fileContentsBytes);
        }

        private byte[] TestDownload(Client client, string filename)
        {
            return client.Download(filename);
        }

        private List<FileInfo> TestList(Client client, string s)
        {
            return client.List(s);
        }

        private void TestDelete(Client client, string filename)
        {
            client.Delete(filename);
        }
    }
}