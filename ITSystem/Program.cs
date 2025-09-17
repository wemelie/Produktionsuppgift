using ITSystem.Data;
using ITSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITSystem
{
    internal class Program
    {
        private static object builder;

        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();

            var app = scope.ServiceProvider.GetRequiredService<ShopApp>();
            app.Init();
            app.RunMenu();

        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
                  .ConfigureAppConfiguration((context, config) =>
                  {
                      config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                  })
                  .ConfigureLogging(logging =>
                  {
                     
                      logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
                      logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                  })
                  .ConfigureServices((context, services) =>
                  {
                      services.AddDbContext<ShopDbContext>();
                      services.AddScoped<ShopApp>();
                  });
    }
}