using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.OpenApi.Models;
using Yoo.Core.Common.Helper;
using Yoo.Core.Common.Storage;
using Yoo.Core.Middleware.Common;

namespace Yoo.Core.BackOffice
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
            //services.AddScoped<IPlayerService, PlayerService>();
            services.AddSingleton(new Appsettings(Configuration));
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            }
            );

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Yoo.Core.BackOffice", Version = "v1" });

                // Define the security scheme
                //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Description = "JWT Authorization header using the Bearer scheme",
                //    Type = SecuritySchemeType.Http,
                //    Scheme = "bearer"
                //});

                // Describe what actions require authorization
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            }
                //        },
                //        new List<string>()
                //    }
                //});
            });

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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Yoo.Core.BackOffice v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //middleware
            app.UseMiddleware<LoggerMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
