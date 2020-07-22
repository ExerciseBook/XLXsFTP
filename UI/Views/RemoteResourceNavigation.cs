using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Controls;
using FTPClient.Client;
using FTPClient.Helpers;
using FileInfo = FTPClient.Client.FileInfo;

namespace UI.Views
{
    public class RemoteResourceNavigation : ResourceNavigation
    {
        private Client client;

        public RemoteResourceNavigation() : base()
        {
            IPEndPoint server = CommandHelper.AddressParser("(127,0,0,1,0,21)");
            client = new Client(server, "anonymous", "anonymous@example.com");
            client.Connect();

            NavigationLabel.Text = "/";
        }

        protected override void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String Path = NavigationLabel.Text;
            
            try
            {
                List<FileInfo> t = client.List(Path);

                NavigationList.Items.Clear();

                foreach (FileInfo fileInfo in t)
                {
                    NavigationList.Items.Add((fileInfo.IsFolder ? "/" : "") + fileInfo.FileName);
                }
            }
            catch (Exception exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }

        }
    }
}