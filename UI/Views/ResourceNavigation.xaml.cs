using System.Windows.Controls;

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
    }
}
