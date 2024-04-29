using Yoo.Core.Settlement.Workers;

namespace Yoo.Core.Settlement
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
                    loggingBuilder.AddConsole();

                    var logPath = "Log4net.config";
                    loggingBuilder.AddLog4Net(logPath);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    new Startup(hostContext.Configuration).ConfigureServices(services);
                });
    }
}
