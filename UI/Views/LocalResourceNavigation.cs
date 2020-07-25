/*
 * This file is part of XLXsFTP
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2020 contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.IO;
using System.Windows;

namespace UI.Views
{
    public class LocalResourceNavigation : ResourceNavigation
    {
        public int Status { get; private set; } = -1;

        public LocalResourceNavigation() : base()
        {
            NavigationLabel.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            NavigationName.Content = "Local";
        }

        public override void NavigationRefresh()
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

                this.Status = 0;
            }
            catch (Exception exception)
            {
                this.Status = -1;

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
            }

            ;
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
                        AddTransmitTask(Direction.ToRemote, Directory.FullName, remotePath + '/' + Directory.Name,
                            Directory.Name, 1);

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