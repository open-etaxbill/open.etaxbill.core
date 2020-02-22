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
using System.Runtime.Serialization;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/config/urlhistory/2015/12")]
    [Serializable]
    public class StatUrlWrapper : IDisposable
    {
        public StatUrlWrapper()
        {
        }

        public StatUrlWrapper(StatUrl stat_url)
        {
            Url = stat_url.Url;
            Title = stat_url.Title;

            LastVisited = stat_url.LastVisited;
            LastUpdated = stat_url.LastUpdated;
            Expires = stat_url.Expires;
        }

        #region Properties

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "URL", Order = 0)]
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Title", Order = 1)]
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "LastVisited", Order = 2)]
        public DateTime LastVisited
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "LastUpdated", Order = 3)]
        public DateTime LastUpdated
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Expires", Order = 4)]
        public DateTime Expires
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
        public static List<StatUrlWrapper> GetinstalledSoftware(DateTime today_date)
        {
            List<StatUrlWrapper> _result = new List<StatUrlWrapper>();

            UrlHistoryWrapper _urlHistory = new UrlHistoryWrapper();
            UrlHistoryWrapper.StatUrlEnum _enumerator = _urlHistory.GetEnumerator();

            today_date = new DateTime(today_date.Year, today_date.Month, today_date.Day);

            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current.LastVisited >= today_date)
                {
                    _result.Add(new StatUrlWrapper(_enumerator.Current));
                }
            }
            _enumerator.Reset();

            return _result;
        }

        /*
         * sample
         *
        public void PrintUrlHistory()
        {
            List<StatUrlWrapper> _urlWrapper = StatUrlWrapper.GetinstalledSoftware(CUnixTime.UtcNow);

            var _x = Serialization.SNG.ObjectToXml<StatUrlWrapper[]>(_urlWrapper.ToArray());
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
        ~StatUrlWrapper()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}