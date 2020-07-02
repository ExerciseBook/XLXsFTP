using System;
using System.Net;

namespace FTPClient.Helpers
{
    public class CommandHelper
    {
        public static IPEndPoint AddressParser(string s)
        {
            var str = GetMiddleText(s, "(", ")");
            var IPE = str.Split(',');
            if (IPE.Length != 6)
                throw new NotImplementedException();
            var ip = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", IPE[0], IPE[1], IPE[2], IPE[3]));
            var port = Convert.ToInt32(IPE[4]) * 256 + Convert.ToInt32(IPE[5]);
            return new IPEndPoint(ip, port);
        }

        private static string GetMiddleText(string t, string k, string j) //取出中间文本
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
    }
}