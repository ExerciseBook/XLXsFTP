using System;
using System.IO;
using System.Windows.Controls;

namespace UI.Views
{
    public class LocalResourceNavigation : ResourceNavigation
    {
        public LocalResourceNavigation() : base()
        {
            NavigationLabel.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        protected override void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String path = NavigationLabel.Text;
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);

                NavigationList.Items.Clear();

                foreach (DirectoryInfo Directory in folder.GetDirectories("*.*"))
                {
                    NavigationList.Items.Add("/" + Directory.Name);
                }

                foreach (FileInfo file in folder.GetFiles("*.*"))
                {
                    NavigationList.Items.Add(file.Name);
                }

            }
            catch (IOException exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }
            catch (ArgumentException exception)
            {
                NavigationList.Items.Clear();

                NavigationList.Items.Add(exception.Message);
            }

        }
    }
}