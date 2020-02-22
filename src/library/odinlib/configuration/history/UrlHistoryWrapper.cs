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

using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// The class that wraps the C# equivalence of the IURLHistory Interface (in the file "urlhist.cs")
    /// </summary>
    public class UrlHistoryWrapper
    {
        private UrlHistoryClass m_urlHistory;
        private IUrlHistoryStg2 m_historyStg2;

        /// <summary>
        /// Default constructor for UrlHistoryWrapperClass
        /// </summary>
        public UrlHistoryWrapper()
        {
            m_urlHistory = new UrlHistoryClass();
            m_historyStg2 = (IUrlHistoryStg2)m_urlHistory;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Marshal.ReleaseComObject(m_historyStg2);
            m_urlHistory = null;
        }

        /// <summary>
        /// Places the specified URL into the history. If the URL does not exist in the history, an entry is created in the history. If the URL does exist in the history, it is overwritten.
        /// </summary>
        /// <param name="pocsUrl">the string of the URL to place in the history</param>
        /// <param name="pocsTitle">the string of the title associated with that URL</param>
        /// <param name="dwFlags">the flag which indicate where a URL is placed in the history.
        /// <example><c>ADDURL_FLAG.ADDURL_ADDTOHISTORYANDCACHE</c></example>
        /// </param>
        public void AddHistoryEntry(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags)
        {
            m_historyStg2.AddUrl(pocsUrl, pocsTitle, dwFlags);
        }

        /// <summary>
        /// Deletes all instances of the specified URL from the history. does not work!
        /// </summary>
        /// <param name="pocsUrl">the string of the URL to delete.</param>
        /// <param name="dwFlags"><c>dwFlags = 0</c></param>
		public void DeleteHistoryEntry(string pocsUrl, int dwFlags)
        {
            m_historyStg2.DeleteUrl(pocsUrl, dwFlags);
        }

        /// <summary>
        ///Queries the history and reports whether the URL passed as the pocsUrl parameter has been visited by the current user.
        /// </summary>
        /// <param name="pocsUrl">the string of the URL to querythe string of the URL to query.</param>
        /// <param name="dwFlags">STATURL_QUERYFLAGS Enumeration
        /// <example><c>STATURL_QUERYFLAGS.STATURL_QUERYFLAG_TOPLEVEL</c></example></param>
        /// <returns>Returns STATURL structure that received additional URL history information. If the returned  STATURL's pwcsUrl is not null, Queried URL has been visited by the current user.
        /// </returns>
        public StatUrl QueryUrl(string pocsUrl, STATURL_QUERYFLAGS dwFlags)
        {
            StatUrl lpSTATURL = new StatUrl();

            try
            {
                //In this case, queried URL has been visited by the current user.
                m_historyStg2.QueryUrl(pocsUrl, dwFlags, ref lpSTATURL);
                //lpSTATURL.pwcsUrl is NOT null;
                return lpSTATURL;
            }
            catch (FileNotFoundException)
            {
                //Queried URL has not been visited by the current user.
                //lpSTATURL.pwcsUrl is set to null;
                return lpSTATURL;
            }
        }

        /// <summary>
        /// Delete all the history except today's history, and Temporary Internet Files.
        /// </summary>
        public void ClearHistory()
        {
            m_historyStg2.ClearHistory();
        }

        /// <summary>
        /// Create an enumerator that can iterate through the history cache. UrlHistoryWrapperClass does not implement IEnumerable interface
        /// </summary>
        /// <returns>Returns STATURLEnumerator object that can iterate through the history cache.</returns>
        public StatUrlEnum GetEnumerator()
        {
            return new StatUrlEnum((IEnumSTATURL)m_historyStg2.EnumUrls);
        }

        /// <summary>
        /// The inner class that can iterate through the history cache. STATURLEnumerator does not implement IEnumerator interface.
        /// The items in the history cache changes often, and enumerator needs to reflect the data as it existed at a specific point in time.
        /// </summary>
        public class StatUrlEnum
        {
            private IEnumSTATURL m_enumrator;
            private int m_index;
            private StatUrl m_statUrl;

            /// <summary>
            /// Constructor for <c>STATURLEnumerator</c> that accepts IEnumSTATURL object that represents the <c>IEnumSTATURL</c> COM Interface.
            /// </summary>
            /// <param name="enumrator">the <c>IEnumSTATURL</c> COM Interface</param>
            public StatUrlEnum(IEnumSTATURL enumrator)
            {
                this.m_enumrator = enumrator;
            }

            /// <summary>
            /// Advances the enumerator to the next item of the url history cache.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element;
            ///  false if the enumerator has passed the end of the url history cache.
            ///  </returns>
            public bool MoveNext()
            {
                m_statUrl = new StatUrl();

                m_enumrator.Next(1, ref m_statUrl, out m_index);
                if (m_index == 0)
                    return false;
                else
                    return true;
            }

            /// <summary>
            /// Gets the current item in the url history cache.
            /// </summary>
            public StatUrl Current
            {
                get
                {
                    return m_statUrl;
                }
            }

            /// <summary>
            /// Skips a specified number of Call objects in the enumeration sequence. does not work!
            /// </summary>
            /// <param name="celt"></param>
            public void Skip(int celt)
            {
                m_enumrator.Skip(celt);
            }

            /// <summary>
            /// Resets the enumerator interface so that it begins enumerating at the beginning of the history.
            /// </summary>
            public void Reset()
            {
                m_enumrator.Reset();
            }

            /// <summary>
            /// Creates a duplicate enumerator containing the same enumeration state as the current one. does not work!
            /// </summary>
            /// <returns>duplicate STATURLEnumerator object</returns>
            public StatUrlEnum Clone()
            {
                IEnumSTATURL ppenum;

                m_enumrator.Clone(out ppenum);

                return new StatUrlEnum(ppenum);
            }

            /// <summary>
            /// Define filter for enumeration. MoveNext() compares the specified URL with each URL in the history list to find matches. MoveNext() then copies the list of matches to a buffer. SetFilter method is used to specify the URL to compare.
            /// </summary>
            /// <param name="poszFilter">The string of the filter.
            /// <example>SetFilter('http://', STATURL_QUERYFLAGS.STATURL_QUERYFLAG_TOPLEVEL)  retrieves only entries starting with 'http.//'. </example>
            /// </param>
            /// <param name="dwFlags">STATURL_QUERYFLAGS Enumeration<exapmle><c>STATURL_QUERYFLAGS.STATURL_QUERYFLAG_TOPLEVEL</c></exapmle></param>
            public void SetFilter(string poszFilter, STATURLFLAGS dwFlags)
            {
                m_enumrator.SetFilter(poszFilter, dwFlags);
            }

            /// <summary>
            ///Enumerate the items in the history cache and store them in the IList object.
            /// </summary>
            /// <param name="list">IList object
            /// <example><c>ArrayList</c>object</example>
            /// </param>
            public void GetUrlHistory(IList list)
            {
                while (true)
                {
                    m_statUrl = new StatUrl();

                    m_enumrator.Next(1, ref m_statUrl, out m_index);
                    if (m_index == 0)
                        break;

                    list.Add(m_statUrl);
                }

                m_enumrator.Reset();
            }
        }
    }
}