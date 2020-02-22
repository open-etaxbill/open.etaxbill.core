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

using System;
using System.Text;

namespace OdinSdk.eTaxBill.Net.Core
{
    /// <summary>
    /// Summary description for _FixedStack.
    /// </summary>
    public class FixedStack
    {
        private byte[] m_SackList = null;
        private byte[] m_TerminaTor = null;

        /// <summary>
        /// Terninator holder and checker stack.
        /// </summary>
        /// <param name="terminator"></param>
        public FixedStack(string terminator)
        {
            //	m_SackList = new ArrayList();
            m_TerminaTor = Encoding.UTF8.GetBytes(terminator);
            m_SackList = new byte[m_TerminaTor.Length];

            // Init empty array
            for (int i = 0; i < m_TerminaTor.Length; i++)
            {
                m_SackList[i] = (byte)0;
                //	m_SackList.Add((byte)0);
            }
        }

        #region function Push

        /// <summary>
        /// Pushes new bytes to stack.(Last in, first out).
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="count">Count to push from bytes parameter</param>
        /// <returns>Returns number of bytes may be pushed next push.
        /// NOTE: returns 0 if stack contains terminator.
        /// </returns>
        public int Push(byte[] bytes, int count)
        {
            if (bytes.Length > m_TerminaTor.Length)
            {
                throw new Exception("bytes.Length is too big, can't be more than terminator.length !");
            }

            // Move stack bytes which will stay and append new ones
            Array.Copy(m_SackList, count, m_SackList, 0, m_SackList.Length - count);
            Array.Copy(bytes, 0, m_SackList, m_SackList.Length - count, count);

            var index = Array.IndexOf(m_SackList, m_TerminaTor[0]);
            if (index > -1)
            {
                if (index == 0)
                {
                    // Check if contains full terminator
                    for (int i = 0; i < m_SackList.Length; i++)
                    {
                        if ((byte)m_SackList[i] != m_TerminaTor[i])
                        {
                            return 1;
                        }
                    }
                    return 0; // If reaches so far, contains terminator
                }

                return 1;
            }
            else
            {
                return m_TerminaTor.Length;
            }

            // Last in, first out
            //	m_SackList.AddRange(bytes);
            //	m_SackList.RemoveRange(0,bytes.Length);

            //	if(m_SackList.Contains(m_TerminaTor[0])){
            //		return 1;
            //	}
            //	else{
            //		return m_TerminaTor.Length;
            //	}
        }

        #endregion function Push

        #region function ContainsTerminator

        /// <summary>
        /// Check if stack contains terminator.
        /// </summary>
        /// <returns></returns>
        public bool ContainsTerminator()
        {
            for (int i = 0; i < m_SackList.Length; i++)
            {
                if ((byte)m_SackList[i] != m_TerminaTor[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion function ContainsTerminator

        #region Properties Implementation

        /*	/// <summary>
		///
		/// </summary>
		public int Count
		{
			get{ return m_SackList.Count; }
		}*/

        #endregion Properties Implementation
    }
}