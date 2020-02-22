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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/config/software/2015/12")]
    [Serializable]
    public class InstalledSoftware : IDisposable
    {
        #region Properties

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "SoftwareKey", Order = 0)]
        public string SoftwareKey
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "DisplayName", Order = 1)]
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "DisplayVersion", Order = 2)]
        public string DisplayVersion
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "InstallDate", Order = 3)]
        public string InstallDate
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "ProductId", Order = 4)]
        public string ProductId
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "ProductVersion", Order = 5)]
        public string ProductVersion
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Publisher", Order = 6)]
        public string Publisher
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "InstallLocation", Order = 7)]
        public string InstallLocation
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "Version", Order = 8)]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "VersionMajor", Order = 9)]
        public string VersionMajor
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "VersionMinor", Order = 10)]
        public string VersionMinor
        {
            get;
            set;
        }

        #endregion Properties

        #region method

        /// <summary>
        /// Gets a list of installed software and, if known, the software's install path.
        /// </summary>
        /// <returns></returns>
        public static List<InstalledSoftware> GetinstalledSoftware()
        {
            List<InstalledSoftware> _result = new List<InstalledSoftware>();

            const string _hklmSoftwareKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (var _regkey = Registry.LocalMachine.OpenSubKey(_hklmSoftwareKey))
            {
                foreach (string _keyNames in _regkey.GetSubKeyNames())
                {
                    using (RegistryKey _subkey = _regkey.OpenSubKey(_keyNames))
                    {
                        var _software = new InstalledSoftware();

                        foreach (string _k in _subkey.GetValueNames())
                        {
                            var _v = _subkey.GetValue(_k).ToString();

                            foreach (PropertyInfo _p in _software.GetType().GetProperties())
                            {
                                if (_p.Name == _k)
                                {
                                    if (_p.PropertyType.Name == "DateTime")
                                        _p.SetValue(_software, Convert.ToDateTime(_v), null);
                                    if (_p.PropertyType.Name == "Boolean")
                                        _p.SetValue(_software, Convert.ToBoolean(_v), null);
                                    else if (_p.PropertyType.Name == "Int32")
                                        _p.SetValue(_software, Convert.ToInt32(_v), null);
                                    else if (_p.PropertyType.Name == "String")
                                        _p.SetValue(_software, _v, null);
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(_software.DisplayName) == false)
                        {
                            _software.SoftwareKey = Path.GetFileName(_subkey.Name);
                            _result.Add(_software);
                        }
                    }
                }
            }

            return _result;
        }

        /*
         * Sample
         *
        public void PrintInstalledSoftware()
        {
            List<InstalledSoftware> _is = InstalledSoftware.GetinstalledSoftware();

            var _x = Serialization.SNG.ObjectToXml<InstalledSoftware[]>(_is.ToArray());
            Console.WriteLine(_x);
        }
        */

        #endregion method

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~InstalledSoftware()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}