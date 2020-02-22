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

namespace OdinSdk.eTaxBill.Net.Dns
{
    #region class MX_Record

    /// <summary>
    /// MX record class.
    /// </summary>
    public class MX_Record
    {
        private int m_Preference = 0;
        private string m_Host = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="preference">MX record preference.</param>
        /// <param name="host">Mail host dns name.</param>
        public MX_Record(int preference, string host)
        {
            m_Preference = preference;
            m_Host = host;
        }

        #region Properties Implementation

        /// <summary>
        /// Gets MX record preference.
        /// </summary>
        public int Preference
        {
            get
            {
                return m_Preference;
            }
        }

        /// <summary>
        /// Gets mail host dns name.
        /// </summary>
        public string Host
        {
            get
            {
                return m_Host;
            }
        }

        #endregion Properties Implementation
    }

    #endregion class MX_Record
}