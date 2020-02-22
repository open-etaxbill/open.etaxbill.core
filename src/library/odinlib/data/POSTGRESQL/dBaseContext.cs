using Npgsql;
using OdinSdk.OdinLib.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace OdinSdk.OdinLib.Data.POSTGRESQL
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
        /// <param name="command_timeout"></param>
        public dBaseContext(string nameOrConnectionString, IDatabaseInitializer<DbContext> strategy = null, int command_timeout = 60)
            : base(nameOrConnectionString)
        {
            base.Database.Initialize(false);
            //base.Database.SetInitializer<DbContext>(strategy);

            if (CfgHelper.SNG.IsUnix == true)
                OContext.CommandTimeout = command_timeout * 1000;
            else
                Database.CommandTimeout = command_timeout;
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
        /// <param name="function_name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteFunction(string function_name, params ObjectParameter[] parameters)
        {
            return this.OContext.ExecuteFunction(function_name, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function_name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ObjectResult<T> ExecuteFunction<T>(string function_name, params ObjectParameter[] parameters)
        {
            return this.OContext.ExecuteFunction<T>(function_name, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteStoreCommand(string command_text, params object[] parameters)
        {
            return this.OContext.ExecuteStoreCommand(command_text, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command_text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<int> ExecuteStoreCommandAsync(string command_text, params object[] parameters)
        {
            return this.OContext.ExecuteStoreCommandAsync(command_text, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command_text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ObjectResult<T> ExecuteStoreQuery<T>(string command_text, params object[] parameters)
        {
            return this.OContext.ExecuteStoreQuery<T>(command_text, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command_text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<ObjectResult<T>> ExecuteStoreQueryAsync<T>(string command_text, params object[] parameters)
        {
            return this.OContext.ExecuteStoreQueryAsync<T>(command_text, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="sql_string"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<TElement> SelectFunction<TElement>(string sql_string, params NpgsqlParameter[] args)
        {
            return this.Database
                    .SqlQuery<TElement>(sql_string, args)
                    .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp_name"></param>
        /// <param name="args"></param>
        public void ExecuteProcedure(string sp_name, params NpgsqlParameter[] args)
        {
            this.Database.SqlQuery(typeof(object), sp_name, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql_command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ExecuteText(string sql_command, params NpgsqlParameter[] args)
        {
            return this.Database.ExecuteSqlCommand(sql_command, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql_string"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DataSet SelectDataSet(string sql_string, params NpgsqlParameter[] args)
        {
            var _result = new DataSet();

            using (var _connection = (NpgsqlConnection)this.Database.Connection)
            {
                var _adapter = new NpgsqlDataAdapter();

                var _command = new NpgsqlCommand(sql_string, _connection);
                _command.Parameters.AddRange(args);
                _command.CommandType = CommandType.Text;

                _adapter.SelectCommand = _command;
                _adapter.Fill(_result);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp_name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string sp_name, params NpgsqlParameter[] args)
        {
            var _result = new DataSet();

            using (var _connection = (NpgsqlConnection)this.Database.Connection)
            {
                var _adapter = new NpgsqlDataAdapter();

                var _command = new NpgsqlCommand(sp_name, _connection);
                _command.Parameters.AddRange(args);
                _command.CommandText = sp_name;
                _command.CommandType = CommandType.StoredProcedure;

                _adapter.Fill(_result);
            }

            return _result;
        }
    }
}
