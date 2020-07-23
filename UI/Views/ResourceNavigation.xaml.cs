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

            process = dVer / (dExtent - dViewport);
            
            if (dVer == 0 && dViewport == dExtent && process >= 1)
            {
                NavigationScrollBar.Visibility = Visibility.Hidden;
            }
            else
            {
                NavigationScrollBar.Visibility = Visibility.Visible;

                double newHight = NavigationScrollBarBorder.ActualHeight > 32
                    ? (NavigationScrollBarBorder.ActualHeight - 16) * process + 16
                    : NavigationScrollBarBorder.ActualHeight * process;
                NavigationScrollBar.Height = newHight;

                int newOpacity = (int)(0xE0 * process) + 0x1F;
                NavigationScrollBar.Background = new SolidColorBrush(Color.FromArgb((byte)newOpacity, 0, 0, 0));
            };

        }

        private void NavigationScrollBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (NavigationList.Items.Count == 0) return;

            Point p = e.GetPosition(NavigationScrollBarBorder);
            double height = NavigationScrollBarBorder.ActualHeight;

            double precent = p.Y / height;

            int idx = (int) (NavigationList.Items.Count * precent);
            idx = NavigationList.Items.Count == idx ? idx - 1 : idx;
            NavigationList.ScrollIntoView(NavigationList.Items[idx]);

        }
    }
}
