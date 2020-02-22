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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/config/process_wrapper/2015/12")]
    [Serializable]
    public class ProcessWrapper : IDisposable
    {
        public ProcessWrapper()
        {
        }

        public ProcessWrapper(Process process)
        {
            ProcessName = process.ProcessName;
            MainWindowTitle = process.MainWindowTitle;

            StartTime = process.StartTime;
            PrivateMemorySize64 = process.PrivateMemorySize64;
            WorkingSet64 = process.WorkingSet64;
        }

        #region Properties

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "ProcessName", Order = 0)]
        public string ProcessName
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "MainWindowTitle", Order = 1)]
        public string MainWindowTitle
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "StartTime", Order = 2)]
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "PrivateMemorySize64", Order = 3)]
        public long PrivateMemorySize64
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "WorkingSet64", Order = 4)]
        public long WorkingSet64
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a list of browser history
        /// </summary>
        /// <returns></returns>
        public static List<ProcessWrapper> GetApplications()
        {
            List<ProcessWrapper> _result = new List<ProcessWrapper>();

            IEnumerable<Process> _processes =
                from _p in Process.GetProcesses()
                orderby _p.ProcessName descending
                select _p;

            foreach (Process _p in _processes)
            {
                if (_p.MainWindowTitle.Length > 0 || _p.MainWindowHandle.ToInt32() > 0)
                    _result.Add(new ProcessWrapper(_p));
            }

            return _result;
        }

        /*
         * sample
         *
            public void PrintProcessList()
            {
                List<ProcessWrapper> _processWrapper = ProcessWrapper.GetApplications();

                var _x = Serialization.SNG.ObjectToXml<ProcessWrapper[]>(_processWrapper.ToArray());
                Console.WriteLine(_x);
            }
         */

        #endregion Methods

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

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~ProcessWrapper()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}