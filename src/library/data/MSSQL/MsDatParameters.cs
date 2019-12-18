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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenTax.Engine.Library.Data.MSSQL
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/data/collection/MsDatParameters/2015/12")]
    [Serializable]
    public class MsDatParameters : NameObjectCollectionBase, ISerializable, IEnumerable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public MsDatParameters()
        {
        }

        /// <summary>
        /// Adds elements from an IDictionary into the new collection.
        /// </summary>
        /// <param name="p_dicts"></param>
        /// <param name="p_readOnly"></param>
        public MsDatParameters(IDictionary p_dicts, Boolean p_readOnly)
        {
            foreach (var _dict in p_dicts)
                Add((string)((DictionaryEntry)_dict).Key, ((DictionaryEntry)_dict).Value);

            IsReadOnly = p_readOnly;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_xmlstring"></param>
        public MsDatParameters(string p_xmlstring)
        {
            PutXmlString(p_xmlstring);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_package"></param>
        public MsDatParameters(XmlPackage p_package)
        {
            PutXmlPackage(p_package);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        public MsDatParameters(MsDatParameters p_dbps)
        {
            foreach (var _parm in p_dbps)
                Add(_parm);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ISerialize
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_info"></param>
        /// <param name="p_context"></param>
        public MsDatParameters(SerializationInfo p_info, StreamingContext p_context)
        {
            var _items = p_info.GetEnumerator();
            while (_items.MoveNext())
                Add(_items.Name, _items.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_info"></param>
        /// <param name="p_context"></param>
        public override void GetObjectData(SerializationInfo p_info, StreamingContext p_context)
        {
            foreach (string m_name in BaseGetAllKeys())
                p_info.AddValue(m_name, BaseGet(m_name));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Static Type Casting
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static implicit operator object[](MsDatParameters p_dbps)
        {
            object[] _params = new object[p_dbps.Count];

            var i = 0;
            foreach (MsDatParameter _dp in p_dbps.BaseGetAllValues())
            {
                _params[i] = _dp.Value;

                i++;
            }

            return _params;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static implicit operator SqlParameter[](MsDatParameters p_dbps)
        {
            SqlParameter[] _params = new SqlParameter[p_dbps.Count];

            var i = 0;
            foreach (MsDatParameter _dp in p_dbps.BaseGetAllValues())
            {
                _params[i] = new SqlParameter(_dp.Name, _dp.Value);
                _params[i].SqlDbType = _dp.Type;
                _params[i].Direction = _dp.Direction;

                i++;
            }

            return _params;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static implicit operator DictionaryEntry[](MsDatParameters p_dbps)
        {
            DictionaryEntry[] _entry = new DictionaryEntry[p_dbps.Count];

            var i = 0;
            foreach (var _dp in p_dbps.BaseGetAllValues())
                _entry[i++] = new DictionaryEntry(((MsDatParameter)_dp).Name, ((MsDatParameter)_dp));

            return _entry;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static implicit operator string(MsDatParameters p_dbps)
        {
            return p_dbps.GetXmlString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public static implicit operator XmlPackage(MsDatParameters p_dbps)
        {
            return p_dbps.GetXmlPackage();
        }

        /// <summary>
        /// DatParamets를 XmlString 표현식으로 변환 합니다.
        /// </summary>
        /// <returns></returns>
        public string GetXmlString()
        {
            foreach (MsDatParameter _p in this)
            {
                Type _type = typeof(object);
                if (_p.Value != null)
                    _type = _p.Value.GetType();

                if (_type.IsSealed == false || _type.BaseType == typeof(Array))
                {
                    _p.FieldType = _type.AssemblyQualifiedName;
                    _p.Value = Serialization.SNG.ClassToString(_type, _p.Value);
                }
                else
                {
                    _p.FieldType = _type.FullName;
                }
            }

            return Serialization.SNG.CompressText
                (
                    Serialization.SNG.ClassToString(typeof(MsDatParameters), this)
                );
        }

        /// <summary>
        /// XmlString형식을 DatParametes 형식으로 변환 합니다.
        /// </summary>
        /// <param name="p_xmlarg"></param>
        public void PutXmlString(string p_xmlarg)
        {
            p_xmlarg = Serialization.SNG.DecompressText(p_xmlarg);
            MsDatParameters _dbps = (MsDatParameters)Serialization.SNG.StringToClass(typeof(MsDatParameters), p_xmlarg);

            foreach (MsDatParameter _p in _dbps)
                Add(_p.Name, _p.FieldType, _p.Type, _p.Direction, _p.Value);

            foreach (MsDatParameter _p in this)
            {
                Type _type = Type.GetType(_p.FieldType);

                if (_type.IsSealed == false || _type.BaseType == typeof(Array))
                    _p.Value = Serialization.SNG.StringToClass(_type, _p.Value.ToString());
            }
        }

        /// <summary>
        /// DatParamets를 XmlPackage 표현식으로 변환 합니다.
        /// </summary>
        /// <returns></returns>
        public XmlPackage GetXmlPackage(MKindOfPacking p_kindOfPacking = MKindOfPacking.Encrypted)
        {
            foreach (MsDatParameter _p in this)
            {
                Type _type = typeof(object);
                if (_p.Value != null)
                    _type = _p.Value.GetType();

                if (_type.IsSealed == false || _type.BaseType == typeof(Array))
                {
                    _p.FieldType = _type.AssemblyQualifiedName;
                    _p.Value = Serialization.SNG.ClassToString(_type, _p.Value);
                }
                else
                {
                    _p.FieldType = _type.FullName;
                }
            }

            return Serialization.SNG.WritePackage<MsDatParameters>(this, p_kindOfPacking);
        }

        /// <summary>
        /// XmlPackage형식을 DatParametes 형식으로 변환 합니다.
        /// </summary>
        /// <param name="p_package"></param>
        public void PutXmlPackage(XmlPackage p_package)
        {
            var _dbps = Serialization.SNG.ReadPackage<MsDatParameters>(p_package);

            foreach (MsDatParameter _p in _dbps)
                Add(_p.Name, _p.FieldType, _p.Type, _p.Direction, _p.Value);

            foreach (MsDatParameter _p in this)
            {
                Type _type = Type.GetType(_p.FieldType);

                if (_type.IsSealed == false || _type.BaseType == typeof(Array))
                    _p.Value = Serialization.SNG.StringToClass(_type, (string)_p.Value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            using (MemoryStream _stream = new MemoryStream())
            {
                BinaryFormatter _formatter = new BinaryFormatter();
                _formatter.Serialize(_stream, this);
                _stream.Position = 0;

                return _formatter.Deserialize(_stream);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MsDatParameter this[int index]
        {
            get
            {
                if (this.AllValues.Length > index)
                    return (MsDatParameter)BaseGet(index);

                return new MsDatParameter();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MsDatParameter this[string key]
        {
            get
            {
                var _result = (MsDatParameter)BaseGet(key);
                if (_result == null)
                    _result = new MsDatParameter();

                return _result;
            }
            set
            {
                BaseSet(key, value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string[] AllKeys
        {
            get
            {
                return (BaseGetAllKeys());
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Array AllValues
        {
            get
            {
                return (BaseGetAllValues());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object GetValue(string key, object value)
        {
            if (HasKey(key) == true)
                value = this[key].Value;

            return value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            if (BaseGet(key) != null)
                return true;

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, MsDatParameter value)
        {
            if (HasKey(key) == true)
                this[key] = value;
            else
                BaseAdd(key, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            if (value is MsDatParameter)
                this.Add(key, value as MsDatParameter);
            else
                throw new ArgumentException("value is not parameter");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Add(MsDatParameter value)
        {
            Add(value.Name, value.FieldType, value.Type, value.Direction, value.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_item"></param>
        public void Add(DictionaryEntry p_item)
        {
            Add((string)p_item.Key, p_item.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="type"></param>
        public void Add(string name, string fieldType, SqlDbType type)
        {
            Add(name, fieldType, type, ParameterDirection.Input, (object)null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(string name, SqlDbType type, object value)
        {
            Add(name, type, ParameterDirection.Input, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(string name, string fieldType, SqlDbType type, object value)
        {
            Add(name, fieldType, type, ParameterDirection.Input, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        public void Add(string name, SqlDbType type, ParameterDirection direction, object value)
        {
            Add(name, "", type, direction, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public void Add(string name, string fieldType, SqlDbType type, ParameterDirection direction, object value)
        {
            if (value is Boolean && type == SqlDbType.NVarChar)
                value = (bool)value ? "T" : "F";

            Add(name, new MsDatParameter(name, fieldType, type, direction, value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            BaseRemove(key);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        ///
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // IEnumerable Interface Implementation:
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public new MsDatEnumerator GetEnumerator() // non-IEnumerable version
        {
            return new MsDatEnumerator(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() // IEnumerable version
        {
            return (IEnumerator)new MsDatEnumerator(this);
        }

        /// <summary>
        ///
        /// </summary>
        public class MsDatEnumerator : IEnumerator
        {
            private int _position = -1;
            private object[] _t;

            /// <summary>
            ///
            /// </summary>
            /// <param name="t"></param>
            public MsDatEnumerator(MsDatParameters t)
            {
                _t = t.BaseGetAllValues();
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
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

            /// <summary>
            ///
            /// </summary>
            public void Reset()
            {
                _position = -1;
            }

            /// <summary>
            ///
            /// </summary>
            public MsDatParameter Current // non-IEnumerator version: type-safe
            {
                get
                {
                    return (MsDatParameter)_t[_position];
                }
            }

            /// <summary>
            ///
            /// </summary>
            object IEnumerator.Current // IEnumerator version: returns object
            {
                get
                {
                    return _t[_position];
                }
            }
        }
    }
}