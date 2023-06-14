using Newtonsoft.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Matches;
using StatisGoat.Team_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StatisGoat.Api.Timers
{
    public static class TS_Timer
    {
        private static IFootballApi footballApi;
        private static ITeam_StatisticsRepository tsRepository;
        private static IMatchesRepository matchesRepository;

        public static void Init(IMatchesRepository matchesRepository, ITeam_StatisticsRepository team_statsRepository, IFootballApi footballApi)
        {
            TS_Timer.matchesRepository = matchesRepository;
            tsRepository = team_statsRepository;
            TS_Timer.footballApi = footballApi;
        }

        public static async void TSTimerEvent(object sender, ElapsedEventArgs e, int mid)
        {
            TSBackfill(mid);
        }

        public static async void TSBackfill(int matchID)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                using (var response = await footballApi.GetAsync($"fixtures/statistics?fixture={matchID}"))
                {
                    response.EnsureSuccessStatusCode();
                    var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                    {
                        response = new[]
                        {
                            new
                            {
                                statistics = new[] { new { type = "", value = "" } } ,
                                team = new { id = 0 }
                            }
                        }
                    });

                    if (result.response.Length != 2) 
                    {
                        Console.WriteLine("External API did not return 2 items - TS BACKFILL");
                        return; 
                    }

                    tasks.Add(WriteTeamStats(result.response[0], matchID));
                    tasks.Add(WriteTeamStats(result.response[1], matchID));

                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (TaskCanceledException) { Console.WriteLine($"Unable to contact external API in TS Timer for match {matchID}"); }
        }

        public static async Task WriteTeamStats(dynamic stats, int match)
        {
            Team_StatisticsRecord record = new Team_StatisticsRecord
            {
                MID = match,
                TID = stats.team.id,
            };
            foreach (var stat in stats.statistics) { record.SwitchStat(stat.type, stat.value); }

            await tsRepository.SaveAsync(record);
        }
    }
}
