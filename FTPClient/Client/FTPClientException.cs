using System;

namespace FTPClient.Client
{
    public class FTPClientException : Exception
    {
        private readonly int _status;

        public int Ststus
        {
            get => _status;
        }

        public FTPClientException(int status, string line):base(line)
        {
            this._status = status;
        }
    }
} 