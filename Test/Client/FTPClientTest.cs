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
using System.Net;
using FTPClient.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

            // 测试文件名
            string testFile = "test.txt";

            // 本地文件路径
            string localDir = @"E:\";
            string localPath = localDir + testFile;

            // 远程文件路径
            string remoteDir = "/temp/";
            string remotePath = remoteDir + testFile;

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

            // 测试本地文件上传（路径上传）
            TestUpload(client, localPath, remotePath);

            // 本地文件长度
            long localFilesize = 0;
            using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    byte[] buff = new byte[fs.Length];
                    fs.Read(buff, 0, (int) fs.Length);
                    localFilesize = buff.Length;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            // 测试目录列表（路径上传）
            list = TestList(client, remoteDir);
            flag = 0;
            foreach (var aFileInfo in list)
            {
                if (aFileInfo.FileName == testFile)
                {
                    Assert.AreEqual(localFilesize, aFileInfo.Size);
                    flag = 1;
                    break;
                }
            }

            Assert.AreEqual(1, flag);

            // 测试文件下载
            byte[] downloadedFile = TestDownload(client, filename);
            Assert.AreEqual(System.Text.Encoding.UTF8.GetString(downloadedFile), fileContents);

            // 测试文件下载（路径下载）
            TestDownload(client, localPath, remotePath);

            // 计算下载文件长度
            long downloadFilesize = 0;
            using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    byte[] buff = new byte[fs.Length];
                    fs.Read(buff, 0, (int) fs.Length);
                    downloadFilesize = buff.Length;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            // 测试文件是否一致（路径下载）
            list = TestList(client, remoteDir);
            foreach (var aFileInfo in list)
            {
                if (aFileInfo.FileName == testFile)
                {
                    Assert.AreEqual(downloadFilesize, aFileInfo.Size);
                    break;
                }
            }

            // 测试目录删除
            string deleteDirectory = "/temp/deletetest";
            client.CreateDirectory(deleteDirectory);
            TestDeleteDirectory(client, deleteDirectory);

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

        private void TestUpload(Client client, string localPath, string remotePath)
        {
            client.Upload(localPath, remotePath);
        }

        private byte[] TestDownload(Client client, string filename)
        {
            return client.Download(filename);
        }

        private void TestDownload(Client client, string localPath, string remotePath)
        {
            client.Download(localPath, remotePath);
        }

        private List<FileInfo> TestList(Client client, string s)
        {
            return client.List(s);
        }

        private void TestDeleteDirectory(Client client, string remotePath)
        {
            client.DeleteDirectory(remotePath);
        }

        private void TestDelete(Client client, string filename)
        {
            client.Delete(filename);
        }
    }
}