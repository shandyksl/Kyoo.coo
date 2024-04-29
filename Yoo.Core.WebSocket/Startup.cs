using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yoo.Core.Business.Services;
using Yoo.Core.Business.Interface;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.Middleware.Common;
using Yoo.Core.WebSocket.Hubs;
using Yoo.Core.WebSocket.Schedulers;
using Yoo.Core.Middleware.Filters;
using Microsoft.AspNetCore.SignalR;

namespace Yoo.Core.WebSocket
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<BearerAuthenticationAttribute>();
            services.AddHostedService<SchedulerService>();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.SetIsOriginAllowed(origin =>
                    {
                        return IsOriginAllowed(origin);
                    })
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST")
                    .AllowCredentials();
                });
            });

            services.AddSingleton(new Appsettings(Configuration));
            services.AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                }
            );

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            DefaultFilesOptions defaultOption = new DefaultFilesOptions();
            defaultOption.DefaultFileNames.Clear();
            defaultOption.DefaultFileNames.Add("index.html");

            app.UseDefaultFiles(defaultOption);
            app.UseStaticFiles();

            var logPath = "Log4net.config";
            loggerFactory.AddLog4Net(logPath);
            ApplicationLogging.LoggerFactory = loggerFactory;

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            //middleware
            app.UseMiddleware<WebSocketMiddleware>();
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DataHub>("/DataHub");
            });
        }

        private bool IsOriginAllowed(string origin)
        {
            var allowedOrigins = new List<string>
            {
                "http://localhost:3000",
                "https://yoo-game-staging.yooslot.xyz",
            };

            return allowedOrigins.Contains(origin);
        }
    }
}
