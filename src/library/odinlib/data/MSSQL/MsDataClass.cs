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

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Data.MSSQL
{
    /// <summary>
    /// Class1에 대한 요약 설명입니다.
    /// </summary>
    public class MsDataClass
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        ///
        //-----------------------------------------------------------------------------------------------------------------------------
        public MsDataClass(string connection_string, string table_name)
        {
            ConnectionString = connection_string;
            TableName = table_name;

            m_data_helper = new Lazy<MsDataHelper>(() =>
            {
                return new MsDataHelper();
            });

            m_delta_helper = new Lazy<MsDeltaHelper>(() =>
            {
                return new MsDeltaHelper();
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Private Functions
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_connection_string = "";

        public string ConnectionString
        {
            get
            {
                return m_connection_string;
            }
            set
            {
                m_connection_string = value;
            }
        }

        private string m_tblName = "";

        public string TableName
        {
            get
            {
                return m_tblName;
            }
            set
            {
                m_tblName = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private Lazy<MsDataHelper> m_data_helper;

        public OdinSdk.OdinLib.Data.MSSQL.MsDataHelper DatHelper
        {
            get
            {
                return m_data_helper.Value;
            }
        }

        private Lazy<MsDeltaHelper> m_delta_helper;

        public OdinSdk.OdinLib.Data.MSSQL.MsDeltaHelper DltHelper
        {
            get
            {
                return m_delta_helper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet FillSchema()
        {
            return DatHelper.FillSchema(ConnectionString, m_tblName);
        }

        public int UpdateDeltaSet(DataSet dataset)
        {
            return DltHelper.UpdateDeltaSet(ConnectionString, dataset);
        }

        public DataSet ExecuteDataSet()
        {
            var _sqlstr = "SELECT * FROM " + m_tblName;
            return DatHelper.ExecuteDataSet(ConnectionString, _sqlstr);
        }

        public DataSet ExecuteDataSet(MsDatParameters db_params)
        {
            var _sqlstr = String.Format("SELECT * FROM {0}\n", m_tblName);

            var _index = 0;
            var _where = "";
            foreach (MsDatParameter _p in db_params)
            {
                if (String.IsNullOrEmpty(_where) == true)
                    _where += " WHERE ";

                if (String.IsNullOrEmpty(_p.Field) == true)
                    _where += String.Format("{0}={1}", _p.Name.Substring(1), _p.Name);
                else
                    _where += String.Format("{0}={1}", _p.Field, _p.Name);

                if (_index != db_params.Count - 1)
                    _where += " AND ";

                _index++;
            }
            _sqlstr += _where;

            return DatHelper.ExecuteDataSet(ConnectionString, _sqlstr, db_params);
        }

        public DataSet ExecuteDataSet(string sql_string, MsDatParameters db_params)
        {
            return DatHelper.ExecuteDataSet(ConnectionString, sql_string, db_params);
        }

        public bool CheckExistence(MsDatParameters db_params)
        {
            bool _result;

            DataSet _ds = ExecuteDataSet(db_params);
            if (DatHelper.IsNullOrEmpty(_ds) == false)
                _result = true;
            else
                _result = false;

            return _result;
        }

        public object SelectScalar(string sql_string, MsDatParameters db_params)
        {
            return DatHelper.SelectScalar(ConnectionString, sql_string, db_params);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}