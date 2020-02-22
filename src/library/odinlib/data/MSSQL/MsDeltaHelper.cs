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

namespace OdinSdk.OdinLib.Data.MSSQL
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
        private SqlDbType _getSqlDbType(Type type)
        {
            SqlDbType _result = SqlDbType.NVarChar;

            switch (type.FullName)
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
        /// <param name="data_table"></param>
        /// <returns></returns>
        private string _bldDeltaSelSQL(DataTable data_table)
        {
            return String.Format("SELECT * FROM {0} ", data_table.TableName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data_table"></param>
        /// <returns></returns>
        private string _bldDeltaDelSQL(DataTable data_table)
        {
            StringBuilder _wheres = new StringBuilder();

            DataColumn[] _keys = data_table.PrimaryKey;
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
                            data_table.TableName, _wheres.ToString().Substring(0, _wheres.Length - 5)
                        );
            else
                _result = "SELECT 1";

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="add_data_row"></param>
        /// <returns></returns>
        private string _bldDeltaAddSQL(DataRow add_data_row)
        {
            StringBuilder _addcol = new StringBuilder();
            StringBuilder _addval = new StringBuilder();

            DataTable _dt = add_data_row.Table;
            foreach (DataColumn _dc in _dt.Columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() == "sfid" || _columnName.ToLower() == "slmd")
                {
                    _addcol.Append(String.Format("[{0}], ", _columnName));

                    if (add_data_row[_columnName] == DBNull.Value)
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
                    if (add_data_row[_columnName] == DBNull.Value)
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
        /// <param name="update_data_row"></param>
        /// <returns></returns>
        private string _bldDeltaUpdSQL(DataRow update_data_row)
        {
            StringBuilder _updcol = new StringBuilder();
            StringBuilder _wheres = new StringBuilder();

            DataTable _dt = update_data_row.Table;
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
                    object _original = update_data_row[_columnName, DataRowVersion.Original];
                    object _current = update_data_row[_columnName, DataRowVersion.Current];

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
        /// <param name="data_columns"></param>
        /// <param name="sql_command"></param>
        private void _bld_DeltaInsParm(DataColumnCollection data_columns, SqlCommand sql_command)
        {
            foreach (DataColumn _dc in data_columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;

                var _args = sql_command.Parameters.Add(new SqlParameter("@" + _columnName, _getSqlDbType(_dc.DataType)));
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
        /// <param name="data_columns"></param>
        /// <param name="sql_command"></param>
        private void _bld_DeltaUpdParm(DataColumnCollection data_columns, SqlCommand sql_command)
        {
            foreach (DataColumn _dc in data_columns)
            {
                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() != "slmd")
                {
                    var _args = sql_command.Parameters.Add(new SqlParameter("@" + _columnName, _getSqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Current;
                    }
                }
                else
                {
                    var _args = sql_command.Parameters.Add(new SqlParameter("@org_" + _columnName, _getSqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Original;
                    }
                }
            }

            foreach (DataColumn _kc in data_columns[0].Table.PrimaryKey)
            {
                var _args = sql_command.Parameters.Add(new SqlParameter("@org_" + _kc.ColumnName, _getSqlDbType(_kc.DataType)));
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
        /// <param name="primary_keys"></param>
        /// <param name="sql_command"></param>
        private void _bld_DeltaDelParm(DataColumn[] primary_keys, SqlCommand sql_command)
        {
            foreach (DataColumn _kc in primary_keys)
            {
                var _args = sql_command.Parameters.Add(new SqlParameter("@org_" + _kc.ColumnName, _getSqlDbType(_kc.DataType)));
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
        /// <param name="connection_string"></param>
        /// <param name="table_name"></param>
        /// <returns></returns>
        public DataSet SelectDeltaSchema(string connection_string, string table_name)
        {
            var _result = new DataSet();

            using (var _sqlcon = new SqlConnection(connection_string))
            {
                var _adapter = new SqlDataAdapter("SELECT * FROM " + table_name, _sqlcon);
                _adapter.FillSchema(_result, SchemaType.Source, table_name);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <returns></returns>
        public DataSet SelectDeltaSet(string connection_string, string command_text)
        {
            var _result = new DataSet();

            var _dbc = new MsDatCommand(command_text);
            _result = MsSqlHelper.SelectDataSet(connection_string, CommandType.Text, command_text, _dbc.Name);

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Update or Insert Delta DataSet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="sql_transaction"></param>
        /// <param name="delta_table"></param>
        /// <remarks>
        /// DataSet 내의 Table간의 Foreign Key 관계로 인하여
        /// 삭제는 detail -> master순서대로, 추가는 master -> detail 관계로 처리되어야 한다.
        /// 따라서 삭제부분은 DataTable_Delete()으로 따로 분리한다.
        /// </remarks>
        /// <returns></returns>
        private int DataTable_Update(SqlTransaction sql_transaction, DataTable delta_table)
        {
            var _result = 0;

            using (var _deltacmd = new SqlDataAdapter(_bldDeltaSelSQL(delta_table), sql_transaction.Connection))
            {
                _deltacmd.InsertCommand = new SqlCommand("", sql_transaction.Connection);
                {
                    _deltacmd.InsertCommand.Transaction = sql_transaction;
                    _bld_DeltaInsParm(delta_table.Columns, _deltacmd.InsertCommand);
                }

                _deltacmd.UpdateCommand = new SqlCommand("", sql_transaction.Connection);
                {
                    _deltacmd.UpdateCommand.Transaction = sql_transaction;
                    _bld_DeltaUpdParm(delta_table.Columns, _deltacmd.UpdateCommand);
                }

                _deltacmd.RowUpdating += _RowUpdating;

                _result += _deltacmd.Update(delta_table.Select(null, null, DataViewRowState.ModifiedCurrent));
                _result += _deltacmd.Update(delta_table.Select(null, null, DataViewRowState.Added));
            }

            return _result;
        }

        private int DataTable_Delete(SqlTransaction sql_transaction, DataTable delta_table)
        {
            var _result = 0;

            using (var _deltacmd = new SqlDataAdapter(_bldDeltaSelSQL(delta_table), sql_transaction.Connection))
            {
                _deltacmd.DeleteCommand = new SqlCommand(_bldDeltaDelSQL(delta_table), sql_transaction.Connection);
                {
                    _deltacmd.DeleteCommand.Transaction = sql_transaction;
                    _bld_DeltaDelParm(delta_table.PrimaryKey, _deltacmd.DeleteCommand);
                }

                _deltacmd.RowUpdating += _RowUpdating;

                _result += _deltacmd.Update(delta_table.Select(null, null, DataViewRowState.Deleted));
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
        /// <param name="connection_string"></param>
        /// <param name="delta_set"></param>
        /// <returns></returns>
        private int UpdateDataSet(string connection_string, DataSet delta_set)
        {
            var _result = 0;

            using (var _sqlconn = new SqlConnection(connection_string))
            {
                _sqlconn.Open();
                var _sqltran = _sqlconn.BeginTransaction();

                try
                {
                    if (delta_set.HasErrors == false && delta_set.HasChanges() == true)
                    {
                        // DataSet 내의 Table간의 Foreign Key 관계로 인하여
                        // 삭제는 detail -> master순서대로, 추가는 master -> detail 관계로 처리되어야 한다.

                        // 삭제처리
                        for (int i = delta_set.Tables.Count - 1; i >= 0; i--)
                            _result += DataTable_Delete(_sqltran, delta_set.Tables[i]);

                        // 추가 및 변경
                        foreach (DataTable _table in delta_set.Tables)
                            _result += DataTable_Update(_sqltran, _table);
                    }

                    if (delta_set.HasErrors == false)
                    {
                        _sqltran.Commit();
                        delta_set.AcceptChanges();
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
        /// <param name="connection_string"></param>
        /// <param name="delta_table"></param>
        /// <returns></returns>
        private int UpdateDataTable(string connection_string, DataTable delta_table)
        {
            var _result = 0;

            using (var _sqlconn = new SqlConnection(connection_string))
            {
                _sqlconn.Open();
                var _sqltran = _sqlconn.BeginTransaction();

                try
                {
                    if (delta_table.HasErrors == false)
                        _result += DataTable_Update(_sqltran, delta_table);

                    if (delta_table.HasErrors == false)
                    {
                        _sqltran.Commit();
                        delta_table.AcceptChanges();
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
        /// <param name="connection_string"></param>
        /// <param name="delta_set"></param>
        /// <returns></returns>
        public int UpdateDeltaSet(string connection_string, DataSet delta_set)
        {
            return UpdateDataSet(connection_string, delta_set);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="delta_set"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public int UpdateDeltaSet(string connection_string, DataSet delta_set, out string message)
        {
            var _result = UpdateDeltaSet(connection_string, delta_set);

            message = "";

            if (_result < 1)
            {
                foreach (DataTable _dt in delta_set.Tables)
                {
                    foreach (DataRow _er in _dt.GetErrors())
                    {
                        message += _er.RowError;
                        break;
                    }
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="delta_set"></param>
        /// <returns></returns>
        public int InsertDeltaSet(string connection_string, DataSet delta_set)
        {
            return UpdateDataSet(connection_string, delta_set);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="delta_table"></param>
        /// <returns></returns>
        public int UpdateDeltaTbl(string connection_string, DataTable delta_table)
        {
            return UpdateDataTable(connection_string, delta_table);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="delta_table"></param>
        /// <returns></returns>
        public int InsertDeltaTbl(string connection_string, DataTable delta_table)
        {
            return UpdateDataTable(connection_string, delta_table);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}