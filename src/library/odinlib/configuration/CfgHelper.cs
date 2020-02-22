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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    public class CfgHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        /// <summary></summary>
        private CfgHelper()
        {
            m_ip_addresses = new Lazy<string[]>(GetIPAddresses);
            m_ip_address = new Lazy<string>(GetIPAddress);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<CfgHelper> m_lzayHelper = new Lazy<CfgHelper>(() =>
        {
            return new CfgHelper();
        });

        /// <summary></summary>
        public static CfgHelper SNG
        {
            get
            {
                return m_lzayHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        public object GetAppValue(string appkey, object default_value = null)
        {
            var _result = (object)null;

            if (String.IsNullOrEmpty(appkey) == false)
                _result = ConfigurationManager.AppSettings[appkey];

            if (_result == null)
                _result = default_value;

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        public string GetAppString(string appkey, string default_value = "")
        {
            return GetAppValue(appkey, default_value).ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        public bool GetAppBoolean(string appkey, bool default_value = false)
        {
            return Convert.ToBoolean(GetAppValue(appkey, default_value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        public int GetAppInteger(string appkey, int default_value = 0)
        {
            return Convert.ToInt32(GetAppValue(appkey, default_value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string[] GetIPAddresses()
        {
            return GetIPAddresses(Dns.GetHostName());
        }

        public string[] GetIPAddresses(string host_name)
        {
            var _result = new string[0];

            if (IsValidIP(host_name) == false)
            {
                var _ip_address = Dns.GetHostEntry(host_name).AddressList;
                _result = new string[_ip_address.Length];

                var i = 0;
                Array.ForEach(_ip_address, _ip =>
                {
                    if (_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        _result[i++] = _ip.ToString();
                });

                Array.Resize(ref _result, i);
            }
            else
            {
                _result = new string[] { host_name };
            }

            return _result;
        }

        public string GetIPAddress()
        {
            return GetIPAddress(Dns.GetHostName());
        }

        public string GetIPAddress(string host_name)
        {
            var _result = host_name;

            if (IsValidIP(host_name) == false)
            {
                if (String.IsNullOrEmpty(host_name) == true || host_name.ToLower() == "localhost")
                    host_name = Dns.GetHostName();

                var _ipv4_address = new List<string>();
                {
                    var _ip_address = Dns.GetHostEntry(host_name).AddressList;
                    foreach (IPAddress _ip in _ip_address)
                    {
                        if (_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            _ipv4_address.Add(_ip.ToString());
                    }
                }

                _ipv4_address.Sort();
                _result = _ipv4_address[0];
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public string GetLocalIpAddress(string host_name)
        {
            return IsLocalMachine(host_name) == true ? IPAddress : host_name;
        }

        /// <summary>
        /// method to validate an IP address
        /// using regular expressions. The pattern
        /// being used will validate an ip address
        /// with the range of 1.0.0.0 to 255.255.255.255
        /// </summary>
        /// <param name="ip_address">Address to validate</param>
        /// <returns></returns>
        public bool IsValidIP(string ip_address)
        {
            //create our match pattern
            const string _pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

            //create our Regular Expression object
            Regex _regCheck = new Regex(_pattern);

            //boolean variable to hold the status
            var _result = false;

            //check to make sure an ip address was provided
            if (ip_address == "")
            {
                //no address provided so return false
                _result = false;
            }
            else
            {
                //address provided so use the IsMatch Method
                //of the Regular Expression object
                _result = _regCheck.IsMatch(ip_address, 0);
            }

            //return the results
            return _result;
        }

        public bool IsLocalMachine(string host_name)
        {
            var _result = false;

            if (String.IsNullOrEmpty(host_name) == true ||
                host_name.ToLower() == "localhost" || host_name == "127.0.0.1" ||
                host_name.ToLower() == MachineName || host_name == "::1"
                )
            {
                _result = true;
            }
            else
            {
                string[] _serverIPs = GetIPAddresses(host_name);
                if (_serverIPs.Length > 0)
                {
                    string[] _ip_address = GetIPAddresses();
                    foreach (string _ip in _ip_address)
                    {
                        if (_ip == _serverIPs[0])
                        {
                            _result = true;
                            break;
                        }
                    }
                }
            }

            return _result;
        }

        public bool IsSameMachine(string local_host_name, string remote_host_name)
        {
            var _result = false;

            if (local_host_name == remote_host_name)
            {
                _result = true;
            }
            else
            {
                string[] _localIPs = GetIPAddresses(local_host_name);
                if (_localIPs.Length > 0)
                {
                    string[] _remoteIPs = GetIPAddresses(remote_host_name);
                    foreach (string _ip in _remoteIPs)
                    {
                        if (_ip == _localIPs[0])
                        {
                            _result = true;
                            break;
                        }
                    }
                }
            }

            return _result;
        }

        private Lazy<string> m_machine = new Lazy<string>(() => Environment.MachineName.ToLower());

        public string MachineName
        {
            get
            {
                return m_machine.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string UserDomainName
        {
            get
            {
                return Environment.UserDomainName.ToLower();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string UserName
        {
            get
            {
                return Environment.UserName.ToLower();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsWindows
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT;
            }
        }

        private Lazy<string[]> m_ip_addresses;

        public string[] IPAddresses
        {
            get
            {
                return m_ip_addresses.Value;
            }
        }

        private Lazy<string> m_ip_address;

        public string IPAddress
        {
            get
            {
                return m_ip_address.Value;
            }
        }

        private string __macAddress;

        /// <summary>
        ///
        /// </summary>
        public string MacAddress
        {
            get
            {
                if (__macAddress == null)
                    __macAddress = GetMacAddress();

                return __macAddress;
            }
        }

        private bool? m_trace_mode = null;

        /// <summary>
        ///
        /// </summary>
        public bool TraceMode
        {
            get
            {
                if (m_trace_mode == null)
                    m_trace_mode = GetAppString("TraceMode") == "true";

                return m_trace_mode.Value;
            }
        }

        private bool? m_debug_mode = null;

        /// <summary>
        ///
        /// </summary>
        public bool DebugMode
        {
            get
            {
                if (m_debug_mode == null)
                    m_debug_mode = GetAppString("DebugMode") == "true";

                return m_debug_mode.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="assembly_names"></param>
        /// <returns></returns>
        public string GetVersion(string assembly_names)
        {
            string[] _splitNames = assembly_names.Split(',');
            var _version = "";

            foreach (string f in _splitNames)
            {
                if (f.Trim().StartsWith("Version="))
                    _version = f.Trim().Replace("Version=", "");
            }

            return _version;
        }

        public string GetPublicKeyToken(string assembly_names)
        {
            string[] _splitNames = assembly_names.Split(',');
            var _keyToken = "";

            foreach (string f in _splitNames)
            {
                if (f.Trim().StartsWith("PublicKeyToken="))
                    _keyToken = f.Trim().Replace("PublicKeyToken=", "").ToLower();
            }

            return _keyToken;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string GetCommonFolder()
        {
            string _result
                    = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)
                    + @"\OdinSoft\";

            if (System.IO.Directory.Exists(_result) == false)
                System.IO.Directory.CreateDirectory(_result);

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public string GetWorkingFolder(string folder)
        {
            var _result = GetCommonFolder();

            if (String.IsNullOrEmpty(folder) == false)
                _result += folder + @"\";

            if (System.IO.Directory.Exists(_result) == false)
                System.IO.Directory.CreateDirectory(_result);

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public string GetWorkingFolder(string category_id, string product_id, string folder)
        {
            var _result = Path.Combine(GetCommonFolder(), category_id, product_id);

            if (String.IsNullOrEmpty(folder) == false)
                _result += folder + @"\";

            if (System.IO.Directory.Exists(_result) == false)
                System.IO.Directory.CreateDirectory(_result);

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            var _nics = NetworkInterface.GetAllNetworkInterfaces();

            var _macAddress = "";
            foreach (NetworkInterface _adapter in _nics)
            {
                // only return MAC Address from first card
                if (String.IsNullOrEmpty(_macAddress) == true)
                {
                    //IPInterfaceProperties properties = adapter.GetIPProperties(); Line is not required
                    _macAddress = _adapter.GetPhysicalAddress().ToString();
                }
            }

            return _macAddress;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // 영어(미국), 일본어(일본), 중국어(중국), 중국어(간체), 중국어(번체), 한국어(대한민국), 태국어(태국). 필리핀어(필리핀)
        //-----------------------------------------------------------------------------------------------------------------------------
        private ArrayList m_culturelist = new ArrayList(new string[] { "ko-kr", "en-us", "ja-jp", "zh-cn", "zh-chs", "zh-cht", "th-th", "fil-ph" });

        public string GetDefaultCulture
        {
            get
            {
                return m_culturelist[0].ToString();
            }
        }

        public bool ContainsCultureName(string culture)
        {
            return m_culturelist.Contains(culture.ToLower());
        }

        public string GetCultureName(string culture)
        {
            var _result = culture;

            for (int i = 0; i < m_culturelist.Count; i++)
            {
                if (_result.ToLower().Equals(m_culturelist[i].ToString()) == true)
                    return _result;
            }

            switch (_result.ToLower().Substring(0, 2))
            {
                case "ko":
                    _result = m_culturelist[0].ToString();
                    break;

                case "en":
                    _result = m_culturelist[1].ToString();
                    break;

                case "ja":
                    _result = m_culturelist[2].ToString();
                    break;

                case "zh":
                    _result = m_culturelist[3].ToString();
                    break;

                default:
                    _result = m_culturelist[1].ToString();
                    break;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public string MD5ComputeHash(byte[] source)
        {
            return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(source)).Replace("-", "").ToLower();
        }

        public string SHA1ComputeHash(byte[] source)
        {
            return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(source)).Replace("-", "").ToLower();
        }

        /// <summary>
        /// compute hash value of the file
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public string ComputeFileHash(string file_name)
        {
            var _result = Guid.NewGuid().ToString();

            using (FileStream _fs = new FileStream(file_name, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
            {
                using (SHA1CryptoServiceProvider _crypto = new SHA1CryptoServiceProvider())
                {
                    byte[] _hashs = _crypto.ComputeHash(_fs);
                    _result = BitConverter.ToString(_hashs);
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the first day of the month for any full date submitted
        /// </summary>
        /// <param name="day_of_target"></param>
        /// <returns></returns>
        public DateTime GetFirstDayOfMonth(DateTime day_of_target)
        {
            // set return value to the first day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _firstDay = day_of_target;

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            _firstDay = _firstDay.AddDays(-(_firstDay.Day - 1));

            // return the first day of the month
            return _firstDay;
        }

        /// <summary>
        /// Get the first day of the month for a month passed by it's integer value
        /// </summary>
        /// <param name="month_of_target"></param>
        /// <returns></returns>
        public DateTime GetFirstDayOfMonth(int month_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _firstDay = new DateTime(CUnixTime.UtcNow.Year, month_of_target, 1);

            // remove all of the days in the month
            // except the first day and set the
            // variable to hold that date
            _firstDay = _firstDay.AddDays(-(_firstDay.Day - 1));

            // return the first day of the month
            return _firstDay;
        }

        /// <summary>
        /// Get the last day of the month for any full date
        /// </summary>
        /// <param name="day_of_target"></param>
        /// <returns></returns>
        public DateTime GetLastDayOfMonth(DateTime day_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _lastDay = day_of_target;

            // overshoot the date by a month
            _lastDay = _lastDay.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the
            // previous month
            _lastDay = _lastDay.AddDays(-(_lastDay.Day));

            // return the last day of the month
            return _lastDay;
        }

        /// <summary>
        /// Get the last day of a month expressed by it's integer value
        /// </summary>
        /// <param name="month_of_target"></param>
        /// <returns></returns>
        public DateTime GetLastDayOfMonth(int month_of_target)
        {
            // set return value to the last day of the month
            // for any date passed in to the method

            // create a datetime variable set to the passed in date
            DateTime _lastDay = new DateTime(CUnixTime.UtcNow.Year, month_of_target, 1);

            // overshoot the date by a month
            _lastDay = _lastDay.AddMonths(1);

            // remove all of the days in the next month
            // to get bumped down to the last day of the
            // previous month
            _lastDay = _lastDay.AddDays(-(_lastDay.Day));

            // return the last day of the month
            return _lastDay;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private bool? _runningFromNUnit = null;

        /// <summary>
        /// Unit Test 중인지를 확인 합니다.
        /// </summary>
        public bool IsRunningFromNunit
        {
            get
            {
                if (_runningFromNUnit == null)
                {
                    _runningFromNUnit = false;

                    const string _testAssemblyName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
                    foreach (Assembly _assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (_assembly.FullName.StartsWith(_testAssemblyName))
                        {
                            _runningFromNUnit = true;
                            break;
                        }
                    }
                }

                return _runningFromNUnit.Value;
            }
        }

        /// <summary>
        /// SQL ConnectionString에 application name을 추가 합니다.
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="application_name"></param>
        /// <returns></returns>
        public string AppendAppNameInConnectionString(string connection_string, string application_name)
        {
            var _result = "";

            const string _cApplication = "application name";

            var _nodes = connection_string.Split(';');
            foreach (string _node in _nodes)
            {
                if (_node.Split('=')[0].ToLower() != _cApplication)
                    _result += _node + ";";
            }

            _result += String.Format("{0}={1};", _cApplication, application_name);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsAdministrator()
        {
            var _result = false;

            WindowsIdentity _identity = WindowsIdentity.GetCurrent();
            if (_identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(_identity);
                _result = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}