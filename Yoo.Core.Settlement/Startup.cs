using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.Settlement.Workers;

namespace Yoo.Core.Settlement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<SettlementWorker>();

            services.AddSingleton(new Appsettings(Configuration));

            //Redis config
            var section = Configuration.GetSection("Redis:Default");
            var _connectionString = section.GetSection("Connection").Value;
            string _instanceName = section.GetSection("InstanceName").Value;
            string _password = section.GetSection("Password").Value;
            int _defaultDB = int.Parse(section.GetSection("DefaultDB").Value ?? "0");
            services.AddSingleton(new RedisHelper(_connectionString, _instanceName, _defaultDB, _password));

            // MySQL config 
            section = Configuration.GetSection("MySql");
            _connectionString = section.GetSection("ConnectionString").Value;
            DbHelper _db = new DbHelper(_connectionString);
            DBContainer.DB = _db;

            //Redis config
            var rabbitMQSection = Configuration.GetSection("RabbitMQ");
            var rabbitMQHost = rabbitMQSection["Host"];
            var rabbitMQPort = int.Parse(rabbitMQSection["Port"]);
            var rabbitMQUsername = rabbitMQSection["Username"];
            var rabbitMQPassword = rabbitMQSection["Password"];
            var rabbitMQVhost = rabbitMQSection["VHost"];
            services.AddSingleton(new RabbitMQHelper(rabbitMQHost, rabbitMQPort, rabbitMQUsername, rabbitMQPassword, rabbitMQVhost));
        }
    }
}
