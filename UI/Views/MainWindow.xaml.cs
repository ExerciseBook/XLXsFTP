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

    }
}
