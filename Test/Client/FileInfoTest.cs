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
            Test("-rwxrwxrwx 1 owner group 74 Jul 20 10:53 5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt", false, "5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt", SystemType.Unix);
            Test("-rwxrwxrwx 1 owner group 3088 Dec 31 2107 sakuracarpet.png", false, "sakuracarpet.png", SystemType.Unix);
            Test("07-24-20  09:53AM                   21 t  e s   t     3 .txt", false, "t  e s   t     3 .txt", SystemType.Windows);
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
