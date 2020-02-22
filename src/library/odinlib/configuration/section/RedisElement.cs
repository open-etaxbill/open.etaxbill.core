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
    public class RedisElement : ConfigurationElement
    {
        /// <summary>
        ///
        /// </summary>
        public RedisElement()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public RedisElement(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address"></param>
        /// <param name="port"></param>
        /// <param name="db_number"></param>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        public RedisElement(string name, string host_name, string ip_address, int port, int db_number, string user_name, string password)
        {
            this.name = name;
            this.hostName = host_name;

            this.ipAddress = ip_address;
            this.port = port;
            this.dbnumber = db_number;

            this.userName = user_name;
            this.password = password;
        }

        [ConfigurationProperty("name", DefaultValue = "redis-default", IsRequired = true, IsKey = true)]
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
        [ConfigurationProperty("port", DefaultValue = 6379, IsRequired = false)]
        public int port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("dbnumber", DefaultValue = 0, IsRequired = false)]
        public int dbnumber
        {
            get
            {
                return (int)this["dbnumber"];
            }
            set
            {
                this["dbnumber"] = value;
            }
        }

        [ConfigurationProperty("allowAdmin", DefaultValue = false, IsRequired = false)]
        public bool AllowAdmin
        {
            get
            {
                return (bool)this["allowAdmin"];
            }
            set
            {
                this["allowAdmin"] = value;
            }
        }

        [ConfigurationProperty("ssl", DefaultValue = false, IsRequired = false)]
        public bool Ssl
        {
            get
            {
                return (bool)this["ssl"];
            }
            set
            {
                this["ssl"] = value;
            }
        }

        [ConfigurationProperty("connectTimeout", DefaultValue = 5000, IsRequired = false)]
        public int ConnectTimeout
        {
            get
            {
                return (int)this["connectTimeout"];
            }
            set
            {
                this["connectTimeout"] = value;
            }
        }

        [ConfigurationProperty("userName", DefaultValue = "guest", IsRequired = false)]
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

        [ConfigurationProperty("password", DefaultValue = "*", IsRequired = false)]
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