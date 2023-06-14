using Newtonsoft.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Formations;
using StatisGoat.Lineups;
using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StatisGoat.Api.Timers
{
    public static class Players_Statistics_Timer
    {
        private static IFootballApi footballApi;
        private static IPlayer_StatisticsRepository psRepository;
        private static IMatchesRepository matchesRepository;
        private static IFormationsRepository formationsRepository;
        private static ILineupsRepository lineupsRepository;

        public static void Init(IMatchesRepository matchesRepository, IPlayer_StatisticsRepository psRepository, IFootballApi footballApi, IFormationsRepository formationsRepository, ILineupsRepository lineupsRepository)
        {
            Players_Statistics_Timer.matchesRepository = matchesRepository;
            Players_Statistics_Timer.psRepository = psRepository;
            Players_Statistics_Timer.footballApi = footballApi;
            Players_Statistics_Timer.formationsRepository = formationsRepository;
            Players_Statistics_Timer.lineupsRepository = lineupsRepository;
        }

        public static async void PSTimerEvent(object sender, ElapsedEventArgs e, int mid)
        {
            PSBackfill(mid);
        }

        public static async void PSBackfill(int matchID)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                using (var response = await footballApi.GetAsync($"fixtures/players?fixture={matchID}"))
                {
                    response.EnsureSuccessStatusCode();
                    var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                    {
                        response = new[]
                        {
                            new
                            {
                                team = new { id = 0 },
                                players = new[]
                                {
                                    new
                                    {
                                        player = new { id = 0 },
                                        statistics = new[]
                                        {
                                            new
                                            {
                                                games = new { minutes = "", rating = "", substitute = false },
                                                shots = new { total = "", on = "" },
                                                goals = new { total = "", assists = "", saves = "" , conceded = "" },
                                                passes = new { total = "", key = "", accuracy = "" },
                                                tackles = new { total = "", blocks = "", interceptions = "" },
                                                duels = new { total = "", won = "" },
                                                dribbles = new { attempts = "", success = "", past = "" },
                                                fouls = new { drawn = "", committed = "" },
                                                cards = new { yellow = "", red = "" },
                                                penalty = new { won = "", committed = "", scored = "", missed = "", saved = "" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });

                    if (result.response.Length != 2) 
                    {
                        Console.WriteLine("External API did not return 2 items - PS BACKFILL");
                        return;
                    }

                    tasks.Add(WriteTeam(result.response[0].players, matchID, result.response[0].team.id));
                    tasks.Add(WriteTeam(result.response[1].players, matchID, result.response[1].team.id));

                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (TaskCanceledException) { Console.WriteLine($"Unable to contact external API in PS Timer for match {matchID}"); }
        }

        public static async Task WriteTeam(dynamic[] players, int match, int team)
        {
            List<Task> tasks = new List<Task>();
            foreach (var player in players)
            {
                tasks.Add(WritePlayer(player.statistics[0], player.player.id, await formationsRepository.FindByMidTidAsync(match, team)));
            }
            Task.WaitAll(tasks.ToArray());
        }

        public static async Task WritePlayer(dynamic stats, int pid, int fid)
        {
            try
            {
                int lid = await lineupsRepository.FindByPidFidAsync(pid, fid);

                await psRepository.SaveAsync(new Player_StatisticsRecord
                {
                    LID = lid,
                    Minutes = ParseCheck<int>(stats.games.minutes),
                    Rating = ParseCheck<double>(stats.games.rating),
                    Substitute = stats.games.substitute is null ? 0 : stats.games.substitute,
                    Shots = ParseCheck<int>(stats.shots.total),
                    Shots_on_goal = ParseCheck<int>(stats.shots.on),
                    Goals = ParseCheck<int>(stats.goals.total),
                    Assists = ParseCheck<int>(stats.goals.assists),
                    Saves = ParseCheck<int>(stats.goals.saves),
                    Conceded = ParseCheck<int>(stats.goals.conceded),
                    Passes = ParseCheck<int>(stats.passes.total),
                    Key_passes = ParseCheck<int>(stats.passes.key),
                    Passes_accurate = stats.passes.accuracy is null ? 0 :
                        stats.passes.accuracy.EndsWith('%') ? int.Parse(stats.passes.accuracy.Trim('%')) *
                                                              ParseCheck<int>(stats.passes.total) :
                        int.Parse(stats.passes.accuracy),
                    Tackles = ParseCheck<int>(stats.tackles.total),
                    Blocks = ParseCheck<int>(stats.tackles.blocks),
                    Interceptions = ParseCheck<int>(stats.tackles.interceptions),
                    Duels = ParseCheck<int>(stats.duels.total),
                    Duels_won = ParseCheck<int>(stats.duels.won),
                    Dribbles = ParseCheck<int>(stats.dribbles.attempts),
                    Dribbles_won = ParseCheck<int>(stats.dribbles.success),
                    Dribbles_past = ParseCheck<int>(stats.dribbles.past),
                    Fouls_drawn = ParseCheck<int>(stats.fouls.drawn),
                    Fouls_committed = ParseCheck<int>(stats.fouls.committed),
                    Yellow = ParseCheck<int>(stats.cards.yellow),
                    Red = ParseCheck<int>(stats.cards.red),
                    Penalties_won = ParseCheck<int>(stats.penalty.won),
                    Penalties_conceded = ParseCheck<int>(stats.penalty.committed),
                    Penalties_scored = ParseCheck<int>(stats.penalty.scored),
                    Penalties_missed = ParseCheck<int>(stats.penalty.missed),
                    Penalties_saved = ParseCheck<int>(stats.penalty.saved)
                });
            }
            catch (Exception e)
            { }
        }

        private static dynamic ParseCheck<T>(string str)
        {
            if (typeof(T) == typeof(int)) { return str is null ? 0 : int.Parse(str); }
            else { return str is null ? 0.0 : double.Parse(str); }
        }
    }
}
