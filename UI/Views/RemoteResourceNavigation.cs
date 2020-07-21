using System.Collections.Generic;
using System.Net;
using FTPClient.Client;
using FTPClient.Helpers;

namespace UI.Views
{
    public class RemoteResourceNavigation : ResourceNavigation
    {
        private Client client;

        public RemoteResourceNavigation() : base()
        {
            NavigationLabel.Content = this.GetType().ToString();
            
            IPEndPoint server = CommandHelper.AddressParser("(127,0,0,1,0,21)");
            client = new Client(server, "anonymous", "anonymous@example.com");
            client.Connect();

            List<FileInfo> t = client.List("/");
            foreach (FileInfo fileInfo in t)
            {
                NavigationList.Items.Add( (fileInfo.IsFolder ? "/" : "") + fileInfo.FileName);
            }

        }

    }
}