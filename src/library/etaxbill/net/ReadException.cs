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
    #region public enum ReadReplyCode

    /// <summary>
    /// Reply reading return codes.
    /// </summary>
    public enum ReadReplyCode
    {
        /// <summary>
        /// Read completed successfully.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Read timed out.
        /// </summary>
        TimeOut = 1,

        /// <summary>
        /// Maximum allowed Length exceeded.
        /// </summary>
        LengthExceeded = 2,

        /// <summary>
        /// UnKnown error, eception raised.
        /// </summary>
        UnKnownError = 3,
    }

    #endregion public enum ReadReplyCode

    /// <summary>
    /// Summary description for ReadException.
    /// </summary>
    public class ReadException : System.Exception
    {
        private ReadReplyCode m_ReadReplyCode;

        /// <summary>
        ///
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ReadException(ReadReplyCode code, string message)
            : base(message)
        {
            m_ReadReplyCode = code;
        }

        #region Properties Implementation

        /// <summary>
        /// Gets read error.
        /// </summary>
        public ReadReplyCode ReadReplyCode
        {
            get
            {
                return m_ReadReplyCode;
            }
        }

        #endregion Properties Implementation
    }
}