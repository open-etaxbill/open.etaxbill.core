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
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// Some Win32Api Pinvoke.
    /// </summary>
    public class Win32api
    {
        /// <summary>
        /// Used by CannonializeURL method.
        /// </summary>
        [Flags]
        public enum ShlWapi_URL : uint
        {
            /// <summary>
            /// Treat "/./" and "/../" in a URL string as literal characters, not as shorthand for navigation.
            /// </summary>
            URL_DONT_SIMPLIFY = 0x08000000,

            /// <summary>
            /// Convert any occurrence of "%" to its escape sequence.
            /// </summary>
            URL_ESCAPE_PERCENT = 0x00001000,

            /// <summary>
            /// Replace only spaces with escape sequences. This flag takes precedence over URL_ESCAPE_UNSAFE, but does not apply to opaque URLs.
            /// </summary>
            URL_ESCAPE_SPACES_ONLY = 0x04000000,

            /// <summary>
            /// Replace unsafe characters with their escape sequences. Unsafe characters are those characters that may be altered during transport across the Internet, and include the (&lt;, &gt;, ", #, {, }, |, \, ^, ~, [, ], and ') characters. This flag applies to all URLs, including opaque URLs.
            /// </summary>
            URL_ESCAPE_UNSAFE = 0x20000000,

            /// <summary>
            /// Combine URLs with client-defined pluggable protocols, according to the World Wide Web Consortium (W3C) specification. This flag does not apply to standard protocols such as ftp, http, gopher, and so on. If this flag is set, UrlCombine does not simplify URLs, so there is no need to also set URL_DONT_SIMPLIFY.
            /// </summary>
            URL_PLUGGABLE_PROTOCOL = 0x40000000,

            /// <summary>
            /// Un-escape any escape sequences that the URLs contain, with two exceptions. The escape sequences for "?" and "#" are not un-escaped. If one of the URL_ESCAPE_XXX flags is also set, the two URLs are first un-escaped, then combined, then escaped.
            /// </summary>
            URL_UNESCAPE = 0x10000000
        }

        [DllImport("shlwapi.dll")]
        public static extern int UrlCanonicalize(
            string pszUrl,
            StringBuilder pszCanonicalized,
            ref int pcchCanonicalized,
            ShlWapi_URL dwFlags
            );

        /// <summary>
        /// Takes a URL string and converts it into canonical form
        /// </summary>
        /// <param name="pszUrl">URL string</param>
        /// <param name="dwFlags">shlwapi_URL Enumeration. Flags that specify how the URL is converted to canonical form.</param>
        /// <returns>The converted URL</returns>
        public static string CannonializeURL(string pszUrl, ShlWapi_URL dwFlags)
        {
            StringBuilder _buffer = new StringBuilder(260);

            var s = _buffer.Capacity;
            var c = UrlCanonicalize(pszUrl, _buffer, ref s, dwFlags);
            if (c != 0)
            {
                _buffer.Capacity = s;
                c = UrlCanonicalize(pszUrl, _buffer, ref s, dwFlags);
            }

            return _buffer.ToString();
        }

        public struct SYSTEMTIME
        {
            public Int16 Year;
            public Int16 Month;
            public Int16 DayOfWeek;
            public Int16 Day;
            public Int16 Hour;
            public Int16 Minute;
            public Int16 Second;
            public Int16 Milliseconds;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FileTimeToSystemTime(ref System.Runtime.InteropServices.ComTypes.FILETIME FileTime, ref SYSTEMTIME SystemTime);

        /// <summary>
        /// Converts a file time to DateTime format.
        /// </summary>
        /// <param name="filetime">FILETIME structure</param>
        /// <returns>DateTime structure</returns>
        public static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            FileTimeToSystemTime(ref filetime, ref st);
            return new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemTimeToFileTime([In] ref SYSTEMTIME lpSystemTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime);

        /// <summary>
        /// Converts a DateTime to file time format.
        /// </summary>
        /// <param name="datetime">DateTime structure</param>
        /// <returns>FILETIME structure</returns>
        public static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime datetime)
        {
            SYSTEMTIME _st = new SYSTEMTIME()
            {
                Year = (short)datetime.Year,
                Month = (short)datetime.Month,
                Day = (short)datetime.Day,
                Hour = (short)datetime.Hour,
                Minute = (short)datetime.Minute,
                Second = (short)datetime.Second,
                Milliseconds = (short)datetime.Millisecond
            };

            System.Runtime.InteropServices.ComTypes.FILETIME filetime;
            SystemTimeToFileTime(ref _st, out filetime);

            return filetime;
        }

        //compares two file times.
        [DllImport("kernel32.dll")]
        public static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        //Retrieves information about an object in the file system.
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public const uint SHGFI_ATTR_SPECIFIED = 0x20000;
        public const uint SHGFI_ATTRIBUTES = 0x800;
        public const uint SHGFI_PIDL = 0x8;
        public const uint SHGFI_DISPLAYNAME = 0x200;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        public const uint FILE_ATTRIBUTRE_NORMAL = 0x4000;
        public const uint SHGFI_EXETYPE = 0x2000;
        public const uint SHGFI_SYSICONINDEX = 0x4000;
        public const uint ILC_COLORDDB = 0x1;
        public const uint ILC_MASK = 0x0;
        public const uint ILD_TRANSPARENT = 0x1;
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SHELLICONSIZE = 0x4;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_ICONLOCATION = 0x1000;
    }

    /// <summary>
    /// Contains information about a file object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    /// <summary>
    /// The helper class to sort in ascending order by FileTime(LastVisited).
    /// </summary>
    public class SortFileTimeAscendingHelper : IComparer
    {
        [DllImport("kernel32.dll")]
        private static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        int IComparer.Compare(object a, object b)
        {
            StatUrl c1 = (StatUrl)a;
            StatUrl c2 = (StatUrl)b;

            return (CompareFileTime(ref c1.ftLastVisited, ref c2.ftLastVisited));
        }

        public static IComparer SortFileTimeAscending()
        {
            return (IComparer)new SortFileTimeAscendingHelper();
        }
    }

    /// <summary>
    /// The helper class to sort in descending order by FileTime(LastVisited).
    /// </summary>
    public class SortFileTimeDescendingHelper : IComparer
    {
        [DllImport("kernel32.dll")]
        private static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        int IComparer.Compare(object a, object b)
        {
            StatUrl c1 = (StatUrl)a;
            StatUrl c2 = (StatUrl)b;

            return (CompareFileTime(ref c2.ftLastVisited, ref c1.ftLastVisited));
        }

        public static IComparer SortFileTimeDescending()
        {
            return (IComparer)new SortFileTimeDescendingHelper();
        }
    }
}