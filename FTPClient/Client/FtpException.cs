using System;

namespace FTPClient.Client
{
    public class FTPException : Exception
    {
        public FTPException(string s) : base(s)
        {

        }
    }
}