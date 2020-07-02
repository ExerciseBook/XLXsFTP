using System;
using FTPClient.Helpers;

namespace FTPClient.Client
{
    class Authorization
    {
        private readonly string _username;
        private readonly string _password;
        private readonly SocketHelper _socketHelper;

        public string Username
        {
            get => _username;
        }

        public string Password
        {
            get => _password;

        }

        public Authorization(string username, string password, SocketHelper socketHelper)
        {
            this._username = username;
            this._password = password;

            this._socketHelper = socketHelper;
        }

        public void Login()
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

            _socketHelper.Writeln("OPTS UTF8 ON");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 200) throw new FTPClientException(status, line);

            _socketHelper.Writeln("PWD");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 257) throw new FTPClientException(status, line);

            _socketHelper.Writeln("TYPE I");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 200) throw new FTPClientException(status, line);
        }
    }
}