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

using System.ComponentModel;
using System.Net;
using System.Windows;
using UI.FTP;
using UI.Src.Walterlv;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IPEndPoint Server = null;
        public static string Username = null, Password = null;

        public static LocalResourceNavigation GlobalLocalResourceNavigation = null;
        public static RemoteResourceNavigation GlobalRemoteResourceNavigation = null;
        public static TaskList GlobalTaskList = null;
        public static TaskListWorker GlobalTaskListWorker;

        public MainWindow()
        {
            InitializeComponent();
            WindowBlur.SetIsEnabled(this, true);

            MainWindow.GlobalLocalResourceNavigation = CTRLLocalResourceNavigation;
            MainWindow.GlobalRemoteResourceNavigation = CTRLRemoteResourceNavigation;
            MainWindow.GlobalTaskList = CTRLTaskList;

            MainWindow.GlobalTaskListWorker = TaskListWorker.Boot();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            GlobalTaskListWorker.Status = 1;
            GlobalTaskList.sem.Release();

            if (GlobalTaskListWorker.ActivatedTask is TransmitTask aTask)
            {
                aTask.Delete();
            }
        }
    }
}
