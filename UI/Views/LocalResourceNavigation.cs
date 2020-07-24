using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views
{
    public class LocalResourceNavigation : ResourceNavigation
    {
        public LocalResourceNavigation() : base()
        {
            NavigationLabel.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        protected override void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String path = NavigationLabel.Text;
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);

                NavigationList.Items.Clear();

                NavigationList.Items.Add(new ResourceItem(NavigationLabel, 2, folder.FullName, "../", 0, ""));

                foreach (DirectoryInfo Directory in folder.GetDirectories("*.*"))
                {
                    //NavigationList.Items.Add("/" + Directory.Name);
                    NavigationList.Items.Add(new ResourceItem(NavigationLabel, 1, Directory.FullName, Directory.Name, 0,
                        Helpers.Helper.FileDateTimeFormat(Directory.LastWriteTime)));
                }

                foreach (FileInfo file in folder.GetFiles("*.*"))
                {
                    //NavigationList.Items.Add(file.Name);
                    NavigationList.Items.Add(new ResourceItem(NavigationLabel, 0, file.FullName, file.Name, file.Length,
                        Helpers.Helper.FileDateTimeFormat(file.LastWriteTime)));
                }

            }
            catch (Exception exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }

        }

        public override void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainWindow.GlobalRemoteResourceNavigation.Client == null) return;
            if (!MainWindow.GlobalRemoteResourceNavigation.Client.Connected) return;

            String remotePath = MainWindow.GlobalRemoteResourceNavigation.NavigationLabel.Text;
            if (String.IsNullOrEmpty(remotePath)) remotePath = "/";
            while (remotePath.EndsWith('/')) remotePath = remotePath.Substring(0, remotePath.Length - 1);
            remotePath += '/';


            foreach (var anItem in NavigationList.SelectedItems)
            {
                if (anItem is ResourceItem t)
                {
                    if (t.Type != 0 && t.Type != 1) continue;

                    this.AddToTaskList(t.FilePath, remotePath + t.FileName);
                }
            };
        }



        private void AddToTaskList(string localPath, string remotePath)
        {
            try
            {
                if (File.Exists(localPath))
                {
                    // 是文件
                    FileInfo file = new FileInfo(localPath);

                    AddTransmitTask(Direction.ToRemote, file.FullName, remotePath, file.Name, 0);
                }
                else if (Directory.Exists(localPath))
                {
                    // 是文件夹
                    DirectoryInfo folder = new DirectoryInfo(localPath);

                    foreach (DirectoryInfo Directory in folder.GetDirectories("*.*"))
                    {
                        AddTransmitTask(Direction.ToRemote, Directory.FullName, remotePath + '/' + Directory.Name, Directory.Name, 1);
                        
                        this.AddToTaskList(Directory.FullName, remotePath + '/' + Directory.Name);
                    }

                    foreach (FileInfo file in folder.GetFiles("*.*"))
                    {
                        AddTransmitTask(Direction.ToRemote, file.FullName, remotePath + '/' + file.Name, file.Name, 0);
                    }
                }
            }
            catch (UnauthorizedAccessException excpetion)
            {
                AddTransmitTask(Direction.Null, localPath, null, excpetion.Message, 0);
            }
        }

    }
}