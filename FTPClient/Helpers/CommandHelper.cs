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
using System.Net.Sockets;

namespace FTPClient.Helpers
{
    public class CommandHelper
    {
        /// <summary>
        /// 解析 IPE 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IPEndPoint AddressParser(string s)
        {
            var str = GetMiddleText(s, "(", ")");
            var ipe = str.Split(',');
            if (ipe.Length != 6)
                throw new NotImplementedException();
            var ip = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", ipe[0], ipe[1], ipe[2], ipe[3]));
            var port = Convert.ToInt32(ipe[4]) * 256 + Convert.ToInt32(ipe[5]);
            return new IPEndPoint(ip, port);
        }

        /// <summary>
        /// 取文本中间
        /// </summary>
        /// <param name="t"></param>
        /// <param name="k"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private static string GetMiddleText(string t, string k, string j)
        {
            try
            {
                var kn = t.IndexOf(k, StringComparison.Ordinal) + k.Length;
                var jn = t.IndexOf(j, kn, StringComparison.Ordinal);
                return t.Substring(kn, jn - kn);
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 创建数据连接
        /// </summary>
        /// <param name="mode">0:PASV, 1:EPSV</param>
        /// <param name="line"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static Socket AddressParserAndConnect(int mode, string line, IPAddress ip = null)
        {
            if (mode == 0)
            {
                IPEndPoint IPE = AddressParser(line);
                Socket dataSocket = new Socket(IPE.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                dataSocket.Connect(IPE);
                return dataSocket;
            }
            else if (mode == 1)
            {
                int port = Convert.ToInt32(GetMiddleText(line, "|||", "|"));
                IPEndPoint IPE = new IPEndPoint(ip, port);
                Socket dataSocket = new Socket(IPE.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                dataSocket.Connect(IPE);
                return dataSocket;
            }
            else
            {
                throw new Exception("Unknown mode.");
            }
        }
    }
}