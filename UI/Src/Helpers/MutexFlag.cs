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