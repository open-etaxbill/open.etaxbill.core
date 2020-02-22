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
    public class WorldConfigElement : ConfigurationElement
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor allowing name, url, and port to be specified.
        /// </summary>
        /// <param name="world_name"></param>
        /// <param name="permit_address"></param>
        /// <param name="logger_address"></param>
        /// <param name="connection_string"></param>
        public WorldConfigElement(string world_name, string permit_address, string logger_address, string connection_string)
        {
            Name = world_name;

            Permit = permit_address;
            Logger = logger_address;

            Connection = connection_string;
        }

        /// <summary>
        /// Default constructor, will use default values as defined below.
        /// </summary>
        public WorldConfigElement()
        {
        }

        /// <summary>
        /// Constructor allowing name to be specified, will take the default values for url and port.
        /// </summary>
        /// <param name="elementName"></param>
        public WorldConfigElement(string elementName)
        {
            Name = elementName;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        [ConfigurationProperty("name", DefaultValue = "world_name", IsRequired = true, IsKey = true)]
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

        [ConfigurationProperty("permit", DefaultValue = "http://www.odinsoftware.co.kr", IsRequired = true)]
        public string Permit
        {
            get
            {
                return (string)this["permit"];
            }
            set
            {
                this["permit"] = value;
            }
        }

        [ConfigurationProperty("logger", DefaultValue = "http://www.odinsoftware.co.kr", IsRequired = true)]
        public string Logger
        {
            get
            {
                return (string)this["logger"];
            }
            set
            {
                this["logger"] = value;
            }
        }

        [ConfigurationProperty("connection", DefaultValue = "server=localhost;uid=sa;pwd=p@ssw0rd;database=dbanme", IsRequired = true)]
        public string Connection
        {
            get
            {
                return (string)this["connection"];
            }
            set
            {
                this["connection"] = value;
            }
        }

        [ConfigurationProperty("db", DefaultValue = "MSSQL", IsRequired = true)]
        public string DB
        {
            get
            {
                return (string)this["db"];
            }
            set
            {
                this["db"] = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
            // You can your custom processing code here.
        }

        protected override bool SerializeElement(System.Xml.XmlWriter writer, bool serializeCollectionKey)
        {
            var _result = base.SerializeElement(writer, serializeCollectionKey);
            // You can enter your custom processing code here.
            return _result;
        }

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