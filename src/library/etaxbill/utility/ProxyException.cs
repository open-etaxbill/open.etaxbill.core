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

namespace OdinSdk.eTaxBill.Utility
{
    /// <summary>
    ///
    /// </summary>
    public class ProxyException : Exception
    {
        /// <summary>
        ///
        /// </summary>
        public ProxyException()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public ProxyException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ProxyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}