using System.Net.Sockets;

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
    }
}