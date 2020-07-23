using System;
using System.CodeDom;
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
    /// Interaction logic for ResourceItem.xaml
    /// </summary>
    public partial class ResourceItem : UserControl
    {
        private int _type;
        private string _path;
        private string _fileName;
        private long _size;
        private string _modifiedTime;


        public int Type
        {
            get => this._type ;
            private set => this._type = value;
        }

        public string Path
        {
            get => this._path;
            private set => this._path = value;
        }

        public string FileName
        {
            get => this._fileName;
            private set
            {
                this._fileName = value;
                LabelFileName.Content = value;
            }
        }

        public long Size
        {
            get => this._size;
            private set
            {
                this._size = value;
                LabelSize.Content = Helpers.Helper.StrFormatByteSize(value);
            }
        }


        public string ModifiedTime
        {
            get => this._modifiedTime ;
            private set
            {
                this._modifiedTime = value;
                LabelModifiedTime.Content = value;
            }
        }
        
        public ResourceItem(int type, string path, string fileName, long size, string modifiedTime)
        {
            InitializeComponent();

            if (type == 1)
            {
                Grid.SetColumnSpan(GridFileName, 3);
                GridSize.Visibility = Visibility.Hidden;
            }

            this.Type = type;
            this.Path = path;
            this.FileName = fileName;
            this.Size = size;
            this.ModifiedTime = modifiedTime;
        }

    }
}
