using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StatisGoat.Matches;
using StatisGoat.Postgres;
using StatisGoat.Players;
using StatisGoat.Teams;
using System.Text.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Lineups;
using StatisGoat.Formations;
using StatisGoat.Team_Statistics;
using StatisGoat.Events;
using StatisGoat.Player_Statistics;
using StatisGoat.xPlayer_Statistics;
using StatisGoat.Accounts;
using StatisGoat.Authentication;
using StatisGoat.xTeam_Statistics;
using System.Timers;
using System;
using StatisGoat.Api.Timers;
using StatisGoat.Chats;
using StatisGoat.Favoriting;

namespace StatisGoat.Api
{
    public class Startup
    {
        IConfiguration Configuration { get; }

        public static IHostingEnvironment Environment { get; set; }

        readonly string specificOrigins = "_specificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Timer dailyTimer = new Timer(1000); // Initially set timer to run at startup
            dailyTimer.Elapsed += DailyMatchTimer.MidnightTimerEvent;
            dailyTimer.AutoReset = true;
            dailyTimer.Enabled = true;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddSingleton<IPostgresConnection, PostgresConnection>();
            services.AddSingleton<ITeamsRepository, TeamsRepository>();
            services.AddSingleton<IPlayersRepository, PlayersRepository>();
            services.AddSingleton<IMatchesRepository, MatchesRepository>();
            services.AddSingleton<ILineupsRepository, LineupsRepository>();
            services.AddSingleton<IFormationsRepository, FormationsRepository>();
            services.AddSingleton<ITeam_StatisticsRepository, Team_StatisticsRepository>();
            services.AddSingleton<IEventsRepository, EventsRepository>();
            services.AddSingleton<IPlayer_StatisticsRepository, Player_StatisticsRepository>();
            services.AddSingleton<IxPlayer_StatisticsRepository, xPlayer_StatisticsRepository>();
            services.AddSingleton<IxTeam_StatisticsRepository, xTeam_StatisticsRepository>();
            services.AddSingleton<IAccountRepository, AccountRepository>();
            services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
            services.AddSingleton<IChatRepository, ChatRepository>();
            services.AddSingleton<IFavoriteTeamsRepository, FavoriteTeamsRepository>();
            services.AddSingleton<IFootballApi, FootballApi>();

            services.AddCors(options =>
            {
                options.AddPolicy(name: specificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("https://localhost:5002", "http://localhost:5003", "https://localhost:44347");
                    });
            });

            var serviceProvider = services.BuildServiceProvider();

            DailyMatchTimer.Init(serviceProvider.GetService<IMatchesRepository>(), serviceProvider.GetService<IPlayersRepository>(),
                serviceProvider.GetService<IxPlayer_StatisticsRepository>(), serviceProvider.GetService<IPlayer_StatisticsRepository>(),
                serviceProvider.GetService<ITeam_StatisticsRepository>(), serviceProvider.GetService<ILineupsRepository>(),
                serviceProvider.GetService<IFormationsRepository>(), serviceProvider.GetService<IFootballApi>());

            EventsTimer.Init(serviceProvider.GetService<IMatchesRepository>(), serviceProvider.GetService<IEventsRepository>(),
                serviceProvider.GetService<IFootballApi>());
            TS_Timer.Init(serviceProvider.GetService<IMatchesRepository>(), serviceProvider.GetService<ITeam_StatisticsRepository>(),
                serviceProvider.GetService<IFootballApi>());
            Players_Statistics_Timer.Init(serviceProvider.GetService<IMatchesRepository>(), serviceProvider.GetService<IPlayer_StatisticsRepository>(),
                serviceProvider.GetService<IFootballApi>(), serviceProvider.GetService<IFormationsRepository>(), serviceProvider.GetService<ILineupsRepository>());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Environment = env;

            app.UsePathBase("/api");
            app.Use(async (
                context,
                func) =>
            {
                await func.Invoke();
            });

            app.UseAuthentication();

            app.UseRouting();

            app.UseCors(specificOrigins);
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
