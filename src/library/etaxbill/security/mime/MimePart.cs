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

using OdinSdk.eTaxBill.Utility;
using System.IO;
using System.Text;

namespace OdinSdk.eTaxBill.Security.Mime
{
    /// <summary>
    ///
    /// </summary>
    public class MimePart
    {
        /// <summary>
        ///
        /// </summary>
        public string ContentType
        {
            get; set;
        }

        /// <summary>
        ///
        /// </summary>
        public string TransferEncoding
        {
            get; set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ContentId
        {
            get; set;
        }

        /// <summary>
        ///
        /// </summary>
        public string CharSet
        {
            get; set;
        }

        private byte[] m_content;

        /// <summary>
        ///
        /// </summary>
        public byte[] Content
        {
            get
            {
                return m_content;
            }
            set
            {
                m_content = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetContentAsString()
        {
            var _result = "";

            using (StreamReader _sr = new StreamReader(GetContentAsStream(), Encoding.UTF8))
            {
                _result = _sr.ReadToEnd();
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetContentAsStream()
        {
            if (this.Content != null)
                return new MemoryStream(this.Content);
            else
                throw new ProxyException("Content is not initialized, no message-part loaded or de-serialized!");
        }
    }
}