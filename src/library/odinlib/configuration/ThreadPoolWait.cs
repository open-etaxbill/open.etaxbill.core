/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    public class ThreadPoolWait : IDisposable
    {
        public static bool WaitForAll(ThreadPoolWait[] events)
        {
            var result = false;

            if (events != null)
            {
                Array.ForEach(events, _event => _event.WaitOne());
                result = true;
            }

            return result;
        }

        private int _remainingWorkItems = 1;
        private ManualResetEvent _done = new ManualResetEvent(false);

        public void QueueUserWorkItem(WaitCallback callback)
        {
            QueueUserWorkItem(callback, null);
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            ThrowIfDisposed();

            QueuedCallback qc = new QueuedCallback();
            qc.Callback = callback;
            qc.State = state;

            Interlocked.Increment(ref _remainingWorkItems);
            ThreadPool.QueueUserWorkItem(HandleWorkItem, qc);
        }

        public bool WaitOne()
        {
            return WaitOne(-1, false);
        }

        public bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            return WaitOne((int)timeout.TotalMilliseconds, exitContext);
        }

        public bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            ThrowIfDisposed();
            DoneWorkItem(); // to offset initial value of _remainingWorkItems

            var rv = _done.WaitOne(millisecondsTimeout, exitContext);
            if (rv)
            {
                _remainingWorkItems = 1;
                _done.Reset();
            }
            else
                Interlocked.Increment(ref _remainingWorkItems);

            return rv;
        }

        private void HandleWorkItem(object state)
        {
            QueuedCallback qc = (QueuedCallback)state;

            try
            {
                qc.Callback(qc.State);
            }
            finally
            {
                DoneWorkItem();
            }
        }

        private void DoneWorkItem()
        {
            if (Interlocked.Decrement(ref _remainingWorkItems) == 0)
                _done.Set();
        }

        private class QueuedCallback
        {
            public WaitCallback Callback;
            public object State;
        }

        private void ThrowIfDisposed()
        {
            if (_done == null)
                throw new ObjectDisposedException(GetType().Name);
        }

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.
                if (_done != null)
                {
                    ((IDisposable)_done).Dispose();
                    _done = null;
                }

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~ThreadPoolWait()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}