using System;
using System.IO;
using System.Windows.Controls;

namespace UI.Views
{
    public class LocalResourceNavigation : ResourceNavigation
    {
        public LocalResourceNavigation() : base()
        {
            NavigationLabel.Text = @"D:\";




        }

        protected override void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            String path = NavigationLabel.Text;
            DirectoryInfo folder = new DirectoryInfo(path);

            try
            {
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

        }
    }
}