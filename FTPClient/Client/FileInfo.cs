using System;
using System.Runtime.Serialization;
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


            public FileModifiedAt(SystemType systemType, string s, string s1, string s2)
            {
                if (systemType != SystemType.Unix) throw new ArgumentException("Invalid argument found when parsing LIST.");

                this._time = s2.Contains(":")
                    ? DateTime.Parse("" + this._now.Year + s + ' ' + s1 + ' ' + s2 + ':' + "00")
                    : DateTime.Parse(s + ' ' + s1 + ' ' + s2);
                this._time = this._time.ToLocalTime();
            }

            public FileModifiedAt(SystemType systemType, string s, string s1)
            {
                if (systemType != SystemType.Windows) throw new ArgumentException("Invalid argument found when parsing LIST.");

                string dateTime = "";
                string[] date = s.Split('-');
                dateTime = date[2] + '-' + date[0] + '-' + date[1] + ' ' + s1;

                this._time = DateTime.Parse(dateTime);
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

        public FileInfo(string line, SystemType serverSystemType)
        {
            int nameIndex = 0;
            int columnCount = 0;
            switch (serverSystemType)
            {
                case SystemType.Unix:
                    nameIndex = 8;
                    columnCount = 9;
                    break;
                case SystemType.Windows:
                    nameIndex = 3;
                    columnCount = 4;
                    break;
            }

            string[] s = new string[columnCount];

            int i = 0;
            int flag = 1;
            int p = 0;
            for (p = 0; p < line.Length; p++)
                if (line[p] != ' ')
                    break;

            for (; p < line.Length; p++)
            {
                // 文件名部分无视空格半段
                if (i == nameIndex)
                {
                    s[i] += line[p];
                    continue;
                }

                if (flag == 0)
                {
                    if (line[p] == ' ') continue;

                    flag = 1;
                    i++;
                    s[i] = "" + line[p];
                    continue;

                }

                if (flag == 1)
                {
                    if (line[p] != ' ')
                    {
                        s[i] += line[p];
                        continue;
                    }

                    flag = 0;
                    continue;
                }
            }

            switch (serverSystemType)
            {
                case SystemType.Unix:
                    this.Permission = new FilePermission(s[0]);
                    this.Preserved = Int32.Parse(s[1]);
                    this.Owner = s[2];
                    this.Group = s[3];
                    this.Size = this.IsFolder ? 0 : Int64.Parse(s[4]);
                    this.ModifiedAt = new FileModifiedAt(serverSystemType,s[5], s[6], s[7]);
                    this.FileName = s[8];
                    break;
                case SystemType.Windows:
                    this.Permission = s[2].ToLower() == "<dir>"
                        ? new FilePermission("drwxrwxrwx")
                        : new FilePermission("-rwxrwxrwx");
                    this.Preserved = 0;
                    this.Owner = "";
                    this.Group = "";
                    this.Size = this.IsFolder ? 0 : Int64.Parse(s[2]);
                    this.ModifiedAt = new FileModifiedAt(serverSystemType, s[0], s[1]);
                    this.FileName = s[3];
                    break;
            }


        }
    }
}