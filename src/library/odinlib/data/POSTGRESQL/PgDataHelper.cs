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
using OdinSdk.OdinLib.Configuration;
using System;
using System.Collections;
using System.Data;
using System.Text;

namespace OdinSdk.OdinLib.Data.POSTGRESQL
{
    /// <summary>
    /// database helper class
    /// </summary>
    public class PgDataHelper
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private readonly Lazy<PgDataHelper> m_lzyHelper = new Lazy<PgDataHelper>(() =>
        {
            return new PgDataHelper();
        });

        /// <summary></summary>
        public PgDataHelper SNG
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
        // Private Functions
        //-----------------------------------------------------------------------------------------------------------------------------

        //private DeltaHelper m_delta_helper = null;
        //private OdinSdk.OdinLib.Data.POSTGRESQL.PgDeltaHelper DeltaHelper
        //{
        //    get
        //    {
        //        if (m_delta_helper == null)
        //            m_delta_helper = new DeltaHelper();
        //        return m_delta_helper;
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------
        ///
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
                        _addval.Append("now(), ");
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
                var _column_name = _keys[_key].ColumnName;
                Object _object = data_rows[_column_name, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("\"{0}\" = {1} AND ", _column_name.ToLower(), _value));
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
                    var _column_name = _col.ColumnName;

                    if (_column_name == "slmd")
                    {
                        _updcol.Append(_column_name + " = now(), ");
                    }
                    else
                    {
                        Object _object = data_rows[_column_name];

                        if (Convert.IsDBNull(_object) == false)
                        {
                            var _value = _getFieldValue(_object);
                            _updcol.Append(String.Format("\"{0}\" = {1}, ", _column_name.ToLower(), _value));
                        }
                        else
                        {
                            _updcol.Append(_column_name + " = null, ");
                        }
                    }
                }
            }

            DataColumn[] _keys = data_rows.Table.PrimaryKey;

            for (int _index = 0; _keys.Length > _index; _index++)
            {
                var _column_name = _keys[_index].ColumnName;
                Object _object = data_rows[_column_name, DataRowVersion.Original];

                var _value = _getFieldValue(_object);
                _where.Append(String.Format("\"{0}\" = {1} AND ", _column_name.ToLower(), _value));
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

        private string _splitTableName(string sql_text)
        {
            var _result = "";

            string[] _words = sql_text.Split(' ', '\t', '\r', '\n');
            for (int i = 0; i < _words.Length; i++)
            {
                if (_words[i].ToLower() == "from")
                {
                    for (int j = i + 1; j < _words.Length; j++)
                    {
                        if (_words[j] != "")
                        {
                            _result = _words[j];
                            break;
                        }
                    }

                    break;
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Build Database Commands
        //-----------------------------------------------------------------------------------------------------------------------------
        //private DatCommands buildCommands(DataSet dataset)
        //{
        //    DatCommands _dbcs = new DatCommands();

        //    foreach (DataTable _table in dataset.Tables)
        //    {
        //        foreach (DataRow _row in _table.Rows)
        //        {
        //            switch (_row.RowState)
        //            {
        //                case DataRowState.Added:
        //                    _dbcs.Add(_bldDataInsSQL(_row), (PgDatParameters)null);
        //                    break;

        //                case DataRowState.Deleted:
        //                    _dbcs.Add(_bldDataDelSQL(_row), (PgDatParameters)null);
        //                    break;

        //                case DataRowState.Modified:
        //                    _dbcs.Add(_bldDataUpdSQL(_row), (PgDatParameters)null);
        //                    break;
        //            }
        //        }
        //    }

        //    return _dbcs;
        //}

        /// <summary>
        /// This method assigns an array of values to an array of NpgsqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of NpgsqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private void _returnParameterValue(NpgsqlParameter[] commandParameters, PgDatParameters parameterValues)
        {
            if (commandParameters != null && parameterValues != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    NpgsqlParameter _cp = commandParameters[i];
                    if (_cp.Direction == ParameterDirection.Input)
                        continue;

                    //Update the Return Value
                    if (_cp.Direction == ParameterDirection.ReturnValue)
                    {
                        foreach (NpgsqlParameter _pv in parameterValues)
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
                        foreach (NpgsqlParameter _pv in parameterValues)
                        {
                            if (_pv.ParameterName.ToLower() == _cp.ParameterName.ToLower())
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

            using (NpgsqlConnection _sqlcon = new NpgsqlConnection(connection_string))
            {
                NpgsqlDataAdapter _adapter = new NpgsqlDataAdapter("SELECT * FROM " + table_name, _sqlcon);
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
        /// <param name="db_param"></param>
        /// <returns></returns>
        public bool IsNullValue(NpgsqlParameter db_param)
        {
            var _result = true;

            if (db_param != null && db_param.Value != null)
            {
                if (String.IsNullOrEmpty(db_param.Value.ToString()) == false)
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
        //public DataSet SelectDataSet(string connection_string, string command_text)
        //{
        //    NpglDatCommand _dbc = new NpglDatCommand(command_text);
        //    return SelectDataSet(connection_string, CommandType.Text, command_text, _dbc.Name);
        //}

        public DataSet SelectDataSet(string connection_string, string command_text, PgDatParameters db_params)
        {
            return SelectDataSet(connection_string, CommandType.Text, command_text, _splitTableName(command_text), db_params);
        }

        //public DataSet SelectDataSet(string connection_string, DatCommands db_commands)
        //{
        //    var _result = new DataSet();

        //    using (NpgsqlConnection _sqlconn = new NpgsqlConnection(connection_string))
        //    {
        //        _sqlconn.Open();

        //        foreach (NpglDatCommand _dbc in db_commands)
        //            _result.Merge(SelectDataSet(_sqlconn, CommandType.Text, _dbc.Text, _dbc.Name));
        //    }

        //    return _result;
        //}

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the database specified in
        /// the connection String.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(string connectionString, CommandType commandType, string commandText, string tabName)
        {
            // Pass through the call providing null for the set of SqlParameters
            return SelectDataSet(connectionString, commandType, commandText, tabName, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the database specified in the connection string
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(connString, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(string connectionString, CommandType commandType, string commandText, string tabName, params NpgsqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");

            // Create & open a NpgsqlConnection, and dispose of it after we are done
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return SelectDataSet(connection, commandType, commandText, tabName, commandParameters);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the provided NpgsqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(NpgsqlConnection connection, CommandType commandType, string commandText, string tabName)
        {
            // Pass through the call providing null for the set of SqlParameters
            return SelectDataSet(connection, commandType, commandText, tabName, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the specified NpgsqlConnection
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(conn, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(NpgsqlConnection connection, CommandType commandType, string commandText, string tabName, params NpgsqlParameter[] commandParameters)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            var mustCloseConnection = false;
            PrepareCommand(cmd, connection, (NpgsqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.FillSchema(ds, SchemaType.Source, tabName);
                da.Fill(ds, tabName);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the provided NpgsqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(NpgsqlTransaction transaction, CommandType commandType, string commandText, string tabName)
        {
            // Pass through the call providing null for the set of SqlParameters
            return SelectDataSet(transaction, commandType, commandText, tabName, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the specified NpgsqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = SelectDataSet(trans, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="tabName"></param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet SelectDataSet(NpgsqlTransaction transaction, CommandType commandType, string commandText, string tabName, params NpgsqlParameter[] commandParameters)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null)
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            var mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.FillSchema(ds, SchemaType.Source, tabName);
                da.Fill(ds, tabName);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                // Return the dataset
                return ds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // SelectScalar
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a 1x1 resultset) against the database specified in the connection string
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            // Create & open a NpgsqlConnection, and dispose of it after we are done
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a 1x1 resultset) against the specified NpgsqlConnection
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(NpgsqlConnection connection, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();

            var mustCloseConnection = false;
            PrepareCommand(cmd, connection, (NpgsqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public object SelectScalar(string connection_string, string command_text, PgDatParameters db_params)
        {
            return ExecuteScalar(connection_string, CommandType.Text, command_text, db_params);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ExecuteReader
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum NpgsqlConnectionOwnership
        {
            /// <summary>Connection is owned and managed by SqlHelper</summary>
            Internal,

            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        /// <summary>
        /// Create and prepare a NpgsqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        ///
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid NpgsqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
        /// <returns>NpgsqlDataReader containing the results of the command</returns>
        private NpgsqlDataReader ExecuteReader(NpgsqlConnection connection, NpgsqlTransaction transaction, CommandType commandType, string commandText, NpgsqlParameter[] commandParameters, NpgsqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            var mustCloseConnection = false;
            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                // Create a reader
                NpgsqlDataReader dataReader;

                // Call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == NpgsqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                // Detach the SqlParameters from the command object, so they can be used again.
                // HACK: There is a problem here, the output parameter values are fletched
                // when the reader is closed, so if the parameters are detached from the command
                // then the SqlReader can't set its values.
                // When this happen, the parameters can't be used again in other command.
                var canClear = true;
                foreach (NpgsqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                        canClear = false;
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                if (mustCloseConnection)
                    connection.Close();

                return dataReader;
            }
            catch (Exception ex)
            {
                throw new Exception("ExecuteReader", ex);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the database specified in the connection string
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  NpgsqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A NpgsqlDataReader containing the resultset generated by the command</returns>
        public NpgsqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            // [주의] 사용한 IDataReader.Close()하지 않으면 Client에서 아래와 같이 정의할 경우 Connection이 계속 오픈되어 있어 sql connection이 Full이 일어남.
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            NpgsqlConnection connection = null;
            try
            {
                connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // Call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(connection, (NpgsqlTransaction)null, commandType, commandText, commandParameters, NpgsqlConnectionOwnership.Internal);
            }
            catch (Exception ex)
            {
                // If we fail to return the SqlDatReader, we need to close the connection ourselves
                if (connection != null)
                    connection.Close();

                throw new Exception("ExecuteReader", ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <returns></returns>
        public NpgsqlDataReader ExecuteReader(string connection_string, string command_text)
        {
            return ExecuteReader(connection_string, CommandType.Text, command_text);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public NpgsqlDataReader ExecuteReader(string connection_string, string command_text, PgDatParameters db_params)
        {
            return ExecuteReader(connection_string, CommandType.Text, command_text, db_params);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ExecuteText
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This method is used to attach array of SqlParameters to a NpgsqlCommand.
        ///
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.
        ///
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        private void AttachParameters(NpgsqlCommand command, NpgsqlParameter[] commandParameters)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (NpgsqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input)
                            if (p.Value == null)
                            {
                                p.Value = DBNull.Value;
                            }

                        command.Parameters.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        private void AssignParameterValues(NpgsqlParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                // Do nothing if we get no data
                return;
            }

            var i = 0;
            // Set the parameters values
            foreach (NpgsqlParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
                    throw new Exception(
                        String.Format(
                            "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                            i, commandParameter.ParameterName));

                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];

                i++;
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private void AssignParameterValues(NpgsqlParameter[] commandParameters, object[] parameterValues)
        {
            AssignParameterValues(commandParameters, parameterValues, false);
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        /// <param name="includeReturnValueParameter"></param>
        private void AssignParameterValues(NpgsqlParameter[] commandParameters, object[] parameterValues, bool includeReturnValueParameter)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            var _parmslen = parameterValues.Length;
            if (includeReturnValueParameter == true)
                _parmslen += 1;

            if (commandParameters.Length > _parmslen)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the SqlParameters, assigning the values from the corresponding position in the
            // value array
            var _parmspos = 0;
            if (includeReturnValueParameter == true)
                _parmspos += 1;

            for (int i = 0, j = parameterValues.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    var paramInstance = (IDbDataParameter)parameterValues[i];

                    if (paramInstance.Direction == ParameterDirection.ReturnValue)
                        continue;

                    var _found = false;
                    for (int k = 0; k < commandParameters.Length; k++)
                    {
                        if (commandParameters[k].ParameterName.ToLower() == paramInstance.ParameterName.ToLower())
                        {
                            if (paramInstance.Value == null)
                            {
                                commandParameters[k].Value = DBNull.Value;
                            }
                            else
                            {
                                commandParameters[k].Value = paramInstance.Value;
                            }

                            _found = true;
                            break;
                        }
                    }

                    if (_found == false)
                        throw new ArgumentException("Parameter name does not match Parameter Value name.");
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i + _parmspos].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i + _parmspos].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters
        /// to the provided command
        /// </summary>
        /// <param name="command">The NpgsqlCommand to be prepared</param>
        /// <param name="connection">A valid NpgsqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid NpgsqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private void PrepareCommand(NpgsqlCommand command, NpgsqlConnection connection, NpgsqlTransaction transaction, CommandType commandType, string commandText, NpgsqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0)
                throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null)
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;
            command.CommandTimeout = 600;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            return;
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private void UpdateParameterValues(NpgsqlParameter[] commandParameters, NpgsqlParameter[] parameterValues)
        {
            if (commandParameters != null && parameterValues != null)
            {
                // Iterate through the SqlParameters, assigning the values from the corresponding position in the
                // value array
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    NpgsqlParameter _cp = commandParameters[i];
                    if (_cp.Direction == ParameterDirection.Input)
                        continue;

                    //Update the Return Value
                    if (_cp.Direction == ParameterDirection.ReturnValue)
                    {
                        foreach (NpgsqlParameter _pv in parameterValues)
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
                        foreach (NpgsqlParameter _pv in parameterValues)
                        {
                            if (_pv.ParameterName.ToLower() == _cp.ParameterName.ToLower())
                            {
                                _pv.Value = _cp.Value;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns no resultset) against the database specified in
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns no resultset) against the specified NpgsqlConnection
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(NpgsqlConnection connection, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            var mustCloseConnection = false;
            PrepareCommand(cmd, connection, (NpgsqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            var retval = cmd.ExecuteNonQuery();

            // return value
            if (cmd.Parameters.Count > 0 && cmd.Parameters[0].Direction == ParameterDirection.ReturnValue)
                retval = (int)cmd.Parameters[0].Value;

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns no resultset) against the specified NpgsqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(NpgsqlTransaction transaction, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            if (transaction != null && transaction.Connection == null)
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();

            var mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            var retval = cmd.ExecuteNonQuery();

            // return value
            if (cmd.Parameters.Count > 0 && cmd.Parameters[0].Direction == ParameterDirection.ReturnValue)
                retval = (int)cmd.Parameters[0].Value;

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            return retval;
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns no resultset) against the database specified in the connection string
        /// using the provided parameters
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");

            // Create & open a NpgsqlConnection, and dispose of it after we are done
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <returns></returns>
        public int ExecuteText(string connection_string, string command_text)
        {
            return ExecuteNonQuery(connection_string, CommandType.Text, command_text);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public int ExecuteText(string connection_string, string command_text, PgDatParameters db_params)
        {
            return ExecuteNonQuery(connection_string, CommandType.Text, command_text, db_params);
        }

        //public int ExecuteText(string connection_string, DatCommands db_commands)
        //{
        //    var _result = 0;

        //    foreach (NpglDatCommand _dbc in db_commands)
        //    {
        //        ExecuteNonQuery(connection_string, CommandType.Text, _dbc.Text, _dbc.Value.ToArray());

        //        _result++;
        //    }

        //    return _result;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_texts"></param>
        /// <returns></returns>
        public int ExecuteText(string connection_string, params string[] command_texts)
        {
            var _result = 0;

            using (NpgsqlConnection _sqlconn = new NpgsqlConnection(connection_string))
            {
                _sqlconn.Open();
                NpgsqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < command_texts.Length; i++)
                    {
                        if (command_texts[i] != null && command_texts[i] != "")
                        {
                            ExecuteNonQuery(transaction, CommandType.Text, command_texts[i]);

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

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="db_params"></param>
        /// <param name="command_texts"></param>
        /// <returns></returns>
        public int ExecuteText(string connection_string, PgDatParameters db_params, params string[] command_texts)
        {
            var _result = 0;

            using (NpgsqlConnection _sqlconn = new NpgsqlConnection(connection_string))
            {
                _sqlconn.Open();
                NpgsqlTransaction transaction = _sqlconn.BeginTransaction();

                try
                {
                    for (int i = 0; i < command_texts.Length; i++)
                    {
                        if (command_texts[i] != null && command_texts[i] != "")
                        {
                            ExecuteNonQuery(transaction, CommandType.Text, command_texts[i], db_params);
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

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="connection_string"></param>
        ///// <param name="stored_procedure_name"></param>
        ///// <returns></returns>
        //public PgDatParameters GetNpgsqlParameters(string connection_string, string stored_procedure_name)
        //{
        //    var _dbps = new PgDatParameters();

        //    NpgsqlParameter[] _sbps = NpgsqlHelperParameterCache.GetSpParameterSet(connection_string, stored_procedure_name);
        //    foreach (NpgsqlParameter _s in _sbps)
        //    {
        //        _dbps.Add(_s.ParameterName, _s.SqlDbType, _s.Direction, DBNull.Value);
        //    }

        //    return _dbps;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ExecuteProc(string connection_string, string stored_procedure_name, params object[] args)
        {
            return ExecuteNonQuery(connection_string, stored_procedure_name, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="stored_procedure_name"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public int ExecuteProc(string connection_string, string stored_procedure_name, PgDatParameters db_params)
        {
            var _result = -1;

            NpgsqlParameter[] _params = db_params;
            _result = ExecuteProcQuery(connection_string, stored_procedure_name, _params);
            {
                _returnParameterValue(_params, db_params);
            }

            return _result;
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns no resultset) against the database specified in
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  int result = ExecuteProcQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteProcQuery(string connectionString, string spName, params NpgsqlParameter[] parameterValues)
        {
            int _result;

            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(connectionString, spName, true);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues, true);

                // Call the overload that takes an array of SqlParameters
                _result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                {
                    UpdateParameterValues(commandParameters, parameterValues);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                _result = ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
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
        public DataSet ExecuteProcSet(string connection_string, string stored_procedure_name, PgDatParameters db_params)
        {
            var _result = new DataSet();

            NpgsqlParameter[] _params = db_params;
            _result = ExecuteProcSet(connection_string, stored_procedure_name, _params);
            {
                _returnParameterValue(_params, db_params);
            }

            return _result;
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns a resultset) against the database specified in
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  DataSet ds = ExecuteProcSet(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteProcSet(string connectionString, string spName, params NpgsqlParameter[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(connectionString, spName, true);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues, true);

                // Call the overload that takes an array of SqlParameters
                var _result = ExecuteDataSet(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                {
                    for (int i = 0; i < commandParameters.Length; i++)
                    {
                        NpgsqlParameter _cp = commandParameters[i];
                        if (_cp.Direction == ParameterDirection.ReturnValue)
                        {
                            if (Convert.ToInt32(_cp.Value) != 0)
                                _result.Clear();
                        }
                    }

                    UpdateParameterValues(commandParameters, parameterValues);
                }

                return _result;
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataSet(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // ExecuteDataSet
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string connection_string, string command_text)
        {
            return ExecuteDataSet(connection_string, CommandType.Text, command_text);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="command_text"></param>
        /// <param name="db_params"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string connection_string, string command_text, PgDatParameters db_params)
        {
            return ExecuteDataSet(connection_string, CommandType.Text, command_text, db_params);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the database specified in
        /// the connection String.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataSet(connectionString, commandType, commandText, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the database specified in the connection string
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(connString, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(string connectionString, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");

            connectionString = connectionString + "; Connect Timeout=3600";

            // Create & open a NpgsqlConnection, and dispose of it after we are done
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                NpgsqlCommand com0 = new NpgsqlCommand("SET ARITHABORT ON", connection);
                com0.ExecuteNonQuery();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataSet(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns a resultset) against the database specified in
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataSet(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataSet(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the provided NpgsqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataSet(connection, commandType, commandText, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the specified NpgsqlConnection
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(conn, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlConnection connection, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            var mustCloseConnection = false;
            cmd.CommandTimeout = 3600;
            PrepareCommand(cmd, connection, (NpgsqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns a resultset) against the specified NpgsqlConnection
        /// using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataSet(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataSet(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset and takes no parameters) against the provided NpgsqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataSet(transaction, commandType, commandText, (NpgsqlParameter[])null);
        }

        /// <summary>
        /// Execute a NpgsqlCommand (that returns a resultset) against the specified NpgsqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(trans, CommandType.StoredProcedure, "GetOrders", new NpgsqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlTransaction transaction, CommandType commandType, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null)
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.CommandTimeout = 2000;
            var mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a NpgsqlCommand (that returns a resultset) against the specified
        /// NpgsqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// e.g.:
        ///  DataSet ds = ExecuteDataSet(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid NpgsqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataSet(NpgsqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null)
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                NpgsqlParameter[] commandParameters = NpgsqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataSet(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataSet(transaction, CommandType.StoredProcedure, spName);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        // Update DataSet
        //-----------------------------------------------------------------------------------------------------------------------------
        //public int UpdateDataSet(string connection_string, DataSet dataset)
        //{
        //    return UpdateDeltaSet(connection_string, dataset);
        //}

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

    /// <summary>
    /// NpgsqlHelperParameterCache provides functions to leverage a cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class NpgsqlHelperParameterCache
    {
        #region private static methods, variables, and constructors

        //Since this class provides only methods, make the default constructor private static to prevent
        //instances from being created with "new NpgsqlHelperParameterCache()"
        private NpgsqlHelperParameterCache()
        {
        }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid NpgsqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static NpgsqlParameter[] DiscoverSpParameterSet(NpgsqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            NpgsqlCommand cmd = new NpgsqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();

            NpgsqlCommand com0 = new NpgsqlCommand("SET ARITHABORT ON", connection);
            com0.ExecuteNonQuery();

            NpgsqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();

            if (includeReturnValueParameter == false)
                cmd.Parameters.RemoveAt(0);

            NpgsqlParameter[] discoveredParameters = new NpgsqlParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (NpgsqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }

            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached NpgsqlParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static NpgsqlParameter[] CloneParameters(NpgsqlParameter[] originalParameters)
        {
            NpgsqlParameter[] clonedParameters = new NpgsqlParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (NpgsqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private static methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params NpgsqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0)
                throw new ArgumentNullException("commandText");

            var hashKey = String.Format("{0}:{1}", connectionString, commandText);

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static NpgsqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0)
                throw new ArgumentNullException("commandText");

            var hashKey = String.Format("{0}:{1}", connectionString, commandText);

            NpgsqlParameter[] cachedParameters = paramCache[hashKey] as NpgsqlParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static NpgsqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static NpgsqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            return GetSpParameterSet(connectionString, spName, includeReturnValueParameter, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a NpgsqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <param name="isCacheFlag"></param>
        /// <returns>An array of SqlParameters</returns>
        public static NpgsqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter, bool isCacheFlag)
        {
            if (connectionString == null || connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter, isCacheFlag);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        internal static NpgsqlParameter[] GetSpParameterSet(NpgsqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid NpgsqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        internal static NpgsqlParameter[] GetSpParameterSet(NpgsqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            using (NpgsqlConnection clonedConnection = (NpgsqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid NpgsqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        private static NpgsqlParameter[] GetSpParameterSetInternal(NpgsqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid NpgsqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <param name="isCacheFlag"></param>
        /// <returns>An array of SqlParameters</returns>
        private static NpgsqlParameter[] GetSpParameterSetInternal(NpgsqlConnection connection, string spName, bool includeReturnValueParameter, bool isCacheFlag)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0)
                throw new ArgumentNullException("spName");

            if (isCacheFlag == true)
            {
                var hashKey = String.Format("{0}:{1}{2}", connection.ConnectionString, spName, (includeReturnValueParameter ? ":include ReturnValue Parameter" : ""));

                NpgsqlParameter[] cachedParameters = paramCache[hashKey] as NpgsqlParameter[];
                if (cachedParameters == null)
                {
                    NpgsqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                    paramCache[hashKey] = spParameters;
                    cachedParameters = spParameters;
                }

                return CloneParameters(cachedParameters);
            }
            else
            {
                return CloneParameters(DiscoverSpParameterSet(connection, spName, includeReturnValueParameter));
            }
        }

        #endregion Parameter Discovery Functions
    }
}