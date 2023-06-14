using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StatisGoat.Areas.Identity.Data;
using StatisGoat.Areas.Identity.Services;
using StatisGoat.Data;
using StatisGoat.WebApp.Data;
using StatisGoat.Authentication;
using StatisGoat.Postgres;
using Microsoft.AspNetCore.Http;
using StatisGoat.Favoriting;

namespace StatisGoat.WebApp
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
            //services.AddDbContext<Players_DB>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Players_DB")));

            //services.AddDbContext<Teams_DB>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Teams_DB")));

            //services.AddDbContext<Games_DB>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Games_DB")));

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IPostgresConnection, PostgresConnection>();
            services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
            services.AddSingleton<IFavoriteTeamsRepository, FavoriteTeamsRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // services.AddTransient<IEmailSender, EmailSender>();
            // services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            
            /*
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection =
                        Configuration.GetSection("Authentication:Google");

                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // We will uncomment this for production, but to show it works this is commented out. The app.Use() function below handles any 404s
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.Use(async (context, next) =>
            //    {
            //        await next();
            //        if (context.Response.StatusCode == 404)
            //        {
            //            context.Request.Path = "/Error/Error404";
            //            await next();
            //        }
            //    });
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Request.Path = "/Error/Error404";
                    await next();
                }
                if (context.Response.StatusCode == 500)
                {
                    context.Request.Path = "/Error/Error500";
                    await next();
                }
            });

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization(); // Comment out to remove authorization/login permissions entirely

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
                endpoints.MapRazorPages();
            });
        }
    }
}
