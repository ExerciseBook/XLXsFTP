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
            Test("drwxrwxrwx 1 owner group 74 Jul 20 10:53 .", true, ".");
            Test("-rwxrwxrwx 1 owner group 74 Jul 20 10:53 5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt", false, "5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt");
            Test("-rwxrwxrwx 1 owner group 3088 Dec 31 2107 sakuracarpet.png", false, "sakuracarpet.png");
        }

        private void Test(string line, bool isFolder, string fileName)
        {
            FileInfo t = new FileInfo(line);

            Assert.AreEqual(t.IsFolder, isFolder);
            Assert.AreEqual(t.FileName, fileName);
        }
    }
}
