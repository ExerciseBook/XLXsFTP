using System;
using System.Net;

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
            var IPE = str.Split(',');
            if (IPE.Length != 6)
                throw new NotImplementedException();
            var ip = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", IPE[0], IPE[1], IPE[2], IPE[3]));
            var port = Convert.ToInt32(IPE[4]) * 256 + Convert.ToInt32(IPE[5]);
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
    }
}