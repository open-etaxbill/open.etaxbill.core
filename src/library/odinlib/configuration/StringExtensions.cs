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
using System.Text.RegularExpressions;

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="toSearch"></param>
        /// <param name="toFind"></param>
        /// <returns></returns>
        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(
                                @"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z",
                                RegexOptions.Singleline
                            )
                .IsMatch(toSearch);
        }

        /// <summary>
        /// 연-월-일(yyyy-MM-dd) 형식으로 변환 합니다.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime d)
        {
            return d.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 연월일(yyyyMMdd) 형식으로 변환 합니다.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDateString2(this DateTime d)
        {
            return d.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 연-월-일 시:분:초(yyyy-MM-dd HH:mm:ss) 형식으로 변환 합니다.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime d)
        {
            return d.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 연-월-일T시:분:초.Zone(yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK) 형식으로 변환 합니다.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDateTimeZoneString(this DateTime d)
        {
            return d.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
        }

        /// <summary>
        /// 연-월-일 시:분(yyyy-MM-dd HH:mm) 형식으로 변환 합니다.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string ToDateHHmmString(this DateTime d)
        {
            return d.ToString("yyyy-MM-dd HH:mm");
        }
    }
}