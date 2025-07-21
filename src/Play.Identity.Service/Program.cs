
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Play.Common.Configuration;


namespace Play.Identity.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAzureKeyVault()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls("http://0.0.0.0:5002")
                        .UseStartup<Startup>();
                });
    }
}
