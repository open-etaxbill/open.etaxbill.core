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
    /// <summary>
    ///
    /// </summary>
    public class RedisCollection : ConfigurationElementCollection
    {
        /// <summary>
        ///
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RedisElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new RedisElement(elementName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((RedisElement)element).name;
        }

        /// <summary>
        ///
        /// </summary>
        public new string AddElementName
        {
            get
            {
                return base.AddElementName;
            }
            set
            {
                base.AddElementName = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public new string ClearElementName
        {
            get
            {
                return base.ClearElementName;
            }
            set
            {
                base.AddElementName = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public new string RemoveElementName
        {
            get
            {
                return base.RemoveElementName;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public new int Count
        {
            get
            {
                return base.Count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RedisElement this[int index]
        {
            get
            {
                return (RedisElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        new public RedisElement this[string Name]
        {
            get
            {
                return (RedisElement)BaseGet(Name);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public int IndexOf(RedisElement service)
        {
            return BaseIndexOf(service);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service"></param>
        public void Add(RedisElement service)
        {
            BaseAdd(service);
            // Add custom code here.
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="service"></param>
        public void Remove(RedisElement service)
        {
            if (BaseIndexOf(service) >= 0)
                BaseRemove(service.name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        /// <summary>
        ///
        /// </summary>
        public void Clear()
        {
            BaseClear();
            // Add custom code here.
        }
    }
}