/*
 * This file is part of XLXsFTP
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2020 contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Threading;
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

        public abstract void NavigationRefresh();

        private void NavigationLabel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            NavigationRefresh();
        }

        private void ResourceNavigation_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) this.NavigationRefresh();
        }

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

                int newOpacity = (int) (0xE0 * process) + 0x1F;
                NavigationScrollBar.Background = new SolidColorBrush(Color.FromArgb((byte) newOpacity, 0, 0, 0));
            }

            ;
        }

        private int _scrollStatus = 0;

        private void NavigationScrollBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this._scrollStatus = 0;
        }

        private void NavigationScrollBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._scrollStatus = 1;
            this.Scroll(sender, e);
        }

        private void NavigationScrollBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._scrollStatus == 0) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            this.Scroll(sender, e);
        }

        private void Scroll(object sender, MouseEventArgs e)
        {
            if (NavigationList.Items.Count == 0) return;

            Point p = e.GetPosition(NavigationScrollBarBorder);
            double height = NavigationScrollBarBorder.ActualHeight;

            double precent = p.Y / height;

            int idx = (int) (NavigationList.Items.Count * precent);
            idx = NavigationList.Items.Count == idx ? idx - 1 : idx;
            NavigationList.ScrollIntoView(NavigationList.Items[idx]);
        }

        public abstract void MenuItem_OnClick(object sender, RoutedEventArgs e);

        private static Semaphore Mutex => MainWindow.GlobalTaskList?.mutex;

        private static Semaphore Sem => MainWindow.GlobalTaskList?.sem;

        protected void AddTransmitTask(Direction direction, string localPath, string remotePath, string filename,
            int type)
        {
            if (Mutex == null) return;
            //Mutex.WaitOne();
            MainWindow.GlobalTaskList.ListViewTaskList.Items.Add(
                new TransmitTask(direction, localPath, remotePath, filename, type)
            );
            //Mutex.Release();
            Sem.Release();
        }
    }
}