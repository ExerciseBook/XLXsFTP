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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for TaskList.xaml
    /// </summary>
    public partial class TaskList : UserControl
    {
        public Semaphore sem = new Semaphore(0, Int32.MaxValue);

        public Semaphore mutex = new Semaphore(1, 1);

        public TaskList()
        {
            InitializeComponent();
        }

        private void TaskList_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double process = 1.0;
            double dVer = e.VerticalOffset;
            double dViewport = e.ViewportHeight;
            double dExtent = e.ExtentHeight;

            process = dVer / (dExtent - dViewport);

            if (dVer == 0 && dViewport == dExtent && process >= 1)
            {
                TaskListScrollBar.Visibility = Visibility.Hidden;
            }
            else
            {
                TaskListScrollBar.Visibility = Visibility.Visible;

                double newHight = TaskListScrollBarBorder.ActualHeight > 32
                    ? (TaskListScrollBarBorder.ActualHeight - 16) * process + 16
                    : TaskListScrollBarBorder.ActualHeight * process;
                TaskListScrollBar.Height = newHight;

                int newOpacity = (int) (0xE0 * process) + 0x1F;
                TaskListScrollBar.Background = new SolidColorBrush(Color.FromArgb((byte) newOpacity, 0, 0, 0));
            }

            ;
        }

        private int _scrollStatus = 0;

        private void TaskListScrollBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this._scrollStatus = 0;
        }

        private void TaskListScrollBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._scrollStatus = 1;
            this.Scroll(sender, e);
        }

        private void TaskListScrollBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._scrollStatus == 0) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            this.Scroll(sender, e);
        }

        private void Scroll(object sender, MouseEventArgs e)
        {
            if (ListViewTaskList.Items.Count == 0) return;

            Point p = e.GetPosition(TaskListScrollBarBorder);
            double height = TaskListScrollBarBorder.ActualHeight;

            double precent = p.Y / height;

            int idx = (int) (ListViewTaskList.Items.Count * precent);
            idx = ListViewTaskList.Items.Count == idx ? idx - 1 : idx;
            ListViewTaskList.ScrollIntoView(ListViewTaskList.Items[idx]);
        }


        private void TaskList_Delete(object sender, RoutedEventArgs e)
        {
            if (this.mutex == null) return;
            //Mutex.WaitOne();

            List<TransmitTask> deleteList = new List<TransmitTask>();

            foreach (var anItem in ListViewTaskList.SelectedItems)
            {
                if (anItem is TransmitTask t)
                {
                    /* if (t.CanDelete) */
                    deleteList.Add(t);
                }
            }

            ;

            int n = deleteList.Count;
            for (int i = 0; i < n; i++) deleteList[i].Delete();

            //Mutex.Release();
        }
    }
}