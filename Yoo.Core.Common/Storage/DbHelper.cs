using MongoDB.Driver.Core.Configuration;
using MySql.Data.MySqlClient;
using Yoo.Core.Common.Helper;
using Yoo.Core.Model.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Storage
{
    public class DbHelper
    {
        private ConcurrentDictionary<string, string> _connections;
        private string _maindbConnectionString;
        private ConcurrentDictionary<string, MySqlConnection> _connectedConnections;

        private readonly List<string> _dbType = new List<string>
        {
            ConstantModels.REPORTINGDB,
            ConstantModels.PLAYERDB
        };

        public DbHelper(string connectionString, string assetdbConnectionString = null)
        {
            _maindbConnectionString = connectionString;
            _connections = new ConcurrentDictionary<string, string>();
            _connectedConnections = new ConcurrentDictionary<string, MySqlConnection>();
		}

        private string handleDateTimeExceptionConnection(string currentConnectionString)
        {
            var theConfig = "convert zero datetime=True;";
            if (currentConnectionString.Contains(theConfig))
            {
                return currentConnectionString;
            }

            currentConnectionString =  currentConnectionString + ";" + theConfig;

            theConfig = "Connection Timeout=60;";
            if (currentConnectionString.Contains("Connection Timeout"))
            {
                return currentConnectionString; 
            }

            return currentConnectionString + ";" + theConfig; 
        }

        //这种做法是简单的。。。
        public MySqlConnection GetMainConnection()
        {
            return new MySqlConnection(handleDateTimeExceptionConnection(_maindbConnectionString));
        }

        public MySqlConnection GetAssetDbConnection()
        {
            string conn_tmpl = Appsettings.app(new string[] { "MySql", "AssetDbConnectionString" });
            return new MySqlConnection(handleDateTimeExceptionConnection(conn_tmpl));
        }

        public MySqlConnection GetLogConnection()
        {
            string conn_tmpl = Appsettings.app(new string[] { "MySql", "LogConnectionString" });
            //if (!_connections.ContainsKey("log"))
            //{
            //    _connections.TryAdd("log", conn_tmpl);
            //}
            return new MySqlConnection(handleDateTimeExceptionConnection(conn_tmpl));
        }

        public MySqlConnection GetReportConnection()
        {
            string conn_tmpl = Appsettings.app(new string[] { "MySql", "ConnectionReport" });
            //if (!_connections.ContainsKey("log"))
            //{
            //    _connections.TryAdd("log", conn_tmpl);
            //}
            return new MySqlConnection(handleDateTimeExceptionConnection(conn_tmpl));
        }

        //这种做法是牛逼的
        public async Task<T> Execute_Main<T>(Func<MySqlConnection, object[], Task<T>> working_func, params object[] args)
        {
            using  (var conn = new MySqlConnection(_maindbConnectionString))
            {
                await conn.OpenAsync();
                var result = await working_func(conn, args);
                return result; 
            }
        }

        public async Task<T> Execute_Agent<T>(string agentCode, string type, Func<MySqlConnection,object[], Task<T>> working_func, params object[] args)
        {
            var connStr = GetConnect(agentCode, type);
            using (var conn = new MySqlConnection(connStr))
            {
                await conn.OpenAsync();
                var result = await working_func(conn, args);
                return result;
            }
        }
        private string GetConnect(string agentCode, string type )
        {
            if (!_dbType.Contains(type.ToLower()))
            {
                throw new FormatException($"{type} is not suported! Please pass in the correct value!");
            }

            var name = _get_key_name(agentCode, type);
            string agentConnection = null;
            var is_get = _connections.TryGetValue(name,out agentConnection);
            if (is_get)
                return agentConnection;

            ArgumentNullException argumentNullException = new ArgumentNullException("Agent DB haven't init yet. Please call Set Connection first before call it ");
            throw argumentNullException;

        }

        private string _get_key_name(string agent_code, string type)
        {
            if (type == "reportdb")
            {
                return $"reporting_{agent_code}";
            }

            return $"player_{agent_code}";
        }

        /// <summary>
        /// 建议用这个来添加代理的connection，容易保持一致
        /// </summary>
        /// <param name="agentCode">代理商编号</param>
        /// <param name="type">reportdb or playerdb</param>
        /// <param name="url">connection string</param>
        public void SetAgentConnection(string agentCode, string type, string url)
        {
            if (!_dbType.Contains(type.ToLower()))
            {
                throw new FormatException($"{type} is not suported! Please pass in the correct value!"); 
            }

           // var key_name = string.Format("{0}_{1}", agentCode, type);
            string conn_tmpl = Appsettings.app(new string[] { "MySql", "ConnectionTmpl" });
            string real_tmpl = string.Format(conn_tmpl, url, type);

            _setConnection(_get_key_name(agentCode, type), real_tmpl);
        }

        public bool CheckAgentConnection(string agentCode, string type)
        {
            var key_name = _get_key_name(agentCode, type);
            return _connections.ContainsKey(key_name); 
        }

        private void _setConnection(string name, string connectionString)
        {
            //if (_connections.ContainsKey(name))
            //{
            //    //没必要重复添加
            //    return; 
            //}
            //_connections.TryAdd(name, connectionString);
            _connections.AddOrUpdate(name, connectionString, (oldkey,oldvalue) => connectionString);
        }

        public void RemoveAgentConnection(string agentCode, string type)
        {
            var key_name = _get_key_name(agentCode, type);
            var removeResult = _connections.TryRemove(key_name, out string removedValue);
        }

        // get special player tag log
        public MySqlConnection GetSpecialTagLogConnection()
        {
            string conn_tmpl = Appsettings.app(new string[] { "MySql", "LogConnection" });
            //if (!_connections.ContainsKey("log"))
            //{
            //    _connections.TryAdd("log", conn_tmpl);
            //}
            return new MySqlConnection(handleDateTimeExceptionConnection(conn_tmpl));
        }
    }

    public static class DBContainer
    {
        private static DbHelper _currentDb = null;
        public static DbHelper DB
        {
            get
            {
                return _currentDb; 
            }
            set
            {
                if(_currentDb == null)
                {
                    _currentDb = value; 
                }
            }
        }
         
    }
}
