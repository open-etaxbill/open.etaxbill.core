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

namespace OdinSdk.eTaxBill.Net.Smtp.Client
{
    /// <summary>
    /// SMTP error types.
    /// </summary>
    public enum SmtpErrorType
    {
        /// <summary>
        /// Connection related error.
        /// </summary>
        ConnectionError = 1,

        /// <summary>
        /// Email address doesn't exist.
        /// </summary>
        InvalidEmailAddress = 2,

        //	MessageSizeExceeded = 3,

        /// <summary>
        /// Some feature isn't supported.
        /// </summary>
        NotSupported = 4,

        /// <summary>
        /// Unknown error.
        /// </summary>
        UnKnown = 256,
    }

    /// <summary>
    /// This class holds smtp error info.
    /// </summary>
    public class SmtpError
    {
        private SmtpErrorType m_errorType = SmtpErrorType.UnKnown;
        private string[] m_affectedEmails = null;
        private string m_errorText = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="affectedEmails"></param>
        /// <param name="errorText"></param>
        public SmtpError(SmtpErrorType errorType, string[] affectedEmails, string errorText)
        {
            m_errorType = errorType;
            m_affectedEmails = affectedEmails;
            m_errorText = errorText;
        }

        #region Properties Implementation

        /// <summary>
        /// Gets SMTP error type.
        /// </summary>
        public SmtpErrorType ErrorType
        {
            get
            {
                return m_errorType;
            }
        }

        /// <summary>
        /// Gets list of email addresses which are affected by this error.
        /// </summary>
        public string[] AffectedEmails
        {
            get
            {
                return m_affectedEmails;
            }
        }

        /// <summary>
        /// Gets additional error text.
        /// </summary>
        public string ErrorText
        {
            get
            {
                return m_errorText;
            }
        }

        #endregion Properties Implementation
    }
}