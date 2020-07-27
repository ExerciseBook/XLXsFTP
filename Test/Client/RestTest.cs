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
using System.Net;
using System.Text;
using FTPClient.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Client
{
    using FTPClient.Client;

    [TestClass]
    public class RestTest
    {
        [TestMethod]
        public void FTPRestTest()
        {
            // 启动 FTP 服务器
            FtpServer.StartServiceThread();

            IPEndPoint server = CommandHelper.AddressParser("(127,0,0,1,0,21)");
            Client client = new Client(server, "anonymous", "anonymous@example.com");
            client.Connect();

            Random random = new Random();

            // 生成文件内容
            string fileContents = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString());
            byte[] fileContentsBytes = System.Text.Encoding.UTF8.GetBytes(fileContents);
            int filesize = fileContentsBytes.Length;

            // 生成文件名
            string remoteFilename = Guid.NewGuid().ToString() + ".txt";
            string remotePath = '/' + remoteFilename;

            // 随机一个中断点
            int uploadBreakpoint = random.Next(36) + 17;
            // 分两次上传数据
            byte[] uploadContentA = Encoding.UTF8.GetBytes(fileContents.Substring(0, uploadBreakpoint));
            client.Upload(remotePath, uploadContentA, 0);
            byte[] uploadContentB = Encoding.UTF8.GetBytes(fileContents.Substring(uploadBreakpoint));
            client.Upload(remotePath, uploadContentB, uploadBreakpoint);

            // 随机一个中断点
            int downloadBreakpoint = random.Next(36) + 17;
            // 一次性完整下载
            byte[] downloadContentFull = client.Download(remotePath, 0);
            string downloadContent = Encoding.UTF8.GetString(downloadContentFull);
            // 检验上传正确
            Assert.AreEqual(fileContents, downloadContent);


            byte[] downloadContentPart = client.Download(remotePath, downloadBreakpoint);
            downloadContent = Encoding.UTF8.GetString(downloadContentPart);
            // 检验下载正确
            Assert.AreEqual(fileContents.Substring(downloadBreakpoint), downloadContent);

            client.Disconnect();
        }
    }
}