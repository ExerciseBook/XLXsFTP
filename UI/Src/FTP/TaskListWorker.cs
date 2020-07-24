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
                if (t is TransmitTask aTask)
                {
                    aTask.Execute();
                }

                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(t);
                    Mutex.Release();
                });

            }
        }

}
}