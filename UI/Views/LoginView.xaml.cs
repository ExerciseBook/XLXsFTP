using System;
using System.Threading;
using System.Web;
using System.Windows.Controls;

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

            TextBoxPassword.KeyDown += this._remoteResourceNavigation.AddressBox_KeyDown;
        }

        /// <summary>
        /// 被占用？
        /// </summary>
        private int _isOccupied = 0;

        /// <summary>
        /// 申请占用
        /// </summary>
        /// <returns>true 申请占用成功，false 申请占用失败</returns>
        public Boolean TryOccupied()
        {
            int t = 1;
            int flag = Interlocked.Exchange(ref this._isOccupied, t);
            if (flag == 1) return false;
            return true;
        }

        /// <summary>
        /// 释放占用
        /// </summary>
        /// <returns></returns>
        public Boolean ReleaseOccupation()
        {
            int t = 0;
            int flag = Interlocked.Exchange(ref this._isOccupied, t);
            if (flag == 1) return true;
            return false;
        }

        public void InputSyncToTop(object sender, TextChangedEventArgs e)
        {
            if (this.TryOccupied())
            {
                _addressBox.Text = "ftp://";
                if (!String.IsNullOrEmpty(TextBoxUsername.Text)) _addressBox.Text += HttpUtility.UrlEncode(TextBoxUsername.Text);
                if (!String.IsNullOrEmpty(TextBoxPassword.Text)) _addressBox.Text += ":" + HttpUtility.UrlEncode(TextBoxPassword.Text);
                if (!String.IsNullOrEmpty(TextBoxPassword.Text) || !String.IsNullOrEmpty(TextBoxUsername.Text)) _addressBox.Text += "@";
                if (!String.IsNullOrEmpty(TextBoxHost.Text)) _addressBox.Text += TextBoxHost.Text;
                if (!String.IsNullOrEmpty(TextBoxPath.Text)) _addressBox.Text += 
                    TextBoxPath.Text.StartsWith('/') ? TextBoxPath.Text : "/" + TextBoxPath.Text;

                this.ReleaseOccupation();
            }
        }

        public void InputSyncToBottom(object sender, TextChangedEventArgs e)
        {
            if (this.TryOccupied())
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

                this.ReleaseOccupation();
            }
        }
    }
}
