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
using System.Collections.Specialized;

namespace OdinSdk.OdinLib.Configuration
{
    /// <summary>
    /// A generic NameValueCollection.
    /// </summary>
    /// <typeparam name="valueT">Value type.</typeparam>
    public class NameObjectCollection<valueT> : NameObjectCollectionBase
    {
        /// <summary>
        /// Cretaes an empty collection.
        /// </summary>
        public NameObjectCollection()
        {
        }

        /// <summary>
        /// Creates a collection from the IDictionary elements.
        /// </summary>
        /// <param name="dic">IDictionary object.</param>
        /// <param name="readOnly">Use TRUE to create a read-only collection.</param>

        public NameObjectCollection(IDictionary dic, bool readOnly)
        {
            foreach (DictionaryEntry de in dic)
            {
                this.BaseAdd(de.Key.ToString(), de.Value);
            }

            this.IsReadOnly = readOnly;
        }

        /// <summary>
        /// Gets a value using an index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public valueT this[int index]
        {
            get
            {
                return (valueT)this.BaseGet(index);
            }
        }

        /// <summary>
        /// Gets or sets a value for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public valueT this[string key]
        {
            get
            {
                return (valueT)this.BaseGet(key);
            }
            set
            {
                this.BaseSet(key, value);
            }
        }

        /// <summary>
        /// Gets an array containing all the keys in the collection.
        /// </summary>
        public string[] AllKeys
        {
            get
            {
                return (string[])this.BaseGetAllKeys();
            }
        }

        /// <summary>
        /// Gets an object array that contains all the values in the collection.
        /// </summary>
        public object[] AllValues
        {
            get
            {
                return this.BaseGetAllValues();
            }
        }

        /// <summary>
        /// Gets a value indicating if the collection contains keys that are not null.
        /// </summary>
        public Boolean HasKeys
        {
            get
            {
                return this.BaseHasKeys();
            }
        }

        /// <summary>
        /// Adds an entry to the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, valueT value)
        {
            this.BaseAdd(key, value);
        }

        /// <summary>
        /// Removes an entry with the specified key from the collection.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            this.BaseRemove(key);
        }

        /// <summary>
        /// Removes an entry in the specified index from the collection.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            this.BaseRemoveAt(index);
        }

        /// <summary>
        /// Clears all the elements in the collection.
        /// </summary>
        public void Clear()
        {
            this.BaseClear();
        }
    }
}