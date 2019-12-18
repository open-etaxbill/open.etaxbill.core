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
using System.Text;

//#pragma warning disable 1589, 1591

namespace OpenTax.Engine.Library.Data.POSTGRESQL
{
    /// <summary>
    ///
    /// </summary>
    public class PgDeltaHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<PgDeltaHelper> m_lzyHelper = new Lazy<PgDeltaHelper>(() =>
        {
            return new PgDeltaHelper();
        });

        /// <summary>
        ///
        /// </summary>
        public static PgDeltaHelper SNG
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

        /*
        Postgresql  NpgsqlDbType System.DbType Enum .Net System Type
        ----------  ------------ ------------------ ----------------
        int8        Bigint       Int64              Int64
        bool        Boolean      Boolean            Boolean
        bytea       Bytea        Binary             Byte[]
        date        Date         Date               DateTime
        float8      Double       Double             Double
        int4        Integer      Int32              Int32
        money       Money        Decimal            Decimal
        numeric     Numeric      Decimal            Decimal
        float4      Real         Single             Single
        int2        Smallint     Int16              Int16
        text        Text         String             String
        time        Time         Time               DateTime
        timetz      Time         Time               DateTime
        timestamp   Timestamp    DateTime           DateTime
        timestamptz TimestampTZ  DateTime           DateTime
        interval    Interval     Object             TimeSpan
        varchar     Varchar      String             String
        inet        Inet         Object             IPAddress
        bit         Bit          Boolean            Boolean
        uuid        Uuid         Guid               Guid
        array       Array        Object             Array
        */

        private NpgsqlDbType _getNpgsqlDbType(Type p_type)
        {
            NpgsqlDbType _result = NpgsqlDbType.Varchar;

            switch (p_type.FullName)
            {
                case "System.String":
                    _result = NpgsqlDbType.Varchar;
                    break;

                //  case "System.String":
                //      _result = NpgsqlDbType.Char;
                //      break;
                //  case "System.String":
                //      _result = NpgsqlDbType.NChar;
                //      break;
                //  case "System.String":
                //      _result = NpgsqlDbType.NText;
                //      break;

                case "System.Int64":
                    _result = NpgsqlDbType.Bigint;
                    break;

                case "System.Array":
                    _result = NpgsqlDbType.Bytea;
                    break;

                case "System.Boolean":
                    _result = NpgsqlDbType.Boolean;
                    break;

                case "System.DateTime":
                    _result = NpgsqlDbType.TimestampTz;
                    break;

                case "System.Decimal":
                    _result = NpgsqlDbType.Numeric;
                    break;

                case "System.Double":
                    _result = NpgsqlDbType.Double;
                    break;

                case "System.Byte[]":
                    _result = NpgsqlDbType.Bytea;
                    break;

                case "System.Int32":
                    _result = NpgsqlDbType.Integer;
                    break;

                //                case "System.Decimal":
                //                    _result = NpgsqlDbType.Money;
                //                    break;
                //

                case "System.Single":
                    _result = NpgsqlDbType.Real;
                    break;

                //                case "System.DateTime":
                //                    _result = NpgsqlDbType.SmallDateTime;
                //                    break;

                case "System.Int16":
                    _result = NpgsqlDbType.Smallint;
                    break;

                //                case "System.Decimal":
                //                    _result = NpgsqlDbType.SmallMoney;
                //                    break;
                //
                //                case "System.String":
                //                    _result = NpgsqlDbType.Text;
                //                    break;
                //
                //                case "System.Array":
                //                    _result = NpgsqlDbType.Timestamp;
                //                    break;

                case "System.Byte":
                    _result = NpgsqlDbType.Char;
                    break;

                case "System.Guid":
                    _result = NpgsqlDbType.Interval;
                    break;

                //                case "System.Array":
                //                    _result = NpgsqlDbType.VarBinary;
                //                    break;
                //
                //                case "System.String":
                //                    _result = NpgsqlDbType.Varchar;
                //                    break;

                case "Object":
                    _result = NpgsqlDbType.Interval;
                    break;

                default:
                    _result = NpgsqlDbType.Interval;
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
                var _column_name = _keys[_index].ColumnName;
                _wheres.Append(String.Format("\"{0}\" = @org_{1} AND ", _column_name.ToLower(), _column_name));
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

                var _column_name = _dc.ColumnName;
                if (_column_name.ToLower() == "sfid" || _column_name.ToLower() == "slmd")
                {
                    _addcol.Append(String.Format("\"{0}\", ", _column_name.ToLower()));

                    if (p_adddrow[_column_name] == DBNull.Value)
                    {
                        _addval.Append("now(), ");
                    }
                    else
                    {
                        _addval.Append(String.Format("@{0}, ", _column_name));
                    }
                }
                else
                {
                    if (p_adddrow[_column_name] == DBNull.Value)
                        continue;
                    else
                    {
                        _addcol.Append(String.Format("\"{0}\", ", _column_name.ToLower()));
                        _addval.Append(String.Format("@{0}, ", _column_name));
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

                var _column_name = _dc.ColumnName;
                if (_column_name.ToLower() == "slmd")
                {
                    _updcol.Append(String.Format("\"{0}\" = now(), ", _column_name.ToLower()));

                    // 예전에는 업데이트 되는 필드(Set으로 정의)가 해당 테이블의 모든 필드에 적용이 되어
                    // 하나의 DataRow를 서로 다른 사람이 바꾸게 되면 마지막 사람의 DataRow가 적용되는 문제가 있어
                    // 해당 DataRow의 Server 날짜와 Clien t날짜를 비교하는 문장을 넣어 Server와 Client를 데이터가 서로 다른지를 확인했는데,
                    // _bldDeltaUpdSQL함수를 수정하여 변경된 Column만 Set으로 처리되므로 필요가 없게 되었음.
                    //_wheres.Append(String.Format("\"{0}\" = @org_{0} AND ", _column_name.ToLower(), _column_name));
                }
                else
                {
                    object _original = p_updrow[_column_name, DataRowVersion.Original];
                    object _current = p_updrow[_column_name, DataRowVersion.Current];

                    if (_original.Equals(_current) == false)
                        _updcol.Append(String.Format("\"{0}\" = @{1}, ", _column_name.ToLower(), _column_name));
                }
            }

            DataColumn[] _keys = _dt.PrimaryKey;
            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _column_name = _keys[_index].ColumnName;
                _wheres.Append(String.Format("\"{0}\" = @org_{1} AND ", _column_name.ToLower(), _column_name));
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
        private void _bld_DeltaInsParm(DataColumnCollection p_columns, NpgsqlCommand p_sqlcmd)
        {
            foreach (DataColumn _dc in p_columns)
            {
                if (_dc.AutoIncrement == true)
                    continue;

                var _columnName = _dc.ColumnName;

                var _args = p_sqlcmd.Parameters.Add(new NpgsqlParameter("@" + _columnName, _getNpgsqlDbType(_dc.DataType)));
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
        private void _bld_DeltaUpdParm(DataColumnCollection p_columns, NpgsqlCommand p_sqlcmd)
        {
            foreach (DataColumn _dc in p_columns)
            {
                var _columnName = _dc.ColumnName;
                if (_columnName.ToLower() != "slmd")
                {
                    var _args = p_sqlcmd.Parameters.Add(new NpgsqlParameter("@" + _columnName, _getNpgsqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Current;
                    }
                }
                else
                {
                    var _args = p_sqlcmd.Parameters.Add(new NpgsqlParameter("@org_" + _columnName, _getNpgsqlDbType(_dc.DataType)));
                    {
                        _args.Direction = ParameterDirection.Input;
                        _args.SourceColumn = _columnName;
                        _args.SourceVersion = DataRowVersion.Original;
                    }
                }
            }

            foreach (DataColumn _kc in p_columns[0].Table.PrimaryKey)
            {
                var _args = p_sqlcmd.Parameters.Add(new NpgsqlParameter("@org_" + _kc.ColumnName, _getNpgsqlDbType(_kc.DataType)));
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
        private void _bld_DeltaDelParm(DataColumn[] p_primaryKeys, NpgsqlCommand p_sqlcmd)
        {
            foreach (DataColumn _kc in p_primaryKeys)
            {
                var _args = p_sqlcmd.Parameters.Add(new NpgsqlParameter("@org_" + _kc.ColumnName, _getNpgsqlDbType(_kc.DataType)));
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

            using (var _sqlcon = new NpgsqlConnection(p_connection_string))
            {
                var _adapter = new NpgsqlDataAdapter("SELECT * FROM " + p_tableName, _sqlcon);
                _adapter.FillSchema(_result, SchemaType.Source, p_tableName);
            }

            return _result;
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="p_connection_string"></param>
        ///// <param name="p_cmdtxt"></param>
        ///// <returns></returns>
        //public DataSet SelectDeltaSet(string p_connection_string, string p_cmdtxt)
        //{
        //    var _result = new DataSet();

        //    var _dbc = new NpglDatCommand(p_cmdtxt);
        //    _result = SelectDataSet(p_connection_string, CommandType.Text, p_cmdtxt, _dbc.Name);

        //    return _result;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------
        // Update or Insert Delta DataSet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="p_npgSqlTransaction"></param>
        /// <param name="p_deltaTbl"></param>
        /// <remarks>
        /// DataSet 내의 Table간의 Foreign Key 관계로 인하여
        /// 삭제는 detail -> master순서대로, 추가는 master -> detail 관계로 처리되어야 한다.
        /// 따라서 삭제부분은 DataTable_Delete()으로 따로 분리한다.
        /// </remarks>
        /// <returns></returns>
        private int DataTable_Update(NpgsqlTransaction p_npgSqlTransaction, DataTable p_deltaTbl)
        {
            var _result = 0;

            using (var _deltacmd = new NpgsqlDataAdapter(_bldDeltaSelSQL(p_deltaTbl), p_npgSqlTransaction.Connection))
            {
                _deltacmd.InsertCommand = new NpgsqlCommand("", p_npgSqlTransaction.Connection);
                {
                    _deltacmd.InsertCommand.Transaction = p_npgSqlTransaction;
                    _bld_DeltaInsParm(p_deltaTbl.Columns, _deltacmd.InsertCommand);
                }

                _deltacmd.UpdateCommand = new NpgsqlCommand("", p_npgSqlTransaction.Connection);
                {
                    _deltacmd.UpdateCommand.Transaction = p_npgSqlTransaction;
                    _bld_DeltaUpdParm(p_deltaTbl.Columns, _deltacmd.UpdateCommand);
                }

                _deltacmd.RowUpdating += _RowUpdating;

                _result += _deltacmd.Update(p_deltaTbl.Select(null, null, DataViewRowState.ModifiedCurrent));
                _result += _deltacmd.Update(p_deltaTbl.Select(null, null, DataViewRowState.Added));
            }

            return _result;
        }

        private int DataTable_Delete(NpgsqlTransaction p_npgSqlTransaction, DataTable p_deltaTbl)
        {
            var _result = 0;

            using (var _deltacmd = new NpgsqlDataAdapter(_bldDeltaSelSQL(p_deltaTbl), p_npgSqlTransaction.Connection))
            {
                _deltacmd.DeleteCommand = new NpgsqlCommand(_bldDeltaDelSQL(p_deltaTbl), p_npgSqlTransaction.Connection);
                {
                    _deltacmd.DeleteCommand.Transaction = p_npgSqlTransaction;
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
        private void _RowUpdating(object sender, NpgsqlRowUpdatingEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Added)
            {
                NpgsqlDataAdapter _adpater = (NpgsqlDataAdapter)sender;

                _adpater.InsertCommand.CommandText = _bldDeltaAddSQL(e.Row);
            }
            else if (e.Row.RowState == DataRowState.Modified)
            {
                NpgsqlDataAdapter _adpater = (NpgsqlDataAdapter)sender;

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

            using (var _sqlconn = new NpgsqlConnection(p_connection_string))
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

            using (var _sqlconn = new NpgsqlConnection(p_connection_string))
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