using System.Net.Sockets;
using System.Text;

namespace FTPClient.Helpers
{
    public class SocketHelper
    {
        private Socket s;

        public SocketHelper(Socket s)
        {
            this.s = s;
        }

        public byte[] Readln()
        {
            byte[] buffer = new byte[65536];

            int index = 0;
            byte[] read = new byte[1];
            do
            {
                s.Receive(read);
                if (read[0] != '\r' && read[0] != '\n')
                {
                    buffer[index] = read[0];
                    index++;
                }
            } while (read[0] != '\n');

            byte[] ret = new byte[index];

            for (int i = 0; i < index; i++) ret[i] = buffer[i];

            return ret;
        }

        private readonly byte[] _bytesCrlf = Encoding.ASCII.GetBytes("\r\n");
        public void Writeln(byte[] bytes)
        {
            s.Send(bytes);
            s.Send(_bytesCrlf);
        }

        public void Writeln(string str)
        {
            Writeln(Encoding.UTF8.GetBytes(str));
        }
    }
}