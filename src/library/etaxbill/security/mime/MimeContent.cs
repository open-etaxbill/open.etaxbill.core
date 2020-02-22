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
using System;
using System.Collections.Generic;
using System.IO;

namespace OdinSdk.eTaxBill.Security.Mime
{
    /// <summary>
    ///
    /// </summary>
    public class MimeContent
    {
        private int StartPartIndex;

        /// <summary>
        ///
        /// </summary>
        public int StatusCode
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string Boundary
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string Start
        {
            get
            {
                return Parts[StartPartIndex].ContentId;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string StartType
        {
            get
            {
                return Parts[StartPartIndex].ContentType;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ContentType
        {
            get
            {
                if (StartPartIndex < 0)
                    throw new ProxyException("Start-part not set, yet! Please call SetAsStartPart() before querying the content type!");

                return String.Format("multipart/related; type=\"{0}\"; start=\"{1}\"; boundary=\"{2}\"", StartType, Start, Boundary);
            }
        }

        private List<MimePart> _Parts;

        /// <summary>
        ///
        /// </summary>
        public List<MimePart> Parts
        {
            get
            {
                return _Parts;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public MimeContent()
        {
            StartPartIndex = -1;
            _Parts = new List<MimePart>();
            StatusCode = -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="part"></param>
        public void SetAsStartPart(MimePart part)
        {
            if (Parts.Contains(part))
            {
                StartPartIndex = Parts.IndexOf(part);
            }
            else
            {
                throw new ProxyException("Part must be in the list of mime parts before being used as start item!");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="part_id"></param>
        public void SetAsStartPart(string part_id)
        {
            foreach (MimePart _part in this.Parts)
            {
                if (_part.ContentId == part_id)
                {
                    this.StartPartIndex = Parts.IndexOf(_part);
                    break;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetContentAsString()
        {
            var _result = "";

            using (StreamReader _sr = new StreamReader(this.GetContentAsStream()))
                _result = _sr.ReadToEnd();

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetContentAsStream()
        {
            MemoryStream _result = new MemoryStream();

            if (this.Parts != null)
                (new MimeParser()).SerializeMimeContent(this, _result);
            else
                throw new ProxyException("Content is not initialized, no message-part loaded or de-serialized!");

            _result.Seek(0, SeekOrigin.Begin);
            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public byte[] GetContentAsBytes()
        {
            return this.GetContentAsStream().ToArray();
        }
    }
}