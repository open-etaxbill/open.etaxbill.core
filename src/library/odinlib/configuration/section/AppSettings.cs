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
using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    public class AppSettings
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        /// <summary></summary>
        private AppSettings()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<AppSettings> m_lzayHelper = new Lazy<AppSettings>(() =>
        {
            return new AppSettings();
        });

        ///// <summary></summary>
        public static AppSettings SNG
        {
            get
            {
                return m_lzayHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public int[] GetServicePorts(string service_name, int[] service_port)
        {
            var _config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string[] _ports = _config.AppSettings.Settings[service_name + "_servicePorts"].Value.Split(',');

            int[] _result = new int[_ports.Length];
            for (int _p = 0; _p < _ports.Length; _p++)
            {
                var _port = Convert.ToInt32(_ports[_p]);
                if (_port == 0)
                    _result[_p] = service_port[_p];
                else
                    _result[_p] = _port;
            }

            return _result;
        }

        public int GetServicePort(string service_name, int service_port)
        {
            var _config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string[] _ports = _config.AppSettings.Settings[service_name + "_servicePorts"].Value.Split(',');

            var _result = Convert.ToInt32(_ports[0]);
            if (_result == 0)
                _result = service_port;

            return _result;
        }

        public int GetBehaviorPort(string service_name, int behavior_port)
        {
            var _config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var _port = _config.AppSettings.Settings[service_name + "_behaviorPort"].Value;

            var _result = Convert.ToInt32(_port);
            if (_result == 0)
                _result = behavior_port;

            return _result;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
}