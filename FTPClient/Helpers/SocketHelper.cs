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

using System.Net.Sockets;
using System.Text;

namespace FTPClient.Helpers
{
    public class SocketHelper
    {
        private Socket _s;

        public SocketHelper(Socket s)
        {
            this._s = s;
        }

        public byte[] Readln(out int status)
        {
            byte[] buffer = new byte[65536];

            int index = 0;
            byte[] read = new byte[1];
            do
            {
                if (_s.Receive(read) == 0) break;
                if (read[0] != '\r' && read[0] != '\n')
                {
                    buffer[index] = read[0];
                    index++;
                }
            } while (read[0] != '\n');

            byte[] ret = new byte[index];

            status = 0;
            bool flag = true;
            for (int i = 0; i < index; i++)
            {
                ret[i] = buffer[i];

                if (ret[i] < '0' || ret[i] > '9')
                {
                    flag = false;
                } else if (flag)
                {
                    status = status * 10 + ret[i] - 48;
                }
            }

            return ret;
        }

        public byte[] Readln()
        {
            int status;
            return Readln(out status);
        }

        private readonly byte[] _bytesCrlf = Encoding.ASCII.GetBytes("\r\n");
        public void Writeln(byte[] bytes)
        {
            _s.Send(bytes);
            _s.Send(_bytesCrlf);
        }

        public void Writeln(string str)
        {
            Writeln(Encoding.UTF8.GetBytes(str));
        }
    }
}