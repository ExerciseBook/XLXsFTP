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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for ResourceItem.xaml
    /// </summary>
    public partial class ResourceItem : UserControl
    {
        private string _fileName;
        private long _size;
        private string _modifiedTime;
        private readonly TextBox _navigationLavel;


        public int Type { get; private set; }

        public string FilePath { get; private set; }

        public string FileName
        {
            get => this._fileName;
            private set
            {
                this._fileName = value;
                LabelFileName.Content = value;
            }
        }

        public long Size
        {
            get => this._size;
            private set
            {
                this._size = value;
                LabelSize.Content = Helpers.Helper.StrFormatByteSize(value);
            }
        }


        public string ModifiedTime
        {
            get => this._modifiedTime;
            private set
            {
                this._modifiedTime = value;
                LabelModifiedTime.Content = value;
            }
        }

        public ResourceItem(TextBox navigationLabel, int type, string filePath, string fileName, long size,
            string modifiedTime)
        {
            InitializeComponent();

            switch (type)
            {
                case 1:
                    Grid.SetColumnSpan(GridFileName, 3);
                    GridSize.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    GridSize.Visibility = Visibility.Hidden;
                    GridModifiedTime.Visibility = Visibility.Hidden;
                    break;
                case 4:
                    GridSize.Visibility = Visibility.Hidden;
                    GridModifiedTime.Visibility = Visibility.Hidden;
                    break;
            }

            this._navigationLavel = navigationLabel;
            this.Type = type;
            this.FilePath = filePath;
            this.FileName = fileName;
            this.Size = size;
            this.ModifiedTime = modifiedTime;

            this.MouseDoubleClick += ResourceItem_MouseDoubleClick;
        }

        private void ResourceItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch (this.Type)
            {
                case 1:
                    this._navigationLavel.Text = this.FilePath;
                    break;
                case 2:
                    try
                    {
                        this._navigationLavel.Text = (System.IO.Directory.GetParent(this.FilePath))?.FullName;
                    }
                    catch (Exception exception)
                    {
                        // ignore
                    }

                    break;
                case 4:
                    string nowPath = this.FilePath;

                    while (nowPath.EndsWith('/')) nowPath = nowPath.Substring(0, nowPath.Length - 1);
                    if (String.IsNullOrEmpty(nowPath)) break;

                    int aidx = nowPath.LastIndexOf('/');
                    int bidx = nowPath.LastIndexOf('\\');
                    int idx = Math.Min(aidx == -1 ? Int32.MaxValue : aidx, bidx == -1 ? Int32.MaxValue : bidx);
                    string newPath = nowPath.Substring(0, idx);
                    if (String.IsNullOrEmpty(newPath)) newPath = "/";
                    this._navigationLavel.Text = newPath;
                    break;
            }
        }
    }
}