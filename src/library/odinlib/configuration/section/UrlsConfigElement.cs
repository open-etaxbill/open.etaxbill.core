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
    public class UrlsConfigElement : ConfigurationElement
    {
        // Constructor allowing name, url, and port to be specified.
        public UrlsConfigElement(String newName, String newUrl, int newPort)
        {
            Name = newName;
            Url = newUrl;
            Port = newPort;
        }

        // Default constructor, will use default values as defined
        // below.
        public UrlsConfigElement()
        {
        }

        // Constructor allowing name to be specified, will take the
        // default values for url and port.
        public UrlsConfigElement(string elementName)
        {
            Name = elementName;
        }

        [ConfigurationProperty("name", DefaultValue = "Microsoft", IsRequired = true, IsKey = true)]
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

        [ConfigurationProperty("url", DefaultValue = "http://www.microsoft.com", IsRequired = true)]
        [RegexStringValidator(@"\w+:\/\/[\w.]+\S*")]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = (int)0, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 8080, ExcludeRange = false)]
        public int Port
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

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
            // You can your custom processing code here.
        }

        protected override bool SerializeElement(System.Xml.XmlWriter writer, bool serializeCollectionKey)
        {
            var ret = base.SerializeElement(writer, serializeCollectionKey);
            // You can enter your custom processing code here.
            return ret;
        }

        protected override bool IsModified()
        {
            var ret = base.IsModified();
            // You can enter your custom processing code here.
            return ret;
        }
    }
}