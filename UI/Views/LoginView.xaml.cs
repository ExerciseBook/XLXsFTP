using System;
using System.Threading;
using System.Web;
using System.Windows.Controls;
using UI.Helpers;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private readonly TextBox _addressBox;
        private readonly RemoteResourceNavigation _remoteResourceNavigation;

        public LoginView(RemoteResourceNavigation remoteResourceNavigation, TextBox addressBox)
        {
            InitializeComponent();

            this._addressBox = addressBox;
            this._remoteResourceNavigation = remoteResourceNavigation;

            TextBoxHost.KeyDown += this._remoteResourceNavigation.AddressBox_KeyDown;
            TextBoxUsername.KeyDown += this._remoteResourceNavigation.AddressBox_KeyDown;
            TextBoxPassword.KeyDown += this._remoteResourceNavigation.AddressBox_KeyDown;
        }

        private readonly MutexFlag _occupyFlag = new MutexFlag();

        public void InputSyncToTop(object sender, TextChangedEventArgs e)
        {
            if (this._occupyFlag.TryOccupied())
            {
                _addressBox.Text = "ftp://";
                if (!String.IsNullOrEmpty(TextBoxUsername.Text)) _addressBox.Text += HttpUtility.UrlEncode(TextBoxUsername.Text);
                if (!String.IsNullOrEmpty(TextBoxPassword.Text)) _addressBox.Text += ":" + HttpUtility.UrlEncode(TextBoxPassword.Text);
                if (!String.IsNullOrEmpty(TextBoxPassword.Text) || !String.IsNullOrEmpty(TextBoxUsername.Text)) _addressBox.Text += "@";
                if (!String.IsNullOrEmpty(TextBoxHost.Text)) _addressBox.Text += TextBoxHost.Text;
                if (!String.IsNullOrEmpty(TextBoxPath.Text)) _addressBox.Text += 
                    TextBoxPath.Text.StartsWith('/') ? TextBoxPath.Text : "/" + TextBoxPath.Text;

                this._occupyFlag.ReleaseOccupation();
            }
        }

        public void InputSyncToBottom(object sender, TextChangedEventArgs e)
        {
            if (this._occupyFlag.TryOccupied())
            {
                try
                {
                    String rawaddress = this._addressBox.Text;
                    String username, password, defaultPath, host;
                    UInt16 port;

                    Helpers.Helper.ParseAddress(rawaddress, out host, out port, out username, out password, out defaultPath);

                    TextBoxUsername.Text = HttpUtility.UrlDecode(username);
                    TextBoxPassword.Text = HttpUtility.UrlDecode(password);
                    TextBoxPath.Text = defaultPath;
                    TextBoxHost.Text = host + (port == 21 ? "" : ":" + port);
                }
                catch (Exception exception)
                {
                    // ignore
                }

                this._occupyFlag.ReleaseOccupation();
            }
        }
    }
}
