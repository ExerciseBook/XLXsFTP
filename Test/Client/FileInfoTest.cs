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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Client
{
    using FTPClient.Client;

    [TestClass]
    public class FileInfoTest
    {
        [TestMethod]
        public void TestFileInfo()
        {
            Test("drwxrwxrwx 1 owner group 74 Jul 20 10:53 .", true, ".", SystemType.Unix);
            Test("-rwxrwxrwx 1 owner group 74 Jul 20 10:53 5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt", false,
                "5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt", SystemType.Unix);
            Test("-rwxrwxrwx 1 owner group 3088 Dec 31 2107 sakuracarpet.png", false, "sakuracarpet.png",
                SystemType.Unix);
            Test("07-24-20  09:53AM                   21 t  e s   t     3 .txt", false, "t  e s   t     3 .txt",
                SystemType.Windows);
            Test("07-24-20  09:50AM       <DIR>          folder", true, "folder", SystemType.Windows);
        }

        private void Test(string line, bool isFolder, string fileName, SystemType systemType)
        {
            FileInfo t = new FileInfo(line, systemType);

            Assert.AreEqual(t.IsFolder, isFolder);
            Assert.AreEqual(t.FileName, fileName);
        }
    }
}