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
using System.Collections;
using System.Configuration;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Configuration
{
    public class UrlsCollection : ConfigurationElementCollection
    {
        protected UrlsCollection()
        {
        }

        protected UrlsCollection(IComparer comparer)
            : base(comparer)
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new UrlsConfigElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new UrlsConfigElement(elementName);
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((UrlsConfigElement)element).Name;
        }

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

        public new string RemoveElementName
        {
            get
            {
                return base.RemoveElementName;
            }
        }

        public new int Count
        {
            get
            {
                return base.Count;
            }
        }

        public UrlsConfigElement this[int index]
        {
            get
            {
                return (UrlsConfigElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        new public UrlsConfigElement this[string Name]
        {
            get
            {
                return (UrlsConfigElement)BaseGet(Name);
            }
        }

        public int IndexOf(UrlsConfigElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(UrlsConfigElement url)
        {
            BaseAdd(url);
            // Add custom code here.
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
            // Add custom code here.
        }

        public void Remove(UrlsConfigElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
            // Add custom code here.
        }
    }
}