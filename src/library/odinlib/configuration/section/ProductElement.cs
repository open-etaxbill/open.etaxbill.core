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
    public class ProductElement : ConfigurationElement
    {
        /// <summary>
        ///
        /// </summary>
        public ProductElement()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="name"></param>
        /// <param name="db_type"></param>
        public ProductElement(string id, string version, string name, string db_type)
        {
            this.id = id;
            this.version = version;
            this.name = name;
            this.type = db_type;
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("id", DefaultValue = "btc_trade_v10", IsRequired = true)]
        public string id
        {
            get
            {
                return (string)this["id"];
            }
            set
            {
                this["id"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("version", DefaultValue = "V5.1.2015.12", IsRequired = true)]
        public string version
        {
            get
            {
                return (string)this["version"];
            }
            set
            {
                this["version"] = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "default", IsRequired = true)]
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
        [ConfigurationProperty("type", DefaultValue = "service", IsRequired = true)]
        public string type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }
    }
}