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

using Npgsql;
using NpgsqlTypes;
using OdinSdk.OdinLib.Security;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OdinSdk.OdinLib.Data.POSTGRESQL
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/data/collection/PgDatParameters/2015/12")]
    [Serializable]
    public class PgDatParameters : NameObjectCollectionBase, ISerializable, IEnumerable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public PgDatParameters()
        {
        }

        /// <summary>
        /// Adds elements from an IDictionary into the new collection.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="read_only"></param>
        public PgDatParameters(IDictionary dictionary, Boolean read_only)
        {
            foreach (var _dict in dictionary)
                Add((string)((DictionaryEntry)_dict).Key, ((DictionaryEntry)_dict).Value);

            IsReadOnly = read_only;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xml_string"></param>
        public PgDatParameters(string xml_string)
        {
            PutXmlString(xml_string);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="package"></param>
        public PgDatParameters(XmlPackage package)
        {
            PutXmlPackage(package);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        public PgDatParameters(PgDatParameters db_params)
        {
            foreach (var _parm in db_params)
                Add(_parm);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ISerialize
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="serialization_info"></param>
        /// <param name="streaming_context"></param>
        public PgDatParameters(SerializationInfo serialization_info, StreamingContext streaming_context)
        {
            var _items = serialization_info.GetEnumerator();
            while (_items.MoveNext())
                Add(_items.Name, _items.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serialization_info"></param>
        /// <param name="streaming_context"></param>
        public override void GetObjectData(SerializationInfo serialization_info, StreamingContext streaming_context)
        {
            foreach (string m_name in BaseGetAllKeys())
                serialization_info.AddValue(m_name, BaseGet(m_name));
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
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static implicit operator object[](PgDatParameters db_params)
        {
            object[] _params = new object[db_params.Count];

            var i = 0;
            foreach (PgDatParameter _dp in db_params.BaseGetAllValues())
            {
                _params[i] = _dp.Value;

                i++;
            }

            return _params;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static implicit operator NpgsqlParameter[](PgDatParameters db_params)
        {
            NpgsqlParameter[] _params = new NpgsqlParameter[db_params.Count];

            var i = 0;
            foreach (PgDatParameter _dp in db_params.BaseGetAllValues())
            {
                _params[i] = new NpgsqlParameter(_dp.Name, _dp.Value);
                _params[i].NpgsqlDbType = _dp.Type;
                _params[i].Direction = _dp.Direction;

                i++;
            }

            return _params;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static implicit operator DictionaryEntry[](PgDatParameters db_params)
        {
            DictionaryEntry[] _entry = new DictionaryEntry[db_params.Count];

            var i = 0;
            foreach (var _dp in db_params.BaseGetAllValues())
                _entry[i++] = new DictionaryEntry(((PgDatParameter)_dp).Name, ((PgDatParameter)_dp));

            return _entry;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static implicit operator string(PgDatParameters db_params)
        {
            return db_params.GetXmlString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public static implicit operator XmlPackage(PgDatParameters db_params)
        {
            return db_params.GetXmlPackage();
        }

        /// <summary>
        /// DatParamets를 XmlString 표현식으로 변환 합니다.
        /// </summary>
        /// <returns></returns>
        public string GetXmlString()
        {
            foreach (PgDatParameter _p in this)
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
                    Serialization.SNG.ClassToString(typeof(PgDatParameters), this)
                );
        }

        /// <summary>
        /// XmlString형식을 DatParametes 형식으로 변환 합니다.
        /// </summary>
        /// <param name="xml_params"></param>
        public void PutXmlString(string xml_params)
        {
            xml_params = Serialization.SNG.DecompressText(xml_params);
            PgDatParameters _dbps = (PgDatParameters)Serialization.SNG.StringToClass(typeof(PgDatParameters), xml_params);

            foreach (PgDatParameter _p in _dbps)
                Add(_p.Name, _p.FieldType, _p.Type, _p.Direction, _p.Value);

            foreach (PgDatParameter _p in this)
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
        public XmlPackage GetXmlPackage(MKindOfPacking kind_of_packing = MKindOfPacking.Encrypted)
        {
            foreach (PgDatParameter _p in this)
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

            return Serialization.SNG.WritePackage<PgDatParameters>(this, kind_of_packing);
        }

        /// <summary>
        /// XmlPackage형식을 DatParametes 형식으로 변환 합니다.
        /// </summary>
        /// <param name="package"></param>
        public void PutXmlPackage(XmlPackage package)
        {
            var _dbps = Serialization.SNG.ReadPackage<PgDatParameters>(package);

            foreach (PgDatParameter _p in _dbps)
                Add(_p.Name, _p.FieldType, _p.Type, _p.Direction, _p.Value);

            foreach (PgDatParameter _p in this)
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
        public PgDatParameter this[int index]
        {
            get
            {
                if (this.AllValues.Length > index)
                    return (PgDatParameter)BaseGet(index);

                return new PgDatParameter();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PgDatParameter this[string key]
        {
            get
            {
                var _result = (PgDatParameter)BaseGet(key);
                if (_result == null)
                    _result = new PgDatParameter();

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
        public void Add(string key, PgDatParameter value)
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
            if (value is PgDatParameter)
                this.Add(key, value as PgDatParameter);
            else
                throw new ArgumentException("value is not parameter");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Add(PgDatParameter value)
        {
            Add(value.Name, value.FieldType, value.Type, value.Direction, value.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public void Add(DictionaryEntry item)
        {
            Add((string)item.Key, item.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="type"></param>
        public void Add(string name, string fieldType, NpgsqlDbType type)
        {
            Add(name, fieldType, type, ParameterDirection.Input, (object)null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(string name, NpgsqlDbType type, object value)
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
        public void Add(string name, string fieldType, NpgsqlDbType type, object value)
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
        public void Add(string name, NpgsqlDbType type, ParameterDirection direction, object value)
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
        public void Add(string name, string fieldType, NpgsqlDbType type, ParameterDirection direction, object value)
        {
            if (value is Boolean && type == NpgsqlDbType.Varchar)
                value = (bool)value ? "T" : "F";

            Add(name, new PgDatParameter(name, fieldType, type, direction, value));
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
            public MsDatEnumerator(PgDatParameters t)
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
            public PgDatParameter Current // non-IEnumerator version: type-safe
            {
                get
                {
                    return (PgDatParameter)_t[_position];
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