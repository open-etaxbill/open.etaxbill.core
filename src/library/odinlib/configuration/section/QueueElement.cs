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

using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public class QueueElement : ConfigurationElement
    {
        /// <summary>
        ///
        /// </summary>
        public QueueElement()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public QueueElement(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="virtual_host"></param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        public QueueElement(string name, string host_name, string ip_address, string virtual_host, string user_name, string password)
        {
            this.name = name;
            this.hostName = host_name;

            this.ipAddress = ip_address;
            this.virtualHost = virtual_host;

            this.userName = user_name;
            this.password = password;
        }

        [ConfigurationProperty("name", DefaultValue = "default", IsRequired = true, IsKey = true)]
        public string name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("hostName", DefaultValue = "localhost", IsRequired = true)]
        public string hostName
        {
            get
            {
                return (string)this["hostName"];
            }
            set
            {
                this["hostName"] = value;
            }
        }

        [ConfigurationProperty("ipAddress", DefaultValue = "127.0.0.0", IsRequired = true)]
        public string ipAddress
        {
            get
            {
                return (string)this["ipAddress"];
            }
            set
            {
                this["ipAddress"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = "5672", IsRequired = false)]
        public string port
        {
            get
            {
                return (string)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("virtualHost", DefaultValue = "/", IsRequired = true)]
        public string virtualHost
        {
            get
            {
                return (string)this["virtualHost"];
            }
            set
            {
                this["virtualHost"] = value;
            }
        }

        [ConfigurationProperty("userName", DefaultValue = "guest", IsRequired = true)]
        public string userName
        {
            get
            {
                return (string)this["userName"];
            }
            set
            {
                this["userName"] = value;
            }
        }

        [ConfigurationProperty("password", DefaultValue = "*", IsRequired = true)]
        public string password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }
}