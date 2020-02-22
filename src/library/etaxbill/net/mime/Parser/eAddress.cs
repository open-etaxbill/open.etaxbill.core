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

namespace OdinSdk.eTaxBill.Net.Mime.Parser
{
    /// <summary>
    /// Electronic address.
    /// </summary>
    public class eAddress
    {
        private string m_eAddress = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="address">Electronic address. Eg. "Ivar Lumi" ivar@lumisoft.ee.</param>
        public eAddress(string address)
        {
            m_eAddress = address.Trim();
        }

        #region Properties Implementation

        /// <summary>
        /// Gets email address.
        /// </summary>
        public string Email
        {
            get
            {
                if (m_eAddress.LastIndexOf(" ") > -1)
                {
                    return m_eAddress.Substring(m_eAddress.LastIndexOf(" ") + 1).Replace("<", "").Replace(">", "").Trim();
                }
                return m_eAddress.Replace("<", "").Replace(">", "").Trim();
            }
        }

        /// <summary>
        /// Gets mailbox.
        /// </summary>
        public string Mailbox
        {
            get
            {
                if (this.Email.IndexOf("@") > -1)
                {
                    return this.Email.Substring(0, this.Email.IndexOf("@"));
                }
                return this.Email;
            }
        }

        /// <summary>
        /// Gets domain.
        /// </summary>
        public string Domain
        {
            get
            {
                if (this.Email.IndexOf("@") > -1)
                {
                    return this.Email.Substring(this.Email.IndexOf("@") + 1);
                }
                return "";
            }
        }

        /// <summary>
        /// Gets name
        /// </summary>
        public string Name
        {
            get
            {
                var startIndex = m_eAddress.IndexOf("\"");
                if (startIndex > -1 && m_eAddress.LastIndexOf("\"") > startIndex)
                {
                    return m_eAddress.Substring(startIndex + 1, m_eAddress.LastIndexOf("\"") - startIndex - 1);
                }
                else
                {
                    return "";
                }
            }
        }

        #endregion Properties Implementation
    }
}