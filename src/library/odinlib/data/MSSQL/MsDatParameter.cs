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
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace OdinSdk.OdinLib.Data.MSSQL
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(Namespace = "http://www.odinsoftware.co.kr/sdk/data/collection/MsDatParameter/2015/12")]
    [Serializable]
    public class MsDatParameter : ISerializable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public MsDatParameter()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db_type"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public MsDatParameter(string name, SqlDbType db_type, ParameterDirection direction, object value)
            : this(name, "", db_type, direction, value)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field_type"></param>
        /// <param name="db_type"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public MsDatParameter(string name, string field_type, SqlDbType db_type, ParameterDirection direction, object value)
        {
            m_name = name;

            m_fieldType = field_type;
            m_type = db_type;

            m_direction = direction;
            m_value = value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ISerialize
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="serialization_info"></param>
        /// <param name="streaming_context"></param>
        public MsDatParameter(SerializationInfo serialization_info, StreamingContext streaming_context)
        {
            if (serialization_info == null)
                throw new System.ArgumentNullException("info");

            m_name = (string)serialization_info.GetValue("name", typeof(string));
            m_field = (string)serialization_info.GetValue("field", typeof(string));

            m_fieldType = (string)serialization_info.GetValue("fieldType", typeof(string));
            m_type = (SqlDbType)serialization_info.GetValue("type", typeof(SqlDbType));

            m_direction = (ParameterDirection)serialization_info.GetValue("direction", typeof(ParameterDirection));
            m_value = (object)serialization_info.GetValue("value", typeof(object));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serialization_info"></param>
        /// <param name="streaming_context"></param>
        public virtual void GetObjectData(SerializationInfo serialization_info, StreamingContext streaming_context)
        {
            if (serialization_info == null)
                throw new System.ArgumentNullException("info");

            serialization_info.AddValue("name", m_name);
            serialization_info.AddValue("field", m_field);

            serialization_info.AddValue("fieldType", m_fieldType);
            serialization_info.AddValue("type", m_type);

            serialization_info.AddValue("direction", m_direction);
            serialization_info.AddValue("value", m_value);
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
        private SqlDbType m_type = SqlDbType.NVarChar;

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
        public SqlDbType Type
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
        /// <param name="db_param"></param>
        /// <returns></returns>
        public static implicit operator SqlParameter(MsDatParameter db_param)
        {
            SqlParameter _result = new SqlParameter(db_param.Name, db_param.Value);

            _result.SqlDbType = db_param.Type;
            _result.Direction = db_param.Direction;

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
            //throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}