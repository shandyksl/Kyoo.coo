using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Yoo.Core.WebSocket;

namespace Yoo.Core.WebSocket
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Threading.ThreadPool.SetMinThreads(200, 200);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}