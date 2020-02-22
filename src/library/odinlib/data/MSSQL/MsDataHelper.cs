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

using OdinSdk.OdinLib.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

//#pragma warning disable 1589, 1591

namespace OdinSdk.OdinLib.Data.MSSQL
{
    /// <summary>
    /// database helper class
    /// </summary>
    public class MsDataHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<MsDataHelper> m_lzyHelper = new Lazy<MsDataHelper>(() =>
        {
            return new MsDataHelper();
        });

        /// <summary></summary>
        public static MsDataHelper SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Private Properties
        //-----------------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Private Functions
        //-----------------------------------------------------------------------------------------------------------------------------
        private MsDeltaHelper m_delta_helper = null;

        private OdinSdk.OdinLib.Data.MSSQL.MsDeltaHelper DeltaHelper
        {
            get
            {
                if (m_delta_helper == null)
                    m_delta_helper = new MsDeltaHelper();
                return m_delta_helper;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _getFieldValue(Object object_value)
        {
            var _result = object_value.ToString();

            if (object_value.GetType() == typeof(DateTime))
            {
                _result
                    = Convert.ToDateTime(object_value).ToString("yyyy-MM-dd") + " "
                    + Convert.ToDateTime(object_value).Hour.ToString("00") + ":"
                    + Convert.ToDateTime(object_value).Minute.ToString("00") + ":"
                    + Convert.ToDateTime(object_value).Second.ToString("00");

                _result = String.Format("'{0}'", _result);
            }
            else if (object_value.GetType() == typeof(string))
            {
                _result = String.Format("'{0}'", _result.Replace("'", "''"));
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// _bldDataInsSQL : datarow와 테이블 컬럼, 키값을 보내 Insert 문장을 만드는 함수
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _bldDataInsSQL(DataRow data_rows)
        {
            var _result = "";

            var _addcol = new StringBuilder();
            var _addval = new StringBuilder();

            foreach (DataColumn _col in data_rows.Table.Columns)
            {
                if (_col.AutoIncrement == false)
                {
                    var _column = _col.ColumnName;
                    Object _object = data_rows[_column];

                    if (Convert.IsDBNull(_object) == false)
                    {
                        var _value = _getFieldValue(_object);

                        _addcol.Append(_column + ", ");
                        _addval.Append(_value + ", ");
                    }
                    else if (_column == "sfid" || _column == "slmd")
                    {
                        _addcol.Append(_column + ", ");
                        _addval.Append("getdate(), ");
                    }
                }
            }

            if (_addcol.ToString() != "" && _addval.ToString() != "")
            {
                _result = ""
                    + " INSERT INTO [" + data_rows.Table.TableName + "] "
                    + " ("
                    + _addcol.ToString().Substring(0, _addcol.Length - 2)
                    + " ) "
                    + " VALUES "
                    + " ( "
                    + _addval.ToString().Substring(0, _addval.Length - 2)
                    + " ) ";
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// _bldDataDelSQL : datarow와 테이블 컬럼, 키값을 보내 Delete 문장을 만드는 함수
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _bldDataDelSQL(DataRow data_rows)
        {
            var _result = "";

            var _where = new StringBuilder();
            DataColumn[] _keys = data_rows.Table.PrimaryKey;

            for (int _key = 0; _keys.Length > _key; _key++)
            {
                var _column = _keys[_key].ColumnName;
                Object _object = data_rows[_column, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("[{0}] = {1} AND ", _column, _value));
            }

            if (_where.Length > 0)
            {
                _result
                    = " DELETE FROM [" + data_rows.Table.TableName + "] "
                    + " WHERE " + _where.ToString().Substring(0, _where.Length - 5);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// _bldDataUpdSQL : datarow와 테이블 컬럼, 키값을 보내 Update 문장을 만드는 함수
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _bldDataUpdSQL(DataRow data_rows)
        {
            var _result = "";

            var _updcol = new StringBuilder();
            var _where = new StringBuilder();

            foreach (DataColumn _col in data_rows.Table.Columns)
            {
                if (_col.AutoIncrement == false)
                {
                    var _column = _col.ColumnName;

                    if (_column == "slmd")
                    {
                        _updcol.Append(_column + " = getdate(), ");
                    }
                    else
                    {
                        Object _object = data_rows[_column];

                        if (Convert.IsDBNull(_object) == false)
                        {
                            var _value = _getFieldValue(_object);
                            _updcol.Append(String.Format("[{0}] = {1}, ", _column, _value));
                        }
                        else
                        {
                            _updcol.Append(_column + " = null, ");
                        }
                    }
                }
            }

            DataColumn[] _keys = data_rows.Table.PrimaryKey;

            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _column = _keys[_index].ColumnName;
                Object _object = data_rows[_column, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("[{0}] = {1} AND ", _column, _value));
            }

            if (_updcol.ToString() != "" && _where.ToString() != "")
            {
                _result
                    = "UPDATE [" + data_rows.Table.TableName + "] "
                    + "   SET " + _updcol.ToString().Substring(0, _updcol.Length - 2)
                    + " WHERE " + _where.ToString().Substring(0, _where.Length - 5);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Build Database Commands
        //-----------------------------------------------------------------------------------------------------------------------------
        private MsDatCommands buildCommands(DataSet dataset)
        {
            MsDatCommands _dbcs = new MsDatCommands();

            foreach (DataTable _table in dataset.Tables)
            {
                foreach (DataRow _row in _table.Rows)
                {
                    switch (_row.RowState)
                    {
                        case DataRowState.Added:
                            _dbcs.Add(_bldDataInsSQL(_row), (MsDatParameters)null);
                            break;

                        case DataRowState.Deleted:
                            _dbcs.Add(_bldDataDelSQL(_row), (MsDatParameters)null);
                            break;

                        case DataRowState.Modified:
                            _dbcs.Add(_bldDataUpdSQL(_row), (MsDatParameters)null);
                            break;
                    }
                }
            }

            return _dbcs;
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private void _returnParameterValue(SqlParameter[] commandParameters, MsDatParameters parameterValues)
        {
            if (commandParameters != null && parameterValues != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    SqlParameter _cp = commandParameters[i];
                    if (_cp.Direction == ParameterDirection.Input)
                        continue;

                    //Update the Return Value
                    if (_cp.Direction == ParameterDirection.ReturnValue)
                    {
                        foreach (MsDatParameter _pv in parameterValues)
                        {
                            if (_pv.Direction == ParameterDirection.ReturnValue)
                            {
                                _pv.Value = _cp.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (MsDatParameter _pv in parameterValues)
                        {
                            if (_pv.Name.ToLower() == _cp.ParameterName.ToLower())
                            {
                                _pv.Value = _cp.Value;
                                break;
                            }
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Public functions
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="table_name"></param>
        /// <returns></returns>
        public DataSet FillSchema(string connection_string, string table_name)
        {
            var _result = new DataSet();

            using (SqlConnection _sqlcon = new SqlConnection(connection_string))
            {
                SqlDataAdapter _adapter = new SqlDataAdapter("SELECT * FROM " + table_name, _sqlcon);
                _adapter.FillSchema(_result, SchemaType.Source, table_name);
            }

            return _result;
        }

        /// <summary>
        /// 데이터셋에 한개 이상의 테이블이 있는지와 각각의 테이블에 한개이상의 row가 있는지 확인 합니다.
        /// 모든 테이블에 record가 한개도 없으면 true 입니다.
        /// </summary>
        /// <param name="orgin_data_set"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataSet orgin_data_set)
        {
            var _result = true;

            if (orgin_data_set != null && orgin_data_set.Tables != null && orgin_data_set.Tables.Count > 0)
            {
                foreach (DataTable _dt in orgin_data_set.Tables)
                {
                    if (_dt.Rows.Count > 0)
                    {
                        _result = false;
                        break;
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// 데이터셋의 특정 테이블에 한개 이상의 row가 있는지 확인 합니다.
        /// </summary>
        /// <param name="orgin_data_set"></param>
        /// <param name="table_index"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataSet orgin_data_set, int table_index)
        {
            var _result = true;

            if (orgin_data_set != null && orgin_data_set.Tables != null && orgin_data_set.Tables.Count > table_index)
            {
                DataTable _dt = orgin_data_set.Tables[table_index];
                if (_dt.Rows.Count > 0)
                    _result = false;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orgin_data_table"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataTable orgin_data_table)
        {
            var _result = true;

            if (orgin_data_table != null && orgin_data_table.Rows.Count > 0)
                _result = false;

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNullValue(MsDatParameter value)
        {
            var _result = true;

            if (value != null && value.Value != null)
            {
                if (String.IsNullOrEmpty(value.Value.ToString()) == false)
                    _result = false;
            }

            return _result;
        }

        /// <summary>
        /// 데이터셋 내의 특정 테이블이 몇개의 record를 가지고 있는지 확인 합니다.
        /// </summary>
        /// <param name="orgin_data_set"></param>
        /// <param name="table_index"></param>
        /// <returns></returns>
        public int RowCount(DataSet orgin_data_set, int table_index)
        {
            var _result = -1;

            if (orgin_data_set != null && orgin_data_set.Tables != null && orgin_data_set.Tables.Count > table_index)
            {
                DataTable _dt = orgin_data_set.Tables[table_index];
                _result = _dt.Rows.Count;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// SelectDataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet SelectDataSet(string connection_string, string command_text)
        {
            MsDatCommand _dbc = new MsDatCommand(command_text);
            return MsSqlHelper.SelectDataSet(connection_string, CommandType.Text, command_text, _dbc.Name);
        }

        public DataSet SelectDataSet(string connection_string, string command_text, MsDatParameters db_params)
        {
            MsDatCommand _dbc = new MsDatCommand(command_text);
            return MsSqlHelper.SelectDataSet(connection_string, CommandType.Text, command_text, _dbc.Name, (SqlParameter[])db_params);
        }

        public DataSet SelectDataSet(string connection_string, MsDatCommands db_commands)
        {
            var _result = new DataSet();

            using (SqlConnection _sqlconn = new SqlConnection(connection_string))
            {
                _sqlconn.Open();

                foreach (MsDatCommand _dbc in db_commands)
                    _result.Merge(MsSqlHelper.SelectDataSet(_sqlconn, CommandType.Text, _dbc.Text, _dbc.Name));
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// SelectScalar
        //-----------------------------------------------------------------------------------------------------------------------------
        public object SelectScalar(string connection_string, string command_text, MsDatParameters db_params)
        {
            return MsSqlHelper.ExecuteScalar(connection_string, CommandType.Text, command_text, (SqlParameter[])db_params);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteReader
        //-----------------------------------------------------------------------------------------------------------------------------
        public SqlDataReader ExecuteReader(string connection_string, string command_text)
        {
            return MsSqlHelper.ExecuteReader(connection_string, CommandType.Text, command_text);
        }

        public SqlDataReader ExecuteReader(string connection_string, string command_text, MsDatParameters db_params)
        {
            return MsSqlHelper.ExecuteReader(connection_string, CommandType.Text, command_text, (SqlParameter[])db_params);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteText
        //-----------------------------------------------------------------------------------------------------------------------------
        public int ExecuteText(string connection_string, string command_text)
        {
            return MsSqlHelper.ExecuteNonQuery(connection_string, CommandType.Text, command_text);
        }

        public int ExecuteText(string connection_string, string command_text, MsDatParameters db_params)
        {
            return MsSqlHelper.ExecuteNonQuery(connection_string, CommandType.Text, command_text, (SqlParameter[])db_params);
        }

        public int ExecuteText(string connection_string, MsDatCommands db_commands)
        {
            var _result = 0;

            foreach (MsDatCommand _dbc in db_commands)
            {
                MsSqlHelper.ExecuteNonQuery(connection_string, CommandType.Text, _dbc.Text, (SqlParameter[])_dbc.Value);

                _result++;
            }

            return _result;
        }

        public int ExecuteText(string connection_string, params string[] command_texts)
        {
            var _result = 0;

            using (SqlConnection _sqlconn = new SqlConnection(connection_string))
            {
                _sqlconn.Open();
                SqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < command_texts.Length; i++)
                    {
                        if (command_texts[i] != null && command_texts[i] != "")
                        {
                            MsSqlHelper.ExecuteNonQuery(transaction, CommandType.Text, command_texts[i]);

                            _result++;
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _result = -1;

                    throw new Exception("ExecuteText", ex);
                }
                finally
                {
                    _sqlconn.Close();
                }
            }

            return _result;
        }

        public int ExecuteText(string connection_string, MsDatParameters db_params, params string[] command_texts)
        {
            var _result = 0;

            using (SqlConnection _sqlconn = new SqlConnection(connection_string))
            {
                _sqlconn.Open();
                SqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < command_texts.Length; i++)
                    {
                        if (command_texts[i] != null && command_texts[i] != "")
                        {
                            MsSqlHelper.ExecuteNonQuery(transaction, CommandType.Text, command_texts[i], (SqlParameter[])db_params);
                            _result++;
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _result = -1;

                    throw new Exception("ExecuteText", ex);
                }
                finally
                {
                    _sqlconn.Close();
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ExecuteProc
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <returns></returns>
        public MsDatParameters GetSqlParameters(string connection_string, string stored_procedure_name)
        {
            var _dbps = new MsDatParameters();

            SqlParameter[] _sbps = SqlHelperParameterCache.GetSpParameterSet(connection_string, stored_procedure_name);
            foreach (SqlParameter _s in _sbps)
            {
                _dbps.Add(_s.ParameterName, _s.SqlDbType, _s.Direction, DBNull.Value);
            }

            return _dbps;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ExecuteProc(string connection_string, string stored_procedure_name, params object[] args)
        {
            return MsSqlHelper.ExecuteNonQuery(connection_string, stored_procedure_name, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public int ExecuteProc(string connection_string, string stored_procedure_name, MsDatParameters db_params)
        {
            var _result = -1;

            SqlParameter[] _params = (SqlParameter[])db_params;
            _result = MsSqlHelper.ExecuteProcQuery(connection_string, stored_procedure_name, _params);
            {
                _returnParameterValue(_params, db_params);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public DataSet ExecuteProcSet(string connection_string, string stored_procedure_name, MsDatParameters db_params)
        {
            var _result = new DataSet();

            SqlParameter[] _params = (SqlParameter[])db_params;
            _result = MsSqlHelper.ExecuteProcSet(connection_string, stored_procedure_name, _params);
            {
                _returnParameterValue(_params, db_params);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteDataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet ExecuteDataSet(string connection_string, string command_text)
        {
            return MsSqlHelper.ExecuteDataSet(connection_string, CommandType.Text, command_text);
        }

        public DataSet ExecuteDataSet(string connection_string, string command_text, MsDatParameters db_params)
        {
            return MsSqlHelper.ExecuteDataSet(connection_string, CommandType.Text, command_text, (SqlParameter[])db_params);
        }

        public DataSet ExecuteDataSet(string connection_string, string stored_procedure_name, params object[] args)
        {
            return MsSqlHelper.ExecuteDataSet(connection_string, stored_procedure_name, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Update DataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public int UpdateDataSet(string connection_string, DataSet dataset)
        {
            return DeltaHelper.UpdateDeltaSet(connection_string, dataset);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        ///
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataRow NewDataRow(DataTable data_table)
        {
            DataRow _result = data_table.NewRow();
            foreach (DataColumn _dc in data_table.Columns)
            {
                if (_dc.AllowDBNull == false)
                {
                    if (_dc.DataType == typeof(System.DateTime))
                    {
                        _result[_dc.ColumnName] = CUnixTime.UtcNow;
                    }
                    else if (_dc.DataType == typeof(System.String))
                    {
                        _result[_dc.ColumnName] = "";
                    }
                    else if (_dc.DataType == typeof(System.Int32))
                    {
                        _result[_dc.ColumnName] = 0;
                    }
                    else if (_dc.DataType == typeof(System.Boolean))
                    {
                        _result[_dc.ColumnName] = false;
                    }
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="data_set"></param>
        /// <returns></returns>
        public string ZipSet(DataSet data_set)
        {
            if (IsNullOrEmpty(data_set) == true)
                return "";

            return ZipDataSet.SNG.CompressDataSet(data_set);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="zip_dataset"></param>
        /// <returns></returns>
        public DataSet UnZipSet(string zip_dataset)
        {
            if (String.IsNullOrEmpty(zip_dataset) == true)
                return null;

            return ZipDataSet.SNG.DecompressDataSet(zip_dataset);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}