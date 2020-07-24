using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for TransmitTask.xaml
    /// </summary>
    public partial class TransmitTask : UserControl
    {
        private Direction _direction = Direction.Null;

        private string _localPath = null;
        private string _remotePath = null;

        private string _fileName = null;

        public TransmitTask(Direction direction, string localPath, string remotePath, string fileName)
        {
            InitializeComponent();

            this._direction = direction;
            this._localPath = localPath;
            this._remotePath = remotePath;
            this._fileName = fileName;

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
    }
    public enum Direction
    {
        Null = 0,
        ToRemote = 1,
        ToLocal = 2
    }
}
