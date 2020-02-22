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
    public class HostElement : ConfigurationElement
    {
        /// <summary>
        /// Default constructor, will use default values as defined below.
        /// </summary>
        public HostElement()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public HostElement(string name)
        {
            this.name = name;
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

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("domain", DefaultValue = ".", IsRequired = false)]
        public string domain
        {
            get
            {
                return (string)this["domain"];
            }
            set
            {
                this["domain"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("productId", DefaultValue = "btc_trade_v10", IsRequired = true)]
        public string productId
        {
            get
            {
                return (string)this["productId"];
            }
            set
            {
                this["productId"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("product_name", DefaultValue = "product", IsRequired = true)]
        public string product_name
        {
            get
            {
                return (string)this["product_name"];
            }
            set
            {
                this["product_name"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
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
        [ConfigurationProperty("port", DefaultValue = "8080", IsRequired = false)]
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

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("macAddress", DefaultValue = "", IsRequired = false)]
        public string macAddress
        {
            get
            {
                return (string)this["macAddress"];
            }
            set
            {
                this["macAddress"] = value;
            }
        }
    }
}