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