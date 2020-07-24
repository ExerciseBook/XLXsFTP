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

            string remotePath = MainWindow.GlobalRemoteResourceNavigation.NavigationLabel.Text;

            foreach (var anItem in NavigationList.SelectedItems)
            {
                if (anItem is ResourceItem t)
                {
                    this.AddToTaskList(t.FilePath, remotePath);
                }
            };
        }

        private void AddToTaskList(string localPath,string remotePath)
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(localPath);

                foreach (DirectoryInfo Directory in folder.GetDirectories("*.*"))
                {
                    this.AddToTaskList(Directory.FullName, remotePath + '/' + Directory.Name);
                }

                foreach (FileInfo file in folder.GetFiles("*.*"))
                {
                    MainWindow.GlobalTaskList.ListViewTaskList.Items.Add(
                        new TransmitTask(Direction.ToRemote, file.FullName, remotePath + '/' + file.Name, file.Name)
                    );
                    MainWindow.GlobalTaskListWorker.ReleaseOne();
                }
            }
            catch (UnauthorizedAccessException excpetion)
            {
                MainWindow.GlobalTaskList.ListViewTaskList.Items.Add(
                    new TransmitTask(Direction.Null, localPath, null, excpetion.Message)
                );
                MainWindow.GlobalTaskListWorker.ReleaseOne();
            }
        }

    }
}