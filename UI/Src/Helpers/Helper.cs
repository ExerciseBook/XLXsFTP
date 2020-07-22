using System;
using System.Net;

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

                if (addr.Length > 2) throw new ArgumentException("Host format error");

                host = addr[0] + "]";
                port = addr.Length == 2 ? UInt16.Parse(addr[1]) : (UInt16)21;
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
    }
}