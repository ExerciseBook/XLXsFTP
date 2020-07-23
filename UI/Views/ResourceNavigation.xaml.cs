using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for ResourceNavigation.xaml
    /// </summary>
    public abstract partial class ResourceNavigation : UserControl
    {
        public ResourceNavigation()
        {
            InitializeComponent();
        }

        protected abstract void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e);

        private void NavigationList_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double process = 1.0;
            double dVer = e.VerticalOffset;
            double dViewport = e.ViewportHeight;
            double dExtent = e.ExtentHeight;

            process = (dVer + dViewport) / dExtent;


            if (dVer == 0 && dViewport == dExtent && process >= 1)
            {
                NavigationScrollBar.Visibility = Visibility.Hidden;
            }
            else
            {
                NavigationScrollBar.Visibility = Visibility.Visible;
            };

            NavigationScrollBar.Height = NavigationScrollBarBorder.ActualHeight * process;
            NavigationScrollBar.Background = new SolidColorBrush(Color.FromArgb((byte)(0xFF * process), 0, 0, 0));
        }

        private void NavigationScrollBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (NavigationList.Items.Count == 0) return;

            Point p = e.GetPosition(NavigationScrollBarBorder);
            double height = NavigationScrollBarBorder.ActualHeight;

            double precent = p.Y / height;

            NavigationList.ScrollIntoView(NavigationList.Items[ (int)(NavigationList.Items.Count * precent) ]);
        }
    }
}
