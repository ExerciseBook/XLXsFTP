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
using System.Threading;

namespace UI.Helpers
{
    public class MutexFlag
    {
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

        public Boolean Occupied()
        {
            while (!this.TryOccupied()) ;
            return true;
        }
    }
}