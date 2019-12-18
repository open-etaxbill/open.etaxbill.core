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

using OdinSdk.BaseLib.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

//#pragma warning disable 1589, 1591

namespace OpenTax.Engine.Library.Data.MSSQL
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

        private OpenTax.Engine.Library.Data.MSSQL.MsDeltaHelper DeltaHelper
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
        private string _getFieldValue(Object p_object)
        {
            var _result = p_object.ToString();

            if (p_object.GetType() == typeof(DateTime))
            {
                _result
                    = Convert.ToDateTime(p_object).ToString("yyyy-MM-dd") + " "
                    + Convert.ToDateTime(p_object).Hour.ToString("00") + ":"
                    + Convert.ToDateTime(p_object).Minute.ToString("00") + ":"
                    + Convert.ToDateTime(p_object).Second.ToString("00");

                _result = String.Format("'{0}'", _result);
            }
            else if (p_object.GetType() == typeof(string))
            {
                _result = String.Format("'{0}'", _result.Replace("'", "''"));
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// _bldDataInsSQL : datarow와 테이블 컬럼, 키값을 보내 Insert 문장을 만드는 함수
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _bldDataInsSQL(DataRow p_rows)
        {
            var _result = "";

            var _addcol = new StringBuilder();
            var _addval = new StringBuilder();

            foreach (DataColumn _col in p_rows.Table.Columns)
            {
                if (_col.AutoIncrement == false)
                {
                    var _column = _col.ColumnName;
                    Object _object = p_rows[_column];

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
                    + " INSERT INTO [" + p_rows.Table.TableName + "] "
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
        private string _bldDataDelSQL(DataRow p_rows)
        {
            var _result = "";

            var _where = new StringBuilder();
            DataColumn[] _keys = p_rows.Table.PrimaryKey;

            for (int _key = 0; _keys.Length > _key; _key++)
            {
                var _column = _keys[_key].ColumnName;
                Object _object = p_rows[_column, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("[{0}] = {1} AND ", _column, _value));
            }

            if (_where.Length > 0)
            {
                _result
                    = " DELETE FROM [" + p_rows.Table.TableName + "] "
                    + " WHERE " + _where.ToString().Substring(0, _where.Length - 5);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// _bldDataUpdSQL : datarow와 테이블 컬럼, 키값을 보내 Update 문장을 만드는 함수
        //-----------------------------------------------------------------------------------------------------------------------------
        private string _bldDataUpdSQL(DataRow p_rows)
        {
            var _result = "";

            var _updcol = new StringBuilder();
            var _where = new StringBuilder();

            foreach (DataColumn _col in p_rows.Table.Columns)
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
                        Object _object = p_rows[_column];

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

            DataColumn[] _keys = p_rows.Table.PrimaryKey;

            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _column = _keys[_index].ColumnName;
                Object _object = p_rows[_column, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("[{0}] = {1} AND ", _column, _value));
            }

            if (_updcol.ToString() != "" && _where.ToString() != "")
            {
                _result
                    = "UPDATE [" + p_rows.Table.TableName + "] "
                    + "   SET " + _updcol.ToString().Substring(0, _updcol.Length - 2)
                    + " WHERE " + _where.ToString().Substring(0, _where.Length - 5);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Build Database Commands
        //-----------------------------------------------------------------------------------------------------------------------------
        private MsDatCommands buildCommands(DataSet p_dataSet)
        {
            MsDatCommands _dbcs = new MsDatCommands();

            foreach (DataTable _table in p_dataSet.Tables)
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
        /// <param name="p_connection_string"></param>
        /// <param name="p_tablename"></param>
        /// <returns></returns>
        public DataSet FillSchema(string p_connection_string, string p_tablename)
        {
            var _result = new DataSet();

            using (SqlConnection _sqlcon = new SqlConnection(p_connection_string))
            {
                SqlDataAdapter _adapter = new SqlDataAdapter("SELECT * FROM " + p_tablename, _sqlcon);
                _adapter.FillSchema(_result, SchemaType.Source, p_tablename);
            }

            return _result;
        }

        /// <summary>
        /// 데이터셋에 한개 이상의 테이블이 있는지와 각각의 테이블에 한개이상의 row가 있는지 확인 합니다.
        /// 모든 테이블에 record가 한개도 없으면 true 입니다.
        /// </summary>
        /// <param name="p_orgset"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataSet p_orgset)
        {
            var _result = true;

            if (p_orgset != null && p_orgset.Tables != null && p_orgset.Tables.Count > 0)
            {
                foreach (DataTable _dt in p_orgset.Tables)
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
        /// <param name="p_orgset"></param>
        /// <param name="p_tableNdx"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataSet p_orgset, int p_tableNdx)
        {
            var _result = true;

            if (p_orgset != null && p_orgset.Tables != null && p_orgset.Tables.Count > p_tableNdx)
            {
                DataTable _dt = p_orgset.Tables[p_tableNdx];
                if (_dt.Rows.Count > 0)
                    _result = false;
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_orgtbl"></param>
        /// <returns></returns>
        public bool IsNullOrEmpty(DataTable p_orgtbl)
        {
            var _result = true;

            if (p_orgtbl != null && p_orgtbl.Rows.Count > 0)
                _result = false;

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_value"></param>
        /// <returns></returns>
        public bool IsNullValue(MsDatParameter p_value)
        {
            var _result = true;

            if (p_value != null && p_value.Value != null)
            {
                if (String.IsNullOrEmpty(p_value.Value.ToString()) == false)
                    _result = false;
            }

            return _result;
        }

        /// <summary>
        /// 데이터셋 내의 특정 테이블이 몇개의 record를 가지고 있는지 확인 합니다.
        /// </summary>
        /// <param name="p_orgset"></param>
        /// <param name="p_tableNdx"></param>
        /// <returns></returns>
        public int RowCount(DataSet p_orgset, int p_tableNdx)
        {
            var _result = -1;

            if (p_orgset != null && p_orgset.Tables != null && p_orgset.Tables.Count > p_tableNdx)
            {
                DataTable _dt = p_orgset.Tables[p_tableNdx];
                _result = _dt.Rows.Count;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// SelectDataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet SelectDataSet(string p_connection_string, string p_cmdtxt)
        {
            MsDatCommand _dbc = new MsDatCommand(p_cmdtxt);
            return MsSqlHelper.SelectDataSet(p_connection_string, CommandType.Text, p_cmdtxt, _dbc.Name);
        }

        public DataSet SelectDataSet(string p_connection_string, string p_cmdtxt, MsDatParameters p_dbps)
        {
            MsDatCommand _dbc = new MsDatCommand(p_cmdtxt);
            return MsSqlHelper.SelectDataSet(p_connection_string, CommandType.Text, p_cmdtxt, _dbc.Name, (SqlParameter[])p_dbps);
        }

        public DataSet SelectDataSet(string p_connection_string, MsDatCommands p_dbcs)
        {
            var _result = new DataSet();

            using (SqlConnection _sqlconn = new SqlConnection(p_connection_string))
            {
                _sqlconn.Open();

                foreach (MsDatCommand _dbc in p_dbcs)
                    _result.Merge(MsSqlHelper.SelectDataSet(_sqlconn, CommandType.Text, _dbc.Text, _dbc.Name));
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// SelectScalar
        //-----------------------------------------------------------------------------------------------------------------------------
        public object SelectScalar(string p_connection_string, string p_cmdtxt, MsDatParameters p_dbps)
        {
            return MsSqlHelper.ExecuteScalar(p_connection_string, CommandType.Text, p_cmdtxt, (SqlParameter[])p_dbps);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteReader
        //-----------------------------------------------------------------------------------------------------------------------------
        public SqlDataReader ExecuteReader(string p_connection_string, string p_cmdtxt)
        {
            return MsSqlHelper.ExecuteReader(p_connection_string, CommandType.Text, p_cmdtxt);
        }

        public SqlDataReader ExecuteReader(string p_connection_string, string p_cmdtxt, MsDatParameters p_dbps)
        {
            return MsSqlHelper.ExecuteReader(p_connection_string, CommandType.Text, p_cmdtxt, (SqlParameter[])p_dbps);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteText
        //-----------------------------------------------------------------------------------------------------------------------------
        public int ExecuteText(string p_connection_string, string p_cmdtxt)
        {
            return MsSqlHelper.ExecuteNonQuery(p_connection_string, CommandType.Text, p_cmdtxt);
        }

        public int ExecuteText(string p_connection_string, string p_cmdtxt, MsDatParameters p_dbps)
        {
            return MsSqlHelper.ExecuteNonQuery(p_connection_string, CommandType.Text, p_cmdtxt, (SqlParameter[])p_dbps);
        }

        public int ExecuteText(string p_connection_string, MsDatCommands p_dbcs)
        {
            var _result = 0;

            foreach (MsDatCommand _dbc in p_dbcs)
            {
                MsSqlHelper.ExecuteNonQuery(p_connection_string, CommandType.Text, _dbc.Text, (SqlParameter[])_dbc.Value);

                _result++;
            }

            return _result;
        }

        public int ExecuteText(string p_connection_string, params string[] p_cmdtxts)
        {
            var _result = 0;

            using (SqlConnection _sqlconn = new SqlConnection(p_connection_string))
            {
                _sqlconn.Open();
                SqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < p_cmdtxts.Length; i++)
                    {
                        if (p_cmdtxts[i] != null && p_cmdtxts[i] != "")
                        {
                            MsSqlHelper.ExecuteNonQuery(transaction, CommandType.Text, p_cmdtxts[i]);

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

        public int ExecuteText(string p_connection_string, MsDatParameters p_dbps, params string[] p_cmdtxts)
        {
            var _result = 0;

            using (SqlConnection _sqlconn = new SqlConnection(p_connection_string))
            {
                _sqlconn.Open();
                SqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < p_cmdtxts.Length; i++)
                    {
                        if (p_cmdtxts[i] != null && p_cmdtxts[i] != "")
                        {
                            MsSqlHelper.ExecuteNonQuery(transaction, CommandType.Text, p_cmdtxts[i], (SqlParameter[])p_dbps);
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
        /// <param name="p_connection_string"></param>
        /// <param name="p_spName"></param>
        /// <returns></returns>
        public MsDatParameters GetSqlParameters(string p_connection_string, string p_spName)
        {
            var _dbps = new MsDatParameters();

            SqlParameter[] _sbps = SqlHelperParameterCache.GetSpParameterSet(p_connection_string, p_spName);
            foreach (SqlParameter _s in _sbps)
            {
                _dbps.Add(_s.ParameterName, _s.SqlDbType, _s.Direction, DBNull.Value);
            }

            return _dbps;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_spName"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        public int ExecuteProc(string p_connection_string, string p_spName, params object[] p_args)
        {
            return MsSqlHelper.ExecuteNonQuery(p_connection_string, p_spName, p_args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_spName"></param>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public int ExecuteProc(string p_connection_string, string p_spName, MsDatParameters p_dbps)
        {
            var _result = -1;

            SqlParameter[] _params = (SqlParameter[])p_dbps;
            _result = MsSqlHelper.ExecuteProcQuery(p_connection_string, p_spName, _params);
            {
                _returnParameterValue(_params, p_dbps);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_spName"></param>
        /// <param name="p_dbps"></param>
        /// <returns></returns>
        public DataSet ExecuteProcSet(string p_connection_string, string p_spName, MsDatParameters p_dbps)
        {
            var _result = new DataSet();

            SqlParameter[] _params = (SqlParameter[])p_dbps;
            _result = MsSqlHelper.ExecuteProcSet(p_connection_string, p_spName, _params);
            {
                _returnParameterValue(_params, p_dbps);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// ExecuteDataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataSet ExecuteDataSet(string p_connection_string, string p_cmdtxt)
        {
            return MsSqlHelper.ExecuteDataSet(p_connection_string, CommandType.Text, p_cmdtxt);
        }

        public DataSet ExecuteDataSet(string p_connection_string, string p_cmdtxt, MsDatParameters p_dbps)
        {
            return MsSqlHelper.ExecuteDataSet(p_connection_string, CommandType.Text, p_cmdtxt, (SqlParameter[])p_dbps);
        }

        public DataSet ExecuteDataSet(string p_connection_string, string p_spName, params object[] p_args)
        {
            return MsSqlHelper.ExecuteDataSet(p_connection_string, p_spName, p_args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        /// Update DataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        public int UpdateDataSet(string p_connection_string, DataSet p_dataSet)
        {
            return DeltaHelper.UpdateDeltaSet(p_connection_string, p_dataSet);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        ///
        //-----------------------------------------------------------------------------------------------------------------------------
        public DataRow NewDataRow(DataTable p_datatable)
        {
            DataRow _result = p_datatable.NewRow();
            foreach (DataColumn _dc in p_datatable.Columns)
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
        /// <param name="p_datset"></param>
        /// <returns></returns>
        public string ZipSet(DataSet p_datset)
        {
            if (IsNullOrEmpty(p_datset) == true)
                return "";

            return ZipDataSet.SNG.CompressDataSet(p_datset);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_zipset"></param>
        /// <returns></returns>
        public DataSet UnZipSet(string p_zipset)
        {
            if (String.IsNullOrEmpty(p_zipset) == true)
                return null;

            return ZipDataSet.SNG.DecompressDataSet(p_zipset);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}