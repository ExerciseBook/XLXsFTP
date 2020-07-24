using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FTPClient.Client;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for TransmitTask.xaml
    /// </summary>
    public partial class TransmitTask : UserControl
    {
        private readonly Direction _direction = Direction.Null;

        private readonly string _localPath = null;
        private readonly string _remotePath = null;

        private readonly string _fileName = null;

        private int _status = 0;

        public TransmitTask(Direction direction, string localPath, string remotePath, string fileName)
        {
            InitializeComponent();

            this._direction = direction;
            this._localPath = localPath;
            this._remotePath = remotePath;
            this._fileName = fileName;

            this._status = 0;
            
            switch (direction)
            {
                case Direction.ToRemote :
                    LabelFileName.Content = '→';
                    break;
                case Direction.ToLocal :
                    LabelFileName.Content = '←';
                    break;
            }

            LabelFileName.Content += this._fileName;
        }

        public void Execute()
        {
            try
            {
                this._status = 1;

                Client client = null;

                if (this._direction != Direction.Null)
                {
                    client = new Client(MainWindow.Server, MainWindow.Username, MainWindow.Password);
                    client.Connect();
                }

                switch (this._direction)
                {
                    case Direction.ToRemote:
                        client?.Upload(this._localPath, this._remotePath, 0);
                        break;
                    case Direction.ToLocal:
                        client?.Download(this._localPath, this._remotePath, 0);
                        break;
                }

            }
            catch (Exception exception)
            {
                LabelFileName.Content += exception.Message;
            }

        }

        private static Semaphore Mutex => MainWindow.GlobalTaskList?.mutex;
        
        private static Semaphore Sem => MainWindow.GlobalTaskList?.sem;


        public bool CanDelete => this._status == 0;

        public void Delete()
        {
            if (this._status == 0)
            {
                Sem.WaitOne();
                MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(this);
            }
        }

    }
    public enum Direction
    {
        Null = 0,
        ToRemote = 1,
        ToLocal = 2
    }
}
