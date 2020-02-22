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

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// Define a custom section containing an individual element and a collection of elements.
    /// </summary>
    public class UrlsSection : ConfigurationSection
    {
        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "MyFavorites", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
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

        /// <summary>
        /// Declare an element (not in a collection) of the type UrlConfigElement.
        /// In the configuration file it corresponds to <simple />.
        /// </summary>
        [ConfigurationProperty("simple")]
        public UrlsConfigElement Simple
        {
            get
            {
                UrlsConfigElement url = (UrlsConfigElement)base["simple"];
                return url;
            }
        }

        /// <summary>
        /// Declare a collection element represented in the configuration file by the sub-section
        /// <urls> <add /> </urls>
        /// Note: the "IsDefaultCollection = false"
        /// instructs the .NET Framework to build a nested section like <urls> ...</urls>.
        /// </summary>
        [ConfigurationProperty("urls", IsDefaultCollection = false)]
        public UrlsCollection Urls
        {
            get
            {
                UrlsCollection urlsCollection = (UrlsCollection)base["urls"];
                return urlsCollection;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        protected override void DeserializeSection(System.Xml.XmlReader reader)
        {
            base.DeserializeSection(reader);
            // You can add custom processing code here.
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="name"></param>
        /// <param name="saveMode"></param>
        /// <returns></returns>
        protected override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            var s = base.SerializeSection(parentElement, name, saveMode);
            // You can add custom processing code here.
            return s;
        }
    }
}