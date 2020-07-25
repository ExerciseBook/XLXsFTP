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

using System.Threading;
using System.Windows;
using UI.Views;

namespace UI.FTP
{
    public class TaskListWorker
    {
        public static TaskListWorker Boot()
        {
            return new TaskListWorker();
        }

        public Thread WorkerThread;

        private TaskListWorker()
        {
            WorkerThread = new Thread(new ThreadStart(Run));
            WorkerThread.Start();
        }

        private static Semaphore Mutex => MainWindow.GlobalTaskList?.mutex;

        private static Semaphore Sem => MainWindow.GlobalTaskList?.sem;

        public int Status = 0;

        private void Run()
        {
            while (Status == 0)
            {
                if (Mutex == null) continue;

                Sem.WaitOne();
                if (Status != 0) return;
                Mutex.WaitOne();

                object t = MainWindow.GlobalTaskList.ListViewTaskList.Items[0];
                this.ActivatedTask = t;
                if (t is TransmitTask aTask)
                {
                    aTask.Execute();
                }

                Thread.Sleep(100);
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(t);
                    if (MainWindow.GlobalTaskList.ListViewTaskList.Items.Count == 0)
                    {
                        MainWindow.GlobalRemoteResourceNavigation.NavigationRefresh();
                        MainWindow.GlobalLocalResourceNavigation.NavigationRefresh();
                    }
                    Mutex.Release();
                });

            }
        }

        public object ActivatedTask { get; set; }
    }
}