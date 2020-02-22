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

using Microsoft.Win32;
using System;
using System.Net;
using System.Security.AccessControl;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
    /// <summary></summary>
    public class RegHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        public RegHelper()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<RegHelper> m_lzayHelper = new Lazy<RegHelper>(() =>
        {
            return new RegHelper();
        });

        /// <summary></summary>
        public static RegHelper SNG
        {
            get
            {
                return m_lzayHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Get Application Register
        //-----------------------------------------------------------------------------------------------------------------------------
        public const string SDKFrameVersion = "V1.0.2016.07";

        public const string c_regRootKey = @"Software\OdinSoft\" + SDKFrameVersion;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public RegistryKey GetBaseKey()
        {
            RegistryKey _result;

            if (Environment.Is64BitOperatingSystem == true)
                _result = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                _result = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seekey"></param>
        /// <param name="regkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        public object ReadRegistry(string seekey, string regkey, object default_value = null)
        {
            object _result = null;

            RegistryKey _rkbase = GetBaseKey();
            {
                RegistryKey _regkey = _rkbase.OpenSubKey(seekey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (_regkey == null && default_value != null)
                    _regkey = _rkbase.CreateSubKey(seekey, RegistryKeyPermissionCheck.ReadWriteSubTree);

                if (_regkey != null)
                {
                    _result = _regkey.GetValue(regkey, null);
                    if (_result == null && default_value != null)
                    {
                        _result = default_value;
                        _regkey.SetValue(regkey, default_value);
                    }

                    _regkey.Close();
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seekey"></param>
        /// <param name="regkey"></param>
        /// <param name="default_value"></param>
        /// <returns></returns>
        private object ReadRegistry2(string seekey, string regkey, object default_value = null)
        {
            var _seekey = c_regRootKey;
            if (String.IsNullOrEmpty(seekey) == false)
                _seekey += @"\" + seekey;

            return ReadRegistry(_seekey, regkey, default_value);
        }

        private string GetSeeKey(string location, string category_id, string product_id = "")
        {
            var _seekey = "";
            {
                if (String.IsNullOrEmpty(location) == false)
                    _seekey += @"\" + location;
                else
                    throw new Exception("argment empty error: location must have a value");

                if (String.IsNullOrEmpty(category_id) == false)
                    _seekey += @"\" + category_id;
                else
                    throw new Exception("argment empty error: category must have a value");

                if (String.IsNullOrEmpty(product_id) == false)
                    _seekey += @"\" + product_id;
            }

            return _seekey;
        }

        private object ReadRegistry(string location, string category_id, string product_id, string regkey, object default_value)
        {
            object _result = null;

            if (String.IsNullOrEmpty(regkey) == false)
            {
                var _seekey = GetSeeKey(location, category_id, product_id);
                _result = ReadRegistry2(_seekey, regkey, default_value);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seekey"></param>
        /// <param name="regkey"></param>
        /// <param name="reg_value"></param>
        /// <param name="is_create"></param>
        public void WriteRegistry(string seekey, string regkey, object reg_value, bool is_create = true)
        {
            var _rkbase = GetBaseKey();
            {
                var _regkey = _rkbase.OpenSubKey(seekey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (_regkey == null && is_create == true)
                    _regkey = _rkbase.CreateSubKey(seekey, RegistryKeyPermissionCheck.ReadWriteSubTree);

                if (_regkey != null)
                {
                    _regkey.SetValue(regkey, reg_value);
                    _regkey.Close();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seekey"></param>
        /// <param name="regkey"></param>
        /// <param name="reg_value"></param>
        /// <param name="is_create"></param>
        private void WriteRegistry2(string seekey, string regkey, object reg_value, bool is_create = true)
        {
            var _seekey = c_regRootKey;
            if (String.IsNullOrEmpty(seekey) == false)
                _seekey += @"\" + seekey;

            WriteRegistry(_seekey, regkey, reg_value, is_create);
        }

        private void WriteRegistry(string location, string category_id, string product_id, string regkey, object reg_value)
        {
            var _seekey = GetSeeKey(location, category_id, product_id);
            WriteRegistry2(_seekey, regkey, reg_value, true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="seekey"></param>
        /// <param name="delete_key"></param>
        public void RemoveRegistry(string seekey, string delete_key)
        {
            var _rkbase = GetBaseKey();
            {
                var _regkey = _rkbase.OpenSubKey(seekey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (_regkey != null)
                {
                    _regkey.DeleteSubKeyTree(delete_key, false);
                    _regkey.Close();
                }
            }
        }

        private void RemoveRegistry2(string location, string category_id, string product_id)
        {
            var _seekey = c_regRootKey + @"\" + GetSeeKey(location, category_id);
            RemoveRegistry(_seekey, product_id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private const string SharedSide = "shared";

        private const string ServerSide = "server";
        private const string ClientSide = "client";

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="default_value">기본값</param>
        /// <returns></returns>
        public string GetShared(string host_name, string regkey, string default_value)
        {
            var _result = (string)ReadRegistry(SharedSide, host_name, "", regkey, "");
            if (String.IsNullOrEmpty(_result) == true)
            {
                WriteRegistry(SharedSide, host_name, "", regkey, default_value);
                _result = default_value;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="value"></param>
        public void SetShared(string host_name, string regkey, string value)
        {
            WriteRegistry(SharedSide, host_name, "", regkey, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        public void DelShared(string host_name, string regkey)
        {
            RemoveRegistry2(SharedSide, host_name, regkey);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="default_value">기본값</param>
        /// <returns></returns>
        public string GetServer(string category_id, string product_id, string regkey, string default_value)
        {
            return (string)ReadRegistry(ServerSide, category_id, product_id, regkey, default_value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="value"></param>
        public void SetServer(string category_id, string product_id, string regkey, string value)
        {
            WriteRegistry(ServerSide, category_id, product_id, regkey, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        public void DelServer(string category_id, string product_id)
        {
            RemoveRegistry2(ServerSide, category_id, product_id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="default_value">기본값</param>
        /// <returns></returns>
        public string GetClient(string category_id, string product_id, string regkey, string default_value)
        {
            return (string)ReadRegistry(ClientSide, category_id, product_id, regkey, default_value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="regkey">사용자가 찾고자 하는 키</param>
        /// <param name="value"></param>
        public void SetClient(string category_id, string product_id, string regkey, string value)
        {
            WriteRegistry(ClientSide, category_id, product_id, regkey, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        public void DelClient(string category_id, string product_id)
        {
            RemoveRegistry2(ClientSide, category_id, product_id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Server
        //-----------------------------------------------------------------------------------------------------------------------------
        private const string m_connection_string = "ConnectionString";

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <returns></returns>
        public string GetConnectionString(string category_id, string product_id)
        {
            return (string)GetServer(category_id, product_id, m_connection_string, "");
        }

        private const string AuthMethod = "AuthMethod";

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <returns></returns>
        public string GetAuthMethod(string category_id, string product_id)
        {
            return (string)GetServer(category_id, product_id, AuthMethod, "DB");
        }

        private const string KindOfDB = "KindOfDB";

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <returns></returns>
        public string GetKindOfDB(string category_id, string product_id)
        {
            return (string)GetServer(category_id, product_id, KindOfDB, "MSSQL");
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Client
        //-----------------------------------------------------------------------------------------------------------------------------
        private const string c_serviceUrl = "WcfServiceUrl";

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <returns></returns>
        public string GetServiceUrl(string category_id, string product_id)
        {
            var _hostName = (string)GetClient(category_id, product_id, c_serviceUrl, "");
            return CfgHelper.SNG.GetLocalIpAddress(_hostName);
        }

        private const string c_topUrlAddress = "TopUrlAddress";

        private string m_topUrlAddress = null;

        public string TopUrlAddress
        {
            get
            {
                if (m_topUrlAddress == null)
                    m_topUrlAddress = (string)ReadRegistry2("", c_topUrlAddress, "");

                return m_topUrlAddress;
            }
            set
            {
                if (m_topUrlAddress != value)
                {
                    WriteRegistry2("", c_topUrlAddress, value);
                    m_topUrlAddress = value;
                }
            }
        }

        private const string c_worldName = "WorldName";

        private string m_worldName = null;

        public string WorldName
        {
            get
            {
                if (m_worldName == null)
                    m_worldName = (string)ReadRegistry2("", c_worldName, "");

                return m_worldName;
            }
            set
            {
                if (m_worldName != value)
                {
                    WriteRegistry2("", c_worldName, value);
                    m_worldName = value;
                }
            }
        }

        private const string c_productId = "ProductId";

        /// <summary>
        ///
        /// </summary>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <returns></returns>
        public string GetProductId(string category_id, string product_id)
        {
            var _result = product_id;

            if (String.IsNullOrEmpty(product_id) == false)
                _result = GetServer(category_id, product_id, c_productId, _result);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Shared
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 호스트명에 해당하는 IP주소를 정의 합니다.
        /// 이 경우는 IP주소가 명확 하지 않아 생기는 오류를 방지 하기 위해
        /// 사용 되어 집니다.
        /// </summary>
        /// <param name="host_name"></param>
        /// <returns></returns>
        public string GetIpAddress(string host_name)
        {
            return this.GetShared(host_name, "IpAddress", "localhost");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address">IP주소</param>
        public void SetIpAddress(string host_name, string ip_address)
        {
            this.SetShared(host_name, "IpAddress", ip_address);
        }

        /// <summary>
        /// 포트 공유를 하는지 여부를 지정 합니다.
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        /// <returns></returns>
        public bool GetIsPortSharing(string ip_address)
        {
            return this.GetShared(ip_address, "IsPortSharing", "false").ToLower() == "true";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        /// <param name="is_port_sharing">포트 공유를 하는지 여부(port sharing service가 동작 중이어야 함)</param>
        public void SetIsPortSharing(string ip_address, bool is_port_sharing)
        {
            this.SetShared(ip_address, "IsPortSharing", is_port_sharing.ToString());
        }

        /// <summary>
        /// IsPortSharing이 true인 경우에 공유할 포트 번호를 지정 합니다.
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        /// <returns></returns>
        public int GetSharingPort(string ip_address)
        {
            return Convert.ToInt32(this.GetShared(ip_address, "SharingPort", "-1"));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip_address">IP주소</param>
        /// <param name="sharing_port">wcf가 포트를 공유 할 포트 번호</param>
        public void SetSharingPort(string ip_address, int sharing_port)
        {
            this.SetShared(ip_address, "SharingPort", sharing_port.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name"></param>
        /// <returns></returns>
        public string GetRedirectUri(string host_name)
        {
            return this.GetShared(host_name, "RedirectUri", "localhost");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name"></param>
        /// <param name="redirect_uri"></param>
        public void SetRedirectUri(string host_name, string redirect_uri)
        {
            this.SetShared(host_name, "RedirectUri", redirect_uri);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host_name">host-name</param>
        /// <returns></returns>
        public string GetRedirectedIpAddress(string host_name)
        {
            var _result = host_name;

            if (CfgHelper.SNG.IsValidIP(host_name) == false)
            {
                if (String.IsNullOrEmpty(host_name) == true || host_name == "localhost")
                    host_name = Dns.GetHostName();

                var _redirected_uri = GetRedirectUri(host_name);
                _result = CfgHelper.SNG.GetIPAddress(_redirected_uri);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // .Net FrameWork
        //-----------------------------------------------------------------------------------------------------------------------------
        private const string c_dotnetFramework = @"Software\Microsoft\.NETFramework";

        /// <summary>
        /// 소스내에 지정된 프레임워크의 버전 입니다.
        /// </summary>
        public string GetFrameWorkVersion
        {
            get
            {
                return SDKFrameVersion;
            }
        }

        public object GetDotNetFrameWorkReg(string regkey, object default_value)
        {
            object _result = null;

            var _regkey = Registry.LocalMachine;

            _regkey = _regkey.OpenSubKey(c_dotnetFramework, false);
            if (_regkey != null)
                _result = _regkey.GetValue(regkey, default_value);

            if (_regkey != null)
                _regkey.Close();

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // MIME Type
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_extension"></param>
        /// <returns></returns>
        public string GetMIMEType(string file_extension)
        {
            var _result = "unknown-type";

            var _regkey = Registry.ClassesRoot.OpenSubKey(file_extension);
            if (_regkey != null)
            {
                object _content_type = _regkey.GetValue("Content Type");
                if (_content_type != null)
                    _result = _content_type.ToString();

                _regkey.Close();
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // x64, x32 program, common files folder
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 윈도즈 O/S에 따라 프로그램 파일의 위치가 틀린 관계로 본 함수가 필요 합니다.
        /// folder_x86이 false이면 x32, x64 무관하게 Program Files을 되돌립니다.
        ///              true이면 x32, x64 무관하게 Program Files(x86)을 되돌립니다.
        /// </summary>
        /// <example>
        /// string _folder = GetProgramCommonFilesFolder(true);
        /// </example>
        /// <seealso cref="GetProgramCommonFilesFolder"/>
        /// <param name="folder_x86">default 값은 false</param>
        /// <returns></returns>
        public string GetProgramFilesFolder(bool folder_x86 = false)
        {
            var _folder = Environment.GetEnvironmentVariable("ProgramFiles");

            if (folder_x86 == false)
            {
                if (Environment.Is64BitOperatingSystem == true)
                {
                    if (Environment.Is64BitProcess == false)
                        _folder = Environment.GetEnvironmentVariable("ProgramW6432");
                }
            }
            else
            {
                if (Environment.Is64BitOperatingSystem == true)
                    _folder = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return _folder;
        }

        /// <summary>
        /// 윈도즈 O/S에 따라 프로그램 파일의 위치가 틀린 관계로 본 함수가 필요 합니다.
        /// folder_x86이 false이면 x32, x64 무관하게 Program Files\Common Files을 되돌립니다.
        ///              true이면 x32, x64 무관하게 Program Files(x86)\Common Files을 되돌립니다.
        /// </summary>
        /// <example>
        /// string _folder = GetProgramCommonFilesFolder(true);
        /// </example>
        /// <seealso cref="GetProgramFilesFolder"/>
        /// <param name="folder_x86">default 값은 false</param>
        /// <returns></returns>
        public string GetProgramCommonFilesFolder(bool folder_x86 = false)
        {
            var _folder = Environment.GetEnvironmentVariable("CommonProgramFiles");

            if (folder_x86 == false)
            {
                if (Environment.Is64BitOperatingSystem == true)
                {
                    if (Environment.Is64BitProcess == false)
                        _folder = Environment.GetEnvironmentVariable("CommonProgramW6432");
                }
            }
            else
            {
                if (Environment.Is64BitOperatingSystem == true)
                    _folder = Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");
            }

            return _folder;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
}