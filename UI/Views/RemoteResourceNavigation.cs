using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FTPClient.Client;
using FileInfo = FTPClient.Client.FileInfo;
using Path = System.IO.Path;

namespace UI.Views
{
    public class RemoteResourceNavigation : ResourceNavigation
    {
        public Client Client { get; private set; }

        private TextBox _addressBox;
        private LoginView _loginView;

        public RemoteResourceNavigation() : base()
        {
            Grid top = (Grid)NavigationLabel.Parent;
            Grid bottom = (Grid)NavigationList.Parent;

            // 隐藏地址栏
            NavigationLabel.Visibility = Visibility.Hidden;

            // 建立登陆状态栏
            this._addressBox = new TextBox();
            this._addressBox.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            this._addressBox.BorderThickness = new Thickness(0);
            this._addressBox.VerticalAlignment = VerticalAlignment.Bottom;
            this._addressBox.KeyDown += AddressBox_KeyDown;

            top.Children.Add(this._addressBox);

            // 建立框
            this._loginView = new LoginView(this, this._addressBox);
            bottom.Children.Add(this._loginView);

            // 写入默认地址
            this._addressBox.TextChanged += this._loginView.InputSyncToBottom;
            this._addressBox.Text = "ftp://anonymous:anonymous%40example.com@127.0.0.1/";
        }

        public void AddressBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: this.Connect(); break;
            }
        }


        public void Connect()
        {
            try
            {
                String rawaddress = this._addressBox.Text;
                String username, password, defaultPath, host;
                UInt16 port;

                Helpers.Helper.ParseAddress(rawaddress, out host, out port, out username, out password, out defaultPath);
                IPEndPoint server = Helpers.Helper.ParseIPEndPoint(host, port);
                Client = new Client(server, username, password);
                Client.Connect();

                MainWindow.Server = server;
                MainWindow.Username = username;
                MainWindow.Password = password;

                this._addressBox.Visibility = Visibility.Hidden;
                NavigationLabel.Visibility = Visibility.Visible;
                this._loginView.Visibility = Visibility.Hidden;

                NavigationLabel.Text = defaultPath;
            }
            catch (SocketException exception)
            {
                NavigationList.Items.Clear();
                NavigationList.Items.Add(exception.Message);
            }
            catch (FTPClientException exception)
            {
                NavigationList.Items.Clear();
                NavigationList.Items.Add(exception.Message);
            }
            catch (Exception exception)
            {
                NavigationList.Items.Clear();
                NavigationList.Items.Add(exception.GetType().ToString());
                NavigationList.Items.Add(exception.Message);
            }

        }
        protected override void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String path = NavigationLabel.Text;
            if (String.IsNullOrEmpty(path)) path = "/";
            if (path[^1] != '/') path += '/';

            try
            {
                List<FileInfo> t = Client.List(path);

                NavigationList.Items.Clear();

                NavigationList.Items.Add(new ResourceItem(NavigationLabel, 4, path, "../", 0, ""));

                foreach (FileInfo fileInfo in t)
                {
                    NavigationList.Items.Add(new ResourceItem(
                        NavigationLabel,
                        fileInfo.IsFolder ? 1 : 0, 
                        path + fileInfo.FileName,
                        fileInfo.FileName,
                        fileInfo.Size,
                        fileInfo.ModifiedAt.ToString()
                        ));

                    //NavigationList.Items.Add((fileInfo.IsFolder ? "/" : "") + fileInfo.FileName);
                }
            }
            catch (SocketException exception)
            {
                NavigationLabel.Visibility = Visibility.Hidden;
                this._addressBox.Visibility = Visibility.Visible;
                this._loginView.Visibility = Visibility.Visible;

                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }
            catch (FTPClientException exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }
            catch (Exception exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.GetType().ToString());
                NavigationList.Items.Add(exception.Message);
            }

        }

        public override void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            string localPath = MainWindow.GlobalLocalResourceNavigation.NavigationLabel.Text;

            String remotePath = NavigationLabel.Text;
            if (String.IsNullOrEmpty(remotePath)) remotePath = "/";
            while (remotePath.EndsWith('/')) remotePath = remotePath.Substring(0, remotePath.Length - 1);
            remotePath += '/';

            foreach (var anItem in NavigationList.SelectedItems)
            {
                if (anItem is ResourceItem t)
                {
                    if (t.Type != 0 && t.Type != 1) continue;
                    string name = t.FilePath;
                    while (name.StartsWith('/')) name = name.Substring(1);

                    this.AddToTaskList(Path.Join(localPath, name), remotePath + name, t.Type == 1);
                }
            };
        }

        private void AddToTaskList(string localPath, string remotePath, bool isFolder)
        {
            try
            {
                List<FileInfo> t = Client.List(remotePath);

                foreach (FileInfo fileInfo in t)
                {
                    if (fileInfo.IsFolder)
                    {
                        AddTransmitTask(Direction.ToLocal, Path.Join(localPath, fileInfo.FileName),
                            remotePath + '/' + fileInfo.FileName, fileInfo.FileName, 1);

                        AddToTaskList(Path.Join(localPath, fileInfo.FileName), remotePath + '/' + fileInfo.FileName, fileInfo.IsFolder);
                    }
                    else
                    {
                        if (isFolder)
                        {
                            AddTransmitTask(Direction.ToLocal, Path.Join(localPath, fileInfo.FileName),
                                remotePath + '/' + fileInfo.FileName, fileInfo.FileName, 0);
                        }
                        else
                        {
                            AddTransmitTask(Direction.ToLocal, localPath, remotePath, fileInfo.FileName, 0);
                        }
                    }
                }
            }
            catch (Exception excpetion)
            {
                AddTransmitTask(Direction.Null, localPath, null, excpetion.Message, 0);
            }
        }
    }

}