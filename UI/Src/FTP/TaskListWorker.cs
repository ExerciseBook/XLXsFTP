using System;
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

        private TaskListWorker()
        {
            Thread childThread = new Thread(Run);
            childThread.Start();
        }

        Semaphore sem = new Semaphore(0, Int32.MaxValue);
        Semaphore mutex = new Semaphore(1, 1);

        public int ReleaseOne()
        {
            return sem.Release(1);
        }

        private void Run()
        {
            while (true) { 

                mutex.WaitOne();
                sem.WaitOne();

                object t = MainWindow.GlobalTaskList.ListViewTaskList.Items[0];
                if (t is TransmitTask aTask)
                {
                    
                }

                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(t);
                    mutex.Release();
                });

            }
        }

}
}