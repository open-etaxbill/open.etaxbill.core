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

namespace OdinSdk.eTaxBill.Net.Mime
{
    /// <summary>
    /// Attachments collection.
    /// </summary>
    public class Attachments : ArrayList
    {
        /// <summary>
        ///
        /// </summary>
        public Attachments()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public int Add(Attachment attachment)
        {
            return base.Add(attachment);
        }

        /// <summary>
        ///
        /// </summary>
        public new Attachment this[int nIndex]
        {
            get
            {
                return (Attachment)base[nIndex];
            }

            set
            {
                base[nIndex] = value;
            }
        }
    }
}