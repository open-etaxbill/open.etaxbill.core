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
    public class ServiceConfigElement : ConfigurationElement
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor allowing name, url, and port to be specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="protocol">통신 프로토콜(tcp, http)</param>
        /// <param name="category_id">분류 명칭(corptool, back-office, fron-office)</param>
        /// <param name="product_name">제품명칭</param>
        /// <param name="product_id">제품 식별자(identification)</param>
        /// <param name="product_version">제품의 버전</param>
        /// <param name="host_name">호스트 명칭</param>
        /// <param name="ip_address">IP주소</param>
        public ServiceConfigElement(string name, string protocol, string category_id, string product_name, string product_id, string product_version, string host_name, string ip_address)
        {
            this.Name = name;

            this.Protocol = protocol;

            this.CategoryId = category_id;
            this.ProductName = product_name;
            this.ProductId = product_id;
            this.ProductVersion = product_version;

            this.HostName = host_name;
            this.IpAddress = ip_address;
        }

        /// <summary>
        /// Default constructor, will use default values as defined below.
        /// </summary>
        public ServiceConfigElement()
        {
        }

        /// <summary>
        /// Constructor allowing name to be specified, will take the default values for url and port.
        /// </summary>
        /// <param name="elementName"></param>
        public ServiceConfigElement(string elementName)
        {
            Name = elementName;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        [ConfigurationProperty("name", DefaultValue = "Logger_UBS-TESTDEV1_V45", IsRequired = true, IsKey = true)]
        public string Name
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

        [ConfigurationProperty("protocol", DefaultValue = "TCP", IsRequired = true)]
        public string Protocol
        {
            get
            {
                return (string)this["protocol"];
            }
            set
            {
                this["protocol"] = value;
            }
        }

        [ConfigurationProperty("categoryId", DefaultValue = "sdk", IsRequired = true)]
        public string CategoryId
        {
            get
            {
                return (string)this["categoryId"];
            }
            set
            {
                this["categoryId"] = value;
            }
        }

        [ConfigurationProperty("product_name", DefaultValue = "Logger_V10", IsRequired = true)]
        public string ProductName
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

        [ConfigurationProperty("productId", DefaultValue = "logger", IsRequired = true)]
        public string ProductId
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

        [ConfigurationProperty("pVersion", DefaultValue = "V4.5.2016.07", IsRequired = true)]
        public string ProductVersion
        {
            get
            {
                return (string)this["pVersion"];
            }
            set
            {
                this["pVersion"] = value;
            }
        }

        [ConfigurationProperty("hostName", DefaultValue = "ubs-testdev1", IsRequired = true)]
        public string HostName
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

        [ConfigurationProperty("ipAddress", DefaultValue = "localhost", IsRequired = true)]
        public string IpAddress
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

        [ConfigurationProperty("UsingL4", DefaultValue = "false", IsRequired = false)]
        public string UsingL4
        {
            get
            {
                return (string)this["UsingL4"];
            }
            set
            {
                this["UsingL4"] = value;
            }
        }

        [ConfigurationProperty("IpAddressL4", DefaultValue = "", IsRequired = false)]
        public string IpAddressL4
        {
            get
            {
                return (string)this["IpAddressL4"];
            }
            set
            {
                this["IpAddressL4"] = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serializeCollectionKey"></param>
        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
            // You can your custom processing code here.
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="serializeCollectionKey"></param>
        /// <returns></returns>
        protected override bool SerializeElement(System.Xml.XmlWriter writer, bool serializeCollectionKey)
        {
            var _result = base.SerializeElement(writer, serializeCollectionKey);

            // You can enter your custom processing code here.
            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override bool IsModified()
        {
            var _result = base.IsModified();

            // You can enter your custom processing code here.
            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}