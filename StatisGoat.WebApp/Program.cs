using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StatisGoat.Areas.Identity.Data;
using StatisGoat.Data;

namespace StatisGoat.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //CreateDbIfNotExists(host);
            host.Run();
        }

        private async static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    //var context = services.GetRequiredService<SG_DB>();
                    //SG_DB_Initializer.Initialize(context);

                    UserManager<SGUser> um = services.GetRequiredService<UserManager<SGUser>>();
                    RoleManager<IdentityRole> rm = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var context2 = services.GetRequiredService<SGUsersRolesDB>();
                    await SeedUsersRolesDB.Initialize(rm, um, context2);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
