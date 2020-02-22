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

namespace OdinSdk.eTaxBill.Net
{
    /// <summary>
    /// SMTP command order validator.
    /// </summary>
    public class CommandValidator
    {
        private bool m_helo_ok = false;
        private bool m_authenticated = false;
        private bool m_mail_from_ok = false;
        private bool m_rcpt_to_ok = false;
        private bool m_bdat_last_ok = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommandValidator()
        {
        }

        #region function Reset

        /// <summary>
        /// Resets state.
        /// </summary>
        public void RESET()
        {
            m_helo_ok = false;
            m_authenticated = false;
            m_mail_from_ok = false;
            m_rcpt_to_ok = false;
            m_bdat_last_ok = false;
        }

        #endregion function Reset

        #region Properties Implementation

        /// <summary>
        /// Gets if may handle MAIL command.
        /// </summary>
        public bool MAY_HANDLE_MAIL
        {
            get
            {
                return HELO_OK && !MAIL_FROM_OK;
            }
        }

        /// <summary>
        /// Gets if may handle RCPT command.
        /// </summary>
        public bool MAY_HANDLE_RCPT
        {
            get
            {
                return MAIL_FROM_OK;
            }
        }

        /// <summary>
        /// Gets if may handle DATA command.
        /// </summary>
        public bool MAY_HANDLE_DATA
        {
            get
            {
                return RCPT_TO_OK;
            }
        }

        /// <summary>
        /// Gets if may handle BDAT command.
        /// </summary>
        public bool MAY_HANDLE_BDAT
        {
            get
            {
                return RCPT_TO_OK && !BDAT_LAST_OK;
            }
        }

        /// <summary>
        /// Gets if may handle AUTH command.
        /// </summary>
        public bool MAY_HANDLE_AUTH
        {
            get
            {
                return !Authenticated;
            }
        }

        /// <summary>
        /// Gest or sets if HELO command handled.
        /// </summary>
        public bool HELO_OK
        {
            get
            {
                return m_helo_ok;
            }

            set
            {
                m_helo_ok = value;
            }
        }

        /// <summary>
        /// Gest or sets if AUTH command handled.
        /// </summary>
        public bool Authenticated
        {
            get
            {
                return m_authenticated;
            }

            set
            {
                m_authenticated = value;
            }
        }

        /// <summary>
        /// Gest or sets if MAIL command handled.
        /// </summary>
        public bool MAIL_FROM_OK
        {
            get
            {
                return m_mail_from_ok;
            }

            set
            {
                m_mail_from_ok = value;
            }
        }

        /// <summary>
        /// Gest or sets if RCPT command handled.
        /// </summary>
        public bool RCPT_TO_OK
        {
            get
            {
                return m_rcpt_to_ok;
            }

            set
            {
                m_rcpt_to_ok = value;
            }
        }

        /// <summary>
        /// Gest or sets if BinaryMime.
        /// </summary>
        public bool BDAT_LAST_OK
        {
            get
            {
                return m_bdat_last_ok;
            }

            set
            {
                m_bdat_last_ok = value;
            }
        }

        #endregion Properties Implementation
    }
}