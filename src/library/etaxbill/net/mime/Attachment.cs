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

using System.IO;

namespace OdinSdk.eTaxBill.Net.Mime
{
    /// <summary>
    /// Attachment.
    /// </summary>
    public class Attachment
    {
        private string m_fileName = "";
        private byte[] m_fileData = null;
        private string m_attachmentType = "";

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        public Attachment(string fileName)
        {
            m_attachmentType = "file";
            m_fileName = Path.GetFileName(fileName);

            using (FileStream _fs = File.OpenRead(fileName))
            {
                m_fileData = new byte[_fs.Length];
                _fs.Read(m_fileData, 0, (int)_fs.Length);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="istream"></param>
        public Attachment(string fileName, Stream istream)
        {
            m_attachmentType = "data";
            m_fileName = fileName;

            var _position = (int)istream.Position;
            m_fileData = new byte[istream.Length - _position];

            istream.Read(m_fileData, 0, (int)istream.Length - _position);
            istream.Position = _position;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileData"></param>
        public Attachment(string fileName, byte[] fileData)
        {
            m_attachmentType = "data";
            m_fileName = fileName;
            m_fileData = fileData;
        }

        #region Properties Implementation

        /// <summary>
        /// Gets file name.
        /// </summary>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
        }

        /// <summary>
        /// Gets file data.
        /// </summary>
        public byte[] FileData
        {
            get
            {
                return m_fileData;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string AttachmentType
        {
            get
            {
                return m_attachmentType;
            }
        }

        #endregion Properties Implementation
    }
}