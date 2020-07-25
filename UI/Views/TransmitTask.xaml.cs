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
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FTPClient.Client;
using UI.Helpers;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for TransmitTask.xaml
    /// </summary>
    public partial class TransmitTask : UserControl
    {
        private readonly Direction _direction = Direction.Null;

        private readonly string _localPath = null;
        private readonly string _remotePath = null;

        private readonly string _fileName = null;

        /// <summary>
        /// 0 文件
        /// 1 文件夹
        /// </summary>
        private readonly int _type = -1;

        private int _status = 0;

        Client client = null;

        public TransmitTask(Direction direction, string localPath, string remotePath, string fileName, int type)
        {
            InitializeComponent();

            this._direction = direction;
            this._localPath = localPath;
            this._remotePath = remotePath;
            this._fileName = fileName;
            this._type = type;

            this._status = 0;

            switch (direction)
            {
                case Direction.ToRemote :
                    ProcessBar.HorizontalAlignment = HorizontalAlignment.Left;
                    ProcessBar.Width = 0;
                    LabelFileName.Content = '→';
                    break;
                case Direction.ToLocal :
                    ProcessBar.HorizontalAlignment = HorizontalAlignment.Right;
                    ProcessBar.Width = 0;
                    LabelFileName.Content = '←';
                    break;
                case Direction.DeleteRemote :
                case Direction.DeleteLocal :
                    LabelFileName.Content = '×';
                    break;
            }

            LabelFileName.Content += this._fileName;
        }

        private readonly MutexFlag _occupyFlag = new MutexFlag();

        public void Execute()
        {
            this._occupyFlag.Occupied();

            try
            {
                if (this._status != 0) return;
                this._status = 1;

                if (this._direction == Direction.ToRemote || this._direction == Direction.ToLocal || this._direction == Direction.DeleteRemote)
                {
                    client = new Client(MainWindow.Server, MainWindow.Username, MainWindow.Password);
                    client.Connect();
                    client.ProcessUpdate += UpdateProcess;
                }

                switch (this._direction)
                {
                    case Direction.ToRemote:
                    {
                        if (this._type == 0)
                        {
                            client?.Upload(this._localPath, this._remotePath, 0);
                        }
                        else if (this._type == 1)
                        {
                            client?.CreateDirectory(this._remotePath);
                        }

                        break;
                    }

                    case Direction.ToLocal:
                    {
                        if (this._type == 0)
                        {
                            client?.Download(this._localPath, this._remotePath, 0);
                        }
                        else if (this._type == 1)
                        {
                            // 判断本地目录是否存在
                            if (!Directory.Exists(this._localPath))
                            {
                                // 创建目录
                                Directory.CreateDirectory(this._localPath);
                            }
                        }

                        break;
                    }

                    case Direction.DeleteRemote:
                    {
                        if (this._type == 0)
                        {
                            this.client.Delete(this._remotePath);
                        }
                        else if (this._type == 1)
                        {
                            this.client.DeleteDirectory(this._remotePath);
                        }
                        break;
                        }

                    case Direction.DeleteLocal:
                    {
                        if (this._type == 0)
                        {
                            if (File.Exists(this._localPath)) File.Delete(this._localPath);
                        }
                        else if (this._type == 1)
                        {
                            if (Directory.Exists(this._localPath)) Directory.Delete(this._localPath);
                        }
                        break;
                    }
                }

                if (this._direction == Direction.ToRemote || this._direction == Direction.ToLocal || this._direction == Direction.DeleteRemote)
                {
                    if (client != null)
                    {
                        client.ProcessUpdate -= UpdateProcess;
                        client.Disconnect();
                    }
                }
            }
            catch (Exception exception)
            {
                Application.Current.Dispatcher.Invoke(() => { LabelFileName.Content += exception.Message; });
            }

            this._occupyFlag.ReleaseOccupation();
        }

        public void UpdateProcess(long done, long total)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // SolidColorBrush first = Brushes.Blue;
                SolidColorBrush first = new SolidColorBrush(Color.FromArgb(0x7F, 0x00, 0x00, 0xFF));

                SolidColorBrush second = Brushes.Aqua;

                double process = (double) done / total;

                SolidColorBrush result = this.MixColor(second, first, process);

                ProcessBar.Background = result;
                ProcessBar.Width = ProcessBarBorder.ActualWidth * process;
            });
        }

        private SolidColorBrush MixColor(SolidColorBrush colorA, SolidColorBrush colorB, double aValue)
        {
            byte a, r, g, b;
            double bValue = 1 - aValue;

            a = (byte) (colorA.Color.A * aValue + colorB.Color.A * bValue);
            r = (byte) (colorA.Color.R * aValue + colorB.Color.R * bValue);
            g = (byte) (colorA.Color.G * aValue + colorB.Color.G * bValue);
            b = (byte) (colorA.Color.B * aValue + colorB.Color.B * bValue);

            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        private static Semaphore Mutex => MainWindow.GlobalTaskList?.mutex;

        private static Semaphore Sem => MainWindow.GlobalTaskList?.sem;


        public bool CanDelete => this._status == 0;

        public void Delete()
        {
            // if (!this._occupyFlag.TryOccupied()) return;
            if (this._status == 0)
            {
                Sem.WaitOne();
                MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(this);
            }
            else
            {
                this.client?.TerminateTransmissionTask();
                MainWindow.GlobalTaskList.ListViewTaskList.Items.Remove(this);
            }
        }
    }

    public enum Direction
    {
        Null = 0,
        ToRemote = 1,
        ToLocal = 2,
        DeleteRemote = 3,
        DeleteLocal = 4
    }
}