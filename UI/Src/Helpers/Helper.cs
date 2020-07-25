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
using System.Runtime.InteropServices;
using System.Text;

namespace UI.Helpers
{
    public class Helper
    {
        public static void ParseAddress(string rawAddress, out string host, out UInt16 port, out string username, out string password, out string defaultPath)
        {
            string lowerAddress = rawAddress.ToLower();
            if (!lowerAddress.StartsWith("ftp://")) throw new ArgumentException("Not start with \"ftp://\"");

            /**
             * 对于 URL 支持
             *
             * ftp://用户名:密码@目标服务器/默认路径
             * ftp://用户名@目标服务器/默认路径
             * ftp://目标服务器/默认路径
             *
             */
            string subAddress = rawAddress.Substring(6);
            string authorizationAddress;
            if (subAddress.Contains('/'))
            {
                string[] slicedAddress = subAddress.Split('/', 2, StringSplitOptions.None);
                defaultPath = slicedAddress[1];
                authorizationAddress = slicedAddress[0];
            }
            else
            {
                defaultPath = "/";
                authorizationAddress = subAddress;
            }

            if (authorizationAddress.Contains('@'))
            {
                string[] slicedAddress = authorizationAddress.Split('@', 2, StringSplitOptions.None);
                Helper.ParseHost(slicedAddress[1], out host, out port);

                string[] authorization = slicedAddress[0].Split(":");

                switch (authorization.Length)
                {
                    case 2:
                        username = authorization[0];
                        password = authorization[1];
                        break;
                    case 1:
                        username = authorization[0];
                        password = username + "@example.com";
                        break;
                    default:
                        throw new ArgumentException("Authorization information format error.");
                }
            }
            else
            {
                Helper.ParseHost(authorizationAddress, out host, out port);
                username = "anonymous";
                password = "anonymous@example.com";
            }

            if (defaultPath == null || defaultPath == "") defaultPath = "/";

        }

        private static void ParseHost(string address, out string host, out UInt16 port)
        {
            if (address.StartsWith('['))
            {
                string[] addr = address.Split("]:");

                switch (addr.Length)
                {
                    case 2:
                        host = addr[0] + "]";
                        port = UInt16.Parse(addr[1]);
                        break;
                    case 1:
                        host = addr[0];
                        port = 21;
                        break;
                    default: throw new ArgumentException("Host format error");
                }
            }
            else
            {
                string[] addr = address.Split(':');

                if (addr.Length > 2) throw new ArgumentException("Host format error");

                host = addr[0];
                port = addr.Length == 2 ? UInt16.Parse(addr[1]) : (UInt16)21;
            }
        }

        public static IPEndPoint ParseIPEndPoint(string host, ushort port)
        {
            IPEndPoint ret = null;

            try
            {
                ret = IPEndPoint.Parse(host);
                ret.Port = port;
            }
            catch (Exception e)
            {
                // ignored
            }

            if (ret != null) return ret;

            try
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(host);

                foreach (IPAddress addr in addresslist)
                {
                    ret = new IPEndPoint(addr, port);
                }
            }
            catch (Exception e)
            {
                // ignore
            }

            if (ret != null) return ret;

            throw new ArgumentException("IPEndPoint parsing failed.");
        }


        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(
            long fileSize
            , [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer
            , int bufferSize);


        /// <summary>
        /// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
        /// </summary>
        /// <param name="filelength">The numeric value to be converted.</param>
        /// <returns>the converted string</returns>
        public static string StrFormatByteSize(long filesize)
        {
            StringBuilder sb = new StringBuilder(11);
            StrFormatByteSize(filesize, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string FileDateTimeFormat(DateTime t)
        {
            return t.Year == DateTime.Now.Year
                ? t.ToString("MMMM dd HH:mm")
                : t.ToString("yyyy-M-d dddd");
        }

    }
}