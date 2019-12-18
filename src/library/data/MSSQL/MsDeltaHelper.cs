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
using System.Text;

//#pragma warning disable 1589, 1591

namespace OpenTax.Engine.Library.Data.MSSQL
{
    /// <summary>
    ///
    /// </summary>
    public class MsDeltaHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<MsDeltaHelper> m_lzyHelper = new Lazy<MsDeltaHelper>(() =>
        {
            return new MsDeltaHelper();
        });

        /// <summary>
        ///
        /// </summary>
        public static MsDeltaHelper SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Construct
        //-----------------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------------------------------
        // Private Properties
        //-----------------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------------------------------
        // Deleta Set
        //-----------------------------------------------------------------------------------------------------------------------------
        private SqlDbType _getSqlDbType(Type p_type)
        {
            SqlDbType _result = SqlDbType.NVarChar;

            switch (p_type.FullName)
            {
                case "System.String":
                    _result = SqlDbType.NVarChar;
                    break;

                //  case "System.String":
                //      _result = SqlDbType.Char;
                //      break;
                //  case "System.String":
                //      _result = SqlDbType.NChar;
                //      break;
                //  case "System.String":
                //      _result = SqlDbType.NText;
                //      break;

                case "System.Int64":
                    _result = SqlDbType.BigInt;
                    break;

                case "System.Array":
                    _result = SqlDbType.Binary;
                    break;

                case "System.Boolean":
                    _result = SqlDbType.Bit;
                    break;

                case "System.DateTime":
                    _result = SqlDbType.DateTime;
                    break;

                case "System.Decimal":
                    _result = SqlDbType.Decimal;
                    break;

                case "System.Double":
                    _result = SqlDbType.Float;
                    break;

                case "System.Byte[]":
                    _result = SqlDbType.Image;
                    break;

                case "System.Int32":
                    _result = SqlDbType.Int;
                    break;

                //                case "System.Decimal":
                //                    _result = SqlDbType.Money;
                //                    break;
                //

                case "System.Single":
                    _result = SqlDbType.Real;
                    break;

                //                case "System.DateTime":
                //                    _result = SqlDbType.SmallDateTime;
                //                    break;

                case "System.Int16":
                    _result = SqlDbType.SmallInt;
                    break;

                //                case "System.Decimal":
                //                    _result = SqlDbType.SmallMoney;
                //                    break;
                //
                //                case "System.String":
                //                    _result = SqlDbType.Text;
                //                    break;
                //
                //                case "System.Array":
                //                    _result = SqlDbType.Timestamp;
                //                    break;

                case "System.Byte":
                    _result = SqlDbType.TinyInt;
                    break;

                case "System.Guid":
                    _result = SqlDbType.UniqueIdentifier;
                    break;

                //                case "System.Array":
                //                    _result = SqlDbType.VarBinary;
                //                    break;
                //
                //                case "System.String":
                //                    _result = SqlDbType.VarChar;
                //                    break;

                case "Object":
                    _result = SqlDbType.Variant;
                    break;

                default:
                    _result = SqlDbType.Variant;
                    break;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dataTable"></param>
        /// <returns></returns>
        private string _bldDeltaSelSQL(DataTable p_dataTable)
        {
            return String.Format("SELECT * FROM {0} ", p_dataTable.TableName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_dataTable"></param>
        /// <returns></returns>
        private string _bldDeltaDelSQL(DataTable p_dataTable)
        {
            StringBuilder _wheres = new StringBuilder();

            DataColumn[] _keys = p_dataTable.PrimaryKey;
            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _columnName = _keys[_index].ColumnName;
                _wheres.Append(String.Format("[{0}] = @org_{0} AND ", _columnName));
            }

            var _result = "";

            if (_wheres.Length > 0)
                _result = String.Format
                        (
                            "DELETE FROM {0} \nWHERE {1}",
                            p_dataTable.TableName, _wheres.ToString().Substring(0, _wheres.Length - 5)
                        );
            else
                _result = "SELECT 1";

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_adddrow"></param>
        /// <returns></returns>
        private string _bldDeltaAddSQL(DataRow p_adddrow)
        {
            StringBuilder _addcol = new StringBuilder();
            StringBuilder _addval = new StringBuilder();

            DataTable _dt = p_adddrow.Table;
            foreach (DataColumn _dc in _dt.Columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() == "sfid" || _columnName.ToLower() == "slmd")
                {
                    _addcol.Append(String.Format("[{0}], ", _columnName));

                    if (p_adddrow[_columnName] == DBNull.Value)
                    {
                        _addval.Append("getdate(), ");
                    }
                    else
                    {
                        _addval.Append(String.Format("@{0}, ", _columnName));
                    }
                }
                else
                {
                    if (p_adddrow[_columnName] == DBNull.Value)
                        continue;
                    else
                    {
                        _addcol.Append(String.Format("[{0}], ", _columnName));
                        _addval.Append(String.Format("@{0}, ", _columnName));
                    }
                }
            }

            var _result = "";

            if (String.IsNullOrEmpty(_addcol.ToString()) == false && String.IsNullOrEmpty(_addval.ToString()) == false)
                _result = String.Format
                        (
                            "INSERT INTO {0} \n(\n{1}\n)\nVALUES\n(\n{2}\n)",
                            _dt.TableName, _addcol.ToString().Substring(0, _addcol.Length - 2), _addval.ToString().Substring(0, _addval.Length - 2)
                        );
            else
                _result = "SELECT 1";

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_updrow"></param>
        /// <returns></returns>
        private string _bldDeltaUpdSQL(DataRow p_updrow)
        {
            StringBuilder _updcol = new StringBuilder();
            StringBuilder _wheres = new StringBuilder();

            DataTable _dt = p_updrow.Table;
            foreach (DataColumn _dc in _dt.Columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() == "slmd")
                {
                    _updcol.Append(String.Format("[{0}] = getdate(), ", _columnName));

                    // 예전에는 업데이트 되는 필드(Set으로 정의)가 해당 테이블의 모든 필드에 적용이 되어
                    // 하나의 DataRow를 서로 다른 사람이 바꾸게 되면 마지막 사람의 DataRow가 적용되는 문제가 있어
                    // 해당 DataRow의 Server 날짜와 Clien t날짜를 비교하는 문장을 넣어 Server와 Client를 데이터가 서로 다른지를 확인했는데,
                    // _bldDeltaUpdSQL함수를 수정하여 변경된 Column만 Set으로 처리되므로 필요가 없게 되었음.
                    //_wheres.Append(String.Format("[{0}] = @org_{0} AND ", _columnName));
                }
                else
                {
                    object _original = p_updrow[_columnName, DataRowVersion.Original];
                    object _current = p_updrow[_columnName, DataRowVersion.Current];

                    if (_original.Equals(_current) == false)
                        _updcol.Append(String.Format("[{0}] = @{0}, ", _columnName));
                }
            }

            DataColumn[] _keys = _dt.PrimaryKey;
            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _columnName = _keys[_index].ColumnName;
                _wheres.Append(String.Format("[{0}] = @org_{0} AND ", _columnName));
            }

            var _result = "";

            if (_updcol.Length > 0 && _wheres.Length > 0)
                _result = String.Format
                          (
                            "UPDATE {0} \nSET {1}\nWHERE {2}",
                            _dt.TableName, _updcol.ToString().Substring(0, _updcol.Length - 2), _wheres.ToString().Substring(0, _wheres.Length - 5)
                          );
            else
                _result = "SELECT 1";

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_columns"></param>
        /// <param name="p_sqlcmd"></param>
        private void _bld_DeltaInsParm(DataColumnCollection p_columns, SqlCommand p_sqlcmd)
        {
            foreach (DataColumn _dc in p_columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;

                var _args = p_sqlcmd.Parameters.Add(new SqlParameter("@" + _columnName, _getSqlDbType(_dc.DataType)));
                {
                    _args.Direction = ParameterDirection.Input;
                    _args.SourceColumn = _columnName;
                    _args.SourceVersion = DataRowVersion.Current;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_columns"></param>
        /// <param name="p_sqlcmd"></param>
        private void _bld_DeltaUpdParm(DataColumnCollection p_columns, SqlCommand p_sqlcmd)
        {
            foreach (DataColumn _dc in p_columns)
            {
                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() != "slmd")
                {
                    var _args = p_sqlcmd.Parameters.Add(new SqlParameter("@" + _columnName, _getSqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Current;
                    }
                }
                else
                {
                    var _args = p_sqlcmd.Parameters.Add(new SqlParameter("@org_" + _columnName, _getSqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Original;
                    }
                }
            }

            foreach (DataColumn _kc in p_columns[0].Table.PrimaryKey)
            {
                var _args = p_sqlcmd.Parameters.Add(new SqlParameter("@org_" + _kc.ColumnName, _getSqlDbType(_kc.DataType)));
                {
                    _args.Direction = ParameterDirection.Input;
                    _args.SourceColumn = _kc.ColumnName;
                    _args.SourceVersion = DataRowVersion.Original;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_primaryKeys"></param>
        /// <param name="p_sqlcmd"></param>
        private void _bld_DeltaDelParm(DataColumn[] p_primaryKeys, SqlCommand p_sqlcmd)
        {
            foreach (DataColumn _kc in p_primaryKeys)
            {
                var _args = p_sqlcmd.Parameters.Add(new SqlParameter("@org_" + _kc.ColumnName, _getSqlDbType(_kc.DataType)));
                {
                    _args.Direction = ParameterDirection.Input;
                    _args.SourceColumn = _kc.ColumnName;
                    _args.SourceVersion = DataRowVersion.Original;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Select Delta DataSet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 테이블에 INSERT 하기 위한 스키마를 구하는 함수 입니다.
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_tableName"></param>
        /// <returns></returns>
        public DataSet SelectDeltaSchema(string p_connection_string, string p_tableName)
        {
            var _result = new DataSet();

            using (var _sqlcon = new SqlConnection(p_connection_string))
            {
                var _adapter = new SqlDataAdapter("SELECT * FROM " + p_tableName, _sqlcon);
                _adapter.FillSchema(_result, SchemaType.Source, p_tableName);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_cmdtxt"></param>
        /// <returns></returns>
        public DataSet SelectDeltaSet(string p_connection_string, string p_cmdtxt)
        {
            var _result = new DataSet();

            var _dbc = new MsDatCommand(p_cmdtxt);
            _result = MsSqlHelper.SelectDataSet(p_connection_string, CommandType.Text, p_cmdtxt, _dbc.Name);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Update or Insert Delta DataSet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_sqlTransaction"></param>
        /// <param name="p_deltaTbl"></param>
        /// <remarks>
        /// DataSet 내의 Table간의 Foreign Key 관계로 인하여
        /// 삭제는 detail -> master순서대로, 추가는 master -> detail 관계로 처리되어야 한다.
        /// 따라서 삭제부분은 DataTable_Delete()으로 따로 분리한다.
        /// </remarks>
        /// <returns></returns>
        private int DataTable_Update(SqlTransaction p_sqlTransaction, DataTable p_deltaTbl)
        {
            var _result = 0;

            using (var _deltacmd = new SqlDataAdapter(_bldDeltaSelSQL(p_deltaTbl), p_sqlTransaction.Connection))
            {
                _deltacmd.InsertCommand = new SqlCommand("", p_sqlTransaction.Connection);
                {
                    _deltacmd.InsertCommand.Transaction = p_sqlTransaction;
                    _bld_DeltaInsParm(p_deltaTbl.Columns, _deltacmd.InsertCommand);
                }

                _deltacmd.UpdateCommand = new SqlCommand("", p_sqlTransaction.Connection);
                {
                    _deltacmd.UpdateCommand.Transaction = p_sqlTransaction;
                    _bld_DeltaUpdParm(p_deltaTbl.Columns, _deltacmd.UpdateCommand);
                }

                _deltacmd.RowUpdating += _RowUpdating;

                _result += _deltacmd.Update(p_deltaTbl.Select(null, null, DataViewRowState.ModifiedCurrent));
                _result += _deltacmd.Update(p_deltaTbl.Select(null, null, DataViewRowState.Added));
            }

            return _result;
        }

        private int DataTable_Delete(SqlTransaction p_sqlTransaction, DataTable p_deltaTbl)
        {
            var _result = 0;

            using (var _deltacmd = new SqlDataAdapter(_bldDeltaSelSQL(p_deltaTbl), p_sqlTransaction.Connection))
            {
                _deltacmd.DeleteCommand = new SqlCommand(_bldDeltaDelSQL(p_deltaTbl), p_sqlTransaction.Connection);
                {
                    _deltacmd.DeleteCommand.Transaction = p_sqlTransaction;
                    _bld_DeltaDelParm(p_deltaTbl.PrimaryKey, _deltacmd.DeleteCommand);
                }

                _deltacmd.RowUpdating += _RowUpdating;

                _result += _deltacmd.Update(p_deltaTbl.Select(null, null, DataViewRowState.Deleted));
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _RowUpdating(object sender, SqlRowUpdatingEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Added)
            {
                SqlDataAdapter _adpater = (SqlDataAdapter)sender;

                _adpater.InsertCommand.CommandText = _bldDeltaAddSQL(e.Row);
            }
            else if (e.Row.RowState == DataRowState.Modified)
            {
                SqlDataAdapter _adpater = (SqlDataAdapter)sender;

                _adpater.UpdateCommand.CommandText = _bldDeltaUpdSQL(e.Row);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaSet"></param>
        /// <returns></returns>
        private int UpdateDataSet(string p_connection_string, DataSet p_deltaSet)
        {
            var _result = 0;

            using (var _sqlconn = new SqlConnection(p_connection_string))
            {
                _sqlconn.Open();
                var _sqltran = _sqlconn.BeginTransaction();

                try
                {
                    if (p_deltaSet.HasErrors == false && p_deltaSet.HasChanges() == true)
                    {
                        // DataSet 내의 Table간의 Foreign Key 관계로 인하여
                        // 삭제는 detail -> master순서대로, 추가는 master -> detail 관계로 처리되어야 한다.

                        // 삭제처리
                        for (int i = p_deltaSet.Tables.Count - 1; i >= 0; i--)
                            _result += DataTable_Delete(_sqltran, p_deltaSet.Tables[i]);

                        // 추가 및 변경
                        foreach (DataTable _table in p_deltaSet.Tables)
                            _result += DataTable_Update(_sqltran, _table);
                    }

                    if (p_deltaSet.HasErrors == false)
                    {
                        _sqltran.Commit();
                        p_deltaSet.AcceptChanges();
                    }
                    else
                    {
                        _sqltran.Rollback();
                        _result = -1;
                    }
                }
                catch (Exception ex)
                {
                    _sqltran.Rollback();
                    _result = -1;
                    throw new Exception("UpdateDataSet", ex);
                }
                finally
                {
                    _sqlconn.Close();
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaTbl"></param>
        /// <returns></returns>
        private int UpdateDataTable(string p_connection_string, DataTable p_deltaTbl)
        {
            var _result = 0;

            using (var _sqlconn = new SqlConnection(p_connection_string))
            {
                _sqlconn.Open();
                var _sqltran = _sqlconn.BeginTransaction();

                try
                {
                    if (p_deltaTbl.HasErrors == false)
                        _result += DataTable_Update(_sqltran, p_deltaTbl);

                    if (p_deltaTbl.HasErrors == false)
                    {
                        _sqltran.Commit();
                        p_deltaTbl.AcceptChanges();
                    }
                    else
                    {
                        _sqltran.Rollback();
                        _result = -1;
                    }
                }
                catch (Exception ex)
                {
                    _sqltran.Rollback();
                    _result = -1;
                    throw new Exception("UpdateDataTable", ex);
                }
                finally
                {
                    _sqlconn.Close();
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
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaSet"></param>
        /// <returns></returns>
        public int UpdateDeltaSet(string p_connection_string, DataSet p_deltaSet)
        {
            return UpdateDataSet(p_connection_string, p_deltaSet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaSet"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public int UpdateDeltaSet(string p_connection_string, DataSet p_deltaSet, out string p_message)
        {
            var _result = UpdateDeltaSet(p_connection_string, p_deltaSet);

            p_message = "";

            if (_result < 1)
            {
                foreach (DataTable _dt in p_deltaSet.Tables)
                {
                    foreach (DataRow _er in _dt.GetErrors())
                    {
                        p_message += _er.RowError;
                        break;
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaSet"></param>
        /// <returns></returns>
        public int InsertDeltaSet(string p_connection_string, DataSet p_deltaSet)
        {
            return UpdateDataSet(p_connection_string, p_deltaSet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaTbl"></param>
        /// <returns></returns>
        public int UpdateDeltaTbl(string p_connection_string, DataTable p_deltaTbl)
        {
            return UpdateDataTable(p_connection_string, p_deltaTbl);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_connection_string"></param>
        /// <param name="p_deltaTbl"></param>
        /// <returns></returns>
        public int InsertDeltaTbl(string p_connection_string, DataTable p_deltaTbl)
        {
            return UpdateDataTable(p_connection_string, p_deltaTbl);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}