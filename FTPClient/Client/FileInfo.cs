using System;
using System.Text.RegularExpressions;

namespace FTPClient.Client
{
    public class FileInfo
    {
        public class FilePermission
        {
            private string Value { get; }

            public FilePermission(string permission)
            {
                this.Value = permission;
            }

            public bool IsFolder()
            {
                return Value[0] == 'd';
            }
        }

        public class FileModifiedAt
        {
            private readonly DateTime _time;

            private readonly DateTime _now = DateTime.Now;


            public FileModifiedAt(string s, string s1, string s2)
            {
                this._time = s2.Contains(":")
                    ? DateTime.Parse("" + this._now.Year + s + ' ' + s1 + ' ' + s2 + ':' + "00")
                    : DateTime.Parse(s + ' ' + s1 + ' '+ s2);
                this._time = this._time.ToLocalTime();
            }

            public String ToString()
            {
                return this._time.Year == this._now.Year
                    ? this._time.ToString("MMMM dd HH:mm")
                    : this._time.ToString("yyyy-M-d dddd");
            }
        }

        /// <summary>
        /// 文件权限
        /// </summary>
        public FilePermission Permission { get; }

        /// <summary>
        /// 是否为文件夹
        /// </summary>
        /// <returns></returns>
        public bool IsFolder => Permission.IsFolder();

        /// <summary>
        /// 不知道是啥
        /// </summary>
        public int Preserved { get; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; }


        /// <summary>
        /// 所有者
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// 组
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 编辑时间
        /// </summary>
        public FileModifiedAt ModifiedAt { get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; }

        public FileInfo(string line)
        {

            string[] s = Regex.Split(line, "\\s+", RegexOptions.IgnoreCase);
            // -rwxrwxrwx 1 owner group 74 Jul 20 10:53 5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt

            // 0 -rwxrwxrwx
            // 1 1
            // 2 owner
            // 3 group
            // 4 74
            // 5 Jul                                                Dec  
            // 6 20                                                 31
            // 7 10:53                                              2107
            // 8 5ead33f1-6b54-4c31-ab1a-0427e9afa5c7.txt

            this.Permission = new FilePermission(s[0]);
            this.Preserved = Int32.Parse(s[1]);
            this.Owner = s[2];
            this.Group = s[3];
            this.Size = Int64.Parse(s[4]);
            this.ModifiedAt = new FileModifiedAt(s[5], s[6], s[7]);
            this.FileName = s[8];
        }
    }
}