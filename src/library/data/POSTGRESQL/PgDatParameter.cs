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
using System;
using System.Data;
using System.Runtime.Serialization;

namespace OpenTax.Engine.Library.Data.POSTGRESQL
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/data/collection/PgDatParameter/2015/12")]
    [Serializable]
    public class PgDatParameter : ISerializable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public PgDatParameter()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_name"></param>
        /// <param name="p_type"></param>
        /// <param name="p_value"></param>
        /// <param name="p_direction"></param>
        public PgDatParameter(string p_name, NpgsqlDbType p_type, ParameterDirection p_direction, object p_value)
            : this(p_name, "", p_type, p_direction, p_value)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_name"></param>
        /// <param name="p_fieldType"></param>
        /// <param name="p_type"></param>
        /// <param name="p_value"></param>
        /// <param name="p_direction"></param>
        public PgDatParameter(string p_name, string p_fieldType, NpgsqlDbType p_type, ParameterDirection p_direction, object p_value)
        {
            m_name = p_name;

            m_fieldType = p_fieldType;
            m_type = p_type;

            m_direction = p_direction;
            m_value = p_value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ISerialize
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_info"></param>
        /// <param name="p_context"></param>
        public PgDatParameter(SerializationInfo p_info, StreamingContext p_context)
        {
            if (p_info == null)
                throw new System.ArgumentNullException("info");

            m_name = (string)p_info.GetValue("name", typeof(string));
            m_field = (string)p_info.GetValue("field", typeof(string));

            m_fieldType = (string)p_info.GetValue("fieldType", typeof(string));
            m_type = (NpgsqlDbType)p_info.GetValue("type", typeof(NpgsqlDbType));

            m_direction = (ParameterDirection)p_info.GetValue("direction", typeof(ParameterDirection));
            m_value = (object)p_info.GetValue("value", typeof(object));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_info"></param>
        /// <param name="p_context"></param>
        public virtual void GetObjectData(SerializationInfo p_info, StreamingContext p_context)
        {
            if (p_info == null)
                throw new System.ArgumentNullException("info");

            p_info.AddValue("name", m_name);
            p_info.AddValue("field", m_field);

            p_info.AddValue("fieldType", m_fieldType);
            p_info.AddValue("type", m_type);

            p_info.AddValue("direction", m_direction);
            p_info.AddValue("value", m_value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        [DataMember(Name = "name", Order = 0)]
        private string m_name = "";

        [DataMember(Name = "field", Order = 1)]
        private string m_field = "";

        [DataMember(Name = "fieldType", Order = 2)]
        private string m_fieldType = "";

        [DataMember(Name = "type", Order = 3)]
        private NpgsqlDbType m_type = NpgsqlDbType.Varchar;

        [DataMember(Name = "direction", Order = 4)]
        private ParameterDirection m_direction = ParameterDirection.Input;

        [DataMember(Name = "value", Order = 5)]
        private object m_value = null;

        /// <summary>
        ///
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string Field
        {
            get
            {
                return m_field;
            }

            set
            {
                m_field = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string FieldType
        {
            get
            {
                return m_fieldType;
            }

            set
            {
                m_fieldType = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public object Value
        {
            get
            {
                return m_value;
            }

            set
            {
                m_value = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public NpgsqlDbType Type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ParameterDirection Direction
        {
            get
            {
                return m_direction;
            }

            set
            {
                m_direction = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dbp"></param>
        /// <returns></returns>
        public static implicit operator NpgsqlParameter(PgDatParameter p_dbp)
        {
            NpgsqlParameter _result = new NpgsqlParameter(p_dbp.Name, p_dbp.Value);

            _result.NpgsqlDbType = p_dbp.Type;
            _result.Direction = p_dbp.Direction;

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsNullOrEmpty
        {
            get
            {
                var _result = true;

                if (this.Value != null)
                {
                    _result = false;

                    if (this.Value is String)
                        _result = this.Value.ToString() == "";
                }

                return _result;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsNotNullValue
        {
            get
            {
                return (this.Value != null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Value != null)
                return this.Value.ToString();

            return "";
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}