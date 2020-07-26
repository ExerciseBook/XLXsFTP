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
            // if (status / 100 != 2) throw new FTPClientException(status, line);

            _socketHelper.Writeln("PWD");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status != 257) throw new FTPClientException(status, line);

            _socketHelper.Writeln("TYPE I");
            line = System.Text.Encoding.UTF8.GetString(_socketHelper.Readln(out status));
            if (status / 100 != 2) throw new FTPClientException(status, line);
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