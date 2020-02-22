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

namespace OdinSdk.eTaxBill.Net.Smtp
{
    /// <summary>
    /// Holds body(mime) type.
    /// </summary>
    public enum BodyType
    {
        /// <summary>
        /// ASCII body.
        /// </summary>
        x7_bit = 1,

        /// <summary>
        /// ANSI body.
        /// </summary>
        x8_bit = 2,

        /// <summary>
        /// Binary body.
        /// </summary>
        binary = 4,
    }
}