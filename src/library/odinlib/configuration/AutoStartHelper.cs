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

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// Enables or disables the autostart (with the OS) of the application.
    /// </summary>
    public class AutoStartHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        /// <summary></summary>
        private AutoStartHelper()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<AutoStartHelper> m_lzyHelper = new Lazy<AutoStartHelper>(() =>
        {
            return new AutoStartHelper();
        });

        /// <summary></summary>
        public static AutoStartHelper SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        private const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the autostart value for the assembly.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        /// <param name="assemblyLocation">Assembly location (e.g. Assembly.GetExecutingAssembly().Location)</param>
        public void SetAutoStart(string keyName, string assemblyLocation)
        {
            var _regkey = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
            _regkey.SetValue(keyName, assemblyLocation);
        }

        /// <summary>
        /// Returns whether auto start is enabled.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        /// <param name="assemblyLocation">Assembly location (e.g. Assembly.GetExecutingAssembly().Location)</param>
        public bool IsAutoStartEnabled(string keyName, string assemblyLocation)
        {
            var _result = false;

            var _regkey = Registry.CurrentUser.OpenSubKey(RUN_LOCATION);
            if (_regkey != null)
            {
                var _value = (string)_regkey.GetValue(keyName);
                if (_value != null)
                    _result = (_value == assemblyLocation);
            }

            return _result;
        }

        /// <summary>
        /// Unsets the autostart value for the assembly.
        /// </summary>
        /// <param name="keyName">Registry Key Name</param>
        public void UnSetAutoStart(string keyName)
        {
            var _regkey = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
            if (_regkey != null)
            {
                object _value = _regkey.GetValue(keyName);
                if (_value != null)
                    _regkey.DeleteValue(keyName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}