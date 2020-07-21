using System;
using System.IO;

namespace UI.Views
{
    public class LocalResourceNavigation : ResourceNavigation
    {
        public LocalResourceNavigation() : base()
        {
            NavigationLabel.Content = this.GetType().ToString();


            String path = @"D:\\";
            DirectoryInfo folder = new DirectoryInfo(path);

            foreach (DirectoryInfo Directory in folder.GetDirectories("*.*"))
            {
                NavigationList.Items.Add("/" + Directory.Name);
            }

            foreach (FileInfo file in folder.GetFiles("*.*"))
            {
                NavigationList.Items.Add(file.Name);
            }
        }
    }
}