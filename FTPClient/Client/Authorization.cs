using FTPClient.Helpers;

namespace FTPClient.Client
{
    class Authorization
    {
        private readonly SocketHelper _socketHelper;

        public string Username { get; }

        public string Password { get; }

        public Authorization(string username, string password, SocketHelper socketHelper)
        {
            this.Username = username;
            this.Password = password;

            this._socketHelper = socketHelper;
        }

        public void Login(out SystemType serverSystemType)
        {
            int status;
            string line;

            _socketHelper.Writeln("USER " + Username);
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 331) throw new FTPClientException(status, line);

            _socketHelper.Writeln("PASS " + Password);
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 230) throw new FTPClientException(status, line);

            _socketHelper.Writeln("SYST");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 215) throw new FTPClientException(status, line);
            serverSystemType = CheckSystemSupport(line);

            _socketHelper.Writeln("OPTS UTF8 ON");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            // if (status != 200) throw new FTPClientException(status, line);

            _socketHelper.Writeln("PWD");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 257) throw new FTPClientException(status, line);

            _socketHelper.Writeln("TYPE I");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 200) throw new FTPClientException(status, line);
        }

        public SystemType CheckSystemSupport(string line)
        {
            string[] t = line.Split(' ');
            if (t.Length < 2) throw new FTPException("Server not supported.\r\n" + line);
            string s = t[1].ToLower();

            if (s.Contains("windows"))
            {
                return SystemType.Windows;
            }

            if (s.Contains("unix") || s.Contains("linux"))
            {
                return SystemType.Unix;
            }

            throw new FTPException("Server not supported.\r\n" + line);
        }
    }
}