using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTax.Engine.Library.Data.POSTGRESQL
{
    class NpgsqlConfiguration : System.Data.Entity.DbConfiguration
    {
        public NpgsqlConfiguration()
        {
            SetProviderServices("Npgsql", Npgsql.NpgsqlServices.Instance);
            SetProviderFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            SetDefaultConnectionFactory(new Npgsql.NpgsqlConnectionFactory());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DbConfigurationType(typeof(NpgsqlConfiguration))]
    public partial class dBaseContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        /// <param name="strategy"></param>
        /// <param name="p_commandTimeout"></param>
        public dBaseContext(string nameOrConnectionString, IDatabaseInitializer<DbContext> strategy = null, int p_commandTimeout = 60)
            : base(nameOrConnectionString)
        {
            base.Database.Initialize(false);
            //base.Database.SetInitializer<DbContext>(strategy);

            if (CfgHelper.SNG.IsUnix == true)
                OContext.CommandTimeout = p_commandTimeout * 1000;
            else
                Database.CommandTimeout = p_commandTimeout;
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectContext OContext
        {
            get
            {
                return ((IObjectContextAdapter)this).ObjectContext;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_functionName"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public int ExecuteFunction(string p_functionName, params ObjectParameter[] p_parameters)
        {
            return this.OContext.ExecuteFunction(p_functionName, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_functionName"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public ObjectResult<T> ExecuteFunction<T>(string p_functionName, params ObjectParameter[] p_parameters)
        {
            return this.OContext.ExecuteFunction<T>(p_functionName, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_commandText"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public int ExecuteStoreCommand(string p_commandText, params object[] p_parameters)
        {
            return this.OContext.ExecuteStoreCommand(p_commandText, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_commandText"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public Task<int> ExecuteStoreCommandAsync(string p_commandText, params object[] p_parameters)
        {
            return this.OContext.ExecuteStoreCommandAsync(p_commandText, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_commandText"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public ObjectResult<T> ExecuteStoreQuery<T>(string p_commandText, params object[] p_parameters)
        {
            return this.OContext.ExecuteStoreQuery<T>(p_commandText, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_commandText"></param>
        /// <param name="p_parameters"></param>
        /// <returns></returns>
        public Task<ObjectResult<T>> ExecuteStoreQueryAsync<T>(string p_commandText, params object[] p_parameters)
        {
            return this.OContext.ExecuteStoreQueryAsync<T>(p_commandText, p_parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="p_sqlstr"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        public List<TElement> SelectFunction<TElement>(string p_sqlstr, params NpgsqlParameter[] p_args)
        {
            return this.Database
                    .SqlQuery<TElement>(p_sqlstr, p_args)
                    .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_sp_name"></param>
        /// <param name="p_args"></param>
        public void ExecuteProcedure(string p_sp_name, params NpgsqlParameter[] p_args)
        {
            this.Database.SqlQuery(typeof(object), p_sp_name, p_args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_sqlcmd"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        public int ExecuteText(string p_sqlcmd, params NpgsqlParameter[] p_args)
        {
            return this.Database.ExecuteSqlCommand(p_sqlcmd, p_args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_sqlstr"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        public DataSet SelectDataSet(string p_sqlstr, params NpgsqlParameter[] p_args)
        {
            var _result = new DataSet();

            using (var _connection = (NpgsqlConnection)this.Database.Connection)
            {
                var _adapter = new NpgsqlDataAdapter();

                var _command = new NpgsqlCommand(p_sqlstr, _connection);
                _command.Parameters.AddRange(p_args);
                _command.CommandType = CommandType.Text;

                _adapter.SelectCommand = _command;
                _adapter.Fill(_result);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_sp_name"></param>
        /// <param name="p_args"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string p_sp_name, params NpgsqlParameter[] p_args)
        {
            var _result = new DataSet();

            using (var _connection = (NpgsqlConnection)this.Database.Connection)
            {
                var _adapter = new NpgsqlDataAdapter();

                var _command = new NpgsqlCommand(p_sp_name, _connection);
                _command.Parameters.AddRange(p_args);
                _command.CommandText = p_sp_name;
                _command.CommandType = CommandType.StoredProcedure;

                _adapter.Fill(_result);
            }

            return _result;
        }
    }
}
