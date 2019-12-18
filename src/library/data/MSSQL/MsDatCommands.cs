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

using OdinSdk.BaseLib.Security;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

//#pragma warning disable 1589, 1591

namespace OpenTax.Engine.Library.Data.MSSQL
{
    [Serializable]
    public class MsDatCommands : NameObjectCollectionBase, IEnumerable, ISerializable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public MsDatCommands()
        {
        }

        public MsDatCommands(IDictionary p_dicts, Boolean p_readOnly)
        {
            foreach (DictionaryEntry _entry in p_dicts)
                Add((string)_entry.Key, _entry.Value);

            IsReadOnly = p_readOnly;
        }

        public MsDatCommands(SerializationInfo p_info, StreamingContext p_context)
        {
            SerializationInfoEnumerator _items = p_info.GetEnumerator();
            while (_items.MoveNext())
                Add(_items.Name, _items.Value);
        }

        public MsDatCommands(XmlPackage p_package)
        {
            PutXmlString(p_package);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Static Type Casting
        //-----------------------------------------------------------------------------------------------------------------------------
        public static implicit operator DictionaryEntry[](MsDatCommands p_cmds)
        {
            DictionaryEntry[] m_entry = new DictionaryEntry[p_cmds.Count];

            var i = 0;
            foreach (MsDatCommand _dc in p_cmds.BaseGetAllValues())
                m_entry[i++] = new DictionaryEntry(_dc.Name, _dc);

            return m_entry;
        }

        public static implicit operator XmlPackage(MsDatCommands p_cmds)
        {
            return p_cmds.GetXmlPackage();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // XmlSerializer
        //-----------------------------------------------------------------------------------------------------------------------------
        public XmlPackage GetXmlPackage()
        {
            return Serialization.SNG.WritePackage<MsDatCommands>(this);
        }

        public void PutXmlString(XmlPackage p_package)
        {
            MsDatCommands _dbcs = Serialization.SNG.ReadPackage<MsDatCommands>(p_package);
            foreach (MsDatCommand _dc in _dbcs)
                Add(_dc.Name, _dc.Text, _dc.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private DictionaryEntry m_entry = new DictionaryEntry();

        // Gets a key-and-value pair (DictionaryEntry) using an index.
        public DictionaryEntry this[int index]
        {
            get
            {
                m_entry.Key = BaseGetKey(index);
                m_entry.Value = BaseGet(index);
                return (m_entry);
            }
        }

        // Gets or sets the value associated with the specified key.
        public Object this[string key]
        {
            get
            {
                return (BaseGet(key));
            }
            set
            {
                BaseSet(key, value);
            }
        }

        // Gets a string array that contains all the keys in the collection.
        public string[] AllKeys
        {
            get
            {
                return (BaseGetAllKeys());
            }
        }

        // Gets an Object array that contains all the values in the collection.
        public Array AllValues
        {
            get
            {
                return (BaseGetAllValues());
            }
        }

        // Gets a string array that contains all the values in the collection.
        public string[] AllStringValues
        {
            get
            {
                return ((string[])BaseGetAllValues(typeof(string)));
            }
        }

        // Gets a value indicating if the collection contains keys that are not null.
        public Boolean HasKeys
        {
            get
            {
                return (BaseHasKeys());
            }
        }

        // Adds an entry to the collection.
        public void Add(string key, Object value)
        {
            if (BaseGet(key) != null)
                this[key] = value;
            else
                BaseAdd(key, value);
        }

        public void Add(string p_text, MsDatParameters p_parms)
        {
            Add(base.Count.ToString(), (object)new MsDatCommand(p_text, p_parms));
        }

        public void Add(string p_name, string p_text, MsDatParameters p_parms)
        {
            Add(p_name, (object)new MsDatCommand(p_name, p_text, p_parms));
        }

        // Removes an entry with the specified key from the collection.
        public void Remove(string key)
        {
            BaseRemove(key);
        }

        // Removes an entry in the specified index from the collection.
        public void Remove(int index)
        {
            BaseRemoveAt(index);
        }

        // Clears all the elements in the collection.
        public void Clear()
        {
            BaseClear();
        }

        public override void GetObjectData(SerializationInfo p_info, StreamingContext p_context)
        {
            foreach (string name in base.BaseGetAllKeys())
                p_info.AddValue(name, base.BaseGet(name));
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // IEnumerable Interface Implementation:
        //-----------------------------------------------------------------------------------------------------------------------------
        public new MsCmdEnumerator GetEnumerator() // non-IEnumerable version
        {
            return new MsCmdEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() // IEnumerable version
        {
            return (IEnumerator)new MsCmdEnumerator(this);
        }

        public class MsCmdEnumerator : IEnumerator
        {
            private int _position = -1;
            private object[] _t;

            public MsCmdEnumerator(MsDatCommands t)
            {
                _t = t.BaseGetAllValues();
            }

            public bool MoveNext()
            {
                if (_position < _t.Length - 1)
                {
                    _position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                _position = -1;
            }

            public MsDatCommand Current // non-IEnumerator version: type-safe
            {
                get
                {
                    return (MsDatCommand)_t[_position];
                }
            }

            object IEnumerator.Current // IEnumerator version: returns object
            {
                get
                {
                    return _t[_position];
                }
            }
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------
}