using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Formations;
using StatisGoat.Lineups;
using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using StatisGoat.Players;
using StatisGoat.Python;
using StatisGoat.Team_Statistics;
using StatisGoat.xPlayer_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StatisGoat.Api.Timers
{
    public static class DailyMatchTimer
    {
        private static IMatchesRepository matchesRepository;
        private static IPlayersRepository playersRepository;
        private static IPlayer_StatisticsRepository playerStatisticsRepository;
        private static ITeam_StatisticsRepository teamStatisticsRepository;
        private static IxPlayer_StatisticsRepository xplayerStatisticsRepository;
        private static ILineupsRepository lineupsRepository;
        private static IFormationsRepository formationsRepository;
        private static IFootballApi footballApi;
        private const int MINUTE = 60000;

        public static void Init(IMatchesRepository matchesRepository, IPlayersRepository playersRepository, IxPlayer_StatisticsRepository xplayerStatisticsRepository, 
            IPlayer_StatisticsRepository playerStatisticsRepository, ITeam_StatisticsRepository teamStatisticsRepository, ILineupsRepository lineupsRepository,
            IFormationsRepository formationsRepository, IFootballApi footballApi)
        {
            DailyMatchTimer.matchesRepository = matchesRepository;
            DailyMatchTimer.playersRepository = playersRepository;
            DailyMatchTimer.playerStatisticsRepository = playerStatisticsRepository;
            DailyMatchTimer.teamStatisticsRepository = teamStatisticsRepository;
            DailyMatchTimer.xplayerStatisticsRepository = xplayerStatisticsRepository;
            DailyMatchTimer.lineupsRepository = lineupsRepository;
            DailyMatchTimer.formationsRepository = formationsRepository;
            DailyMatchTimer.footballApi = footballApi;
        }

        public static async void MidnightTimerEvent(object sender, ElapsedEventArgs e)
        {
            // Find time to midnight
            var midnight = DateTime.Today.AddDays(1).ToUniversalTime().AddHours(7).AddMinutes(30);
            var timeUntilMidnight = midnight.Subtract(DateTime.Now).TotalMilliseconds;
            ((Timer)sender).Interval = timeUntilMidnight;
            
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            
            var todaysMatches = await matchesRepository.FindByDayRangeAsync(today, today);

            var groupedMatches = todaysMatches.GroupBy(m => m.DateTime.ToString("hh:mm"))
                        .Select(group => group.ToList())
                        .ToList();

            List<MatchesInfoRecord> oldMatches = new List<MatchesInfoRecord>();
            foreach (var matchGroup in groupedMatches)
            {
                var matchTimeUtc = matchGroup[0].DateTime.AddHours(+6);

                var timeUntilMatches = matchTimeUtc.Subtract(DateTime.Now.ToUniversalTime()).TotalMilliseconds;
                var timeUntilLineups = timeUntilMatches - (MINUTE * 15); // 15 minutes before start time

                List<MatchesInfoRecord> upcomingAndOngoingMatches = new List<MatchesInfoRecord>();
                foreach (var match in matchGroup)
                {
                    if (!match.Status.Equals("FT"))
                    {
                        if (timeUntilMatches <= -(MINUTE * 90))
                        {
                            oldMatches.Add(match);
                        } 
                        else
                        {
                            upcomingAndOngoingMatches.Add(match);
                        }
                    }
                }
                
                if (upcomingAndOngoingMatches.Count > 0)
                {
                    Console.WriteLine($"Match Group Time: {matchTimeUtc.ToString()}, # of matches: {upcomingAndOngoingMatches.Count}");

                    if (timeUntilLineups < 0) timeUntilLineups = 1000;
                    Timer lineupsUpdate = new Timer(timeUntilLineups);
                    lineupsUpdate.Elapsed += (sender, e) => PriorToMatchEventThreader(sender, e, upcomingAndOngoingMatches, false);
                    lineupsUpdate.AutoReset = false;
                    lineupsUpdate.Enabled = true;

                    if (timeUntilMatches < 0) timeUntilMatches = 1000;
                    foreach (var match in upcomingAndOngoingMatches)
                    {
                        Timer matchStartTimer = new Timer(timeUntilMatches);
                        matchStartTimer.Elapsed += (sender, e) => MatchStartEvent(sender, e, match);
                        matchStartTimer.AutoReset = false;
                        matchStartTimer.Enabled = true;
                    }
                }
            }

            if (oldMatches.Count > 0)
            {
                Console.WriteLine($"# of old matches without data: {oldMatches.Count}");

                // Check if any lineups/predictions need to be done for the old matches
                PriorToMatchEventThreader(null, null, oldMatches, true);
            }
        }

        public static void PriorToMatchEventThreader(object? sender, ElapsedEventArgs? e, List<MatchesInfoRecord> matchGroup, bool doManualUpdate)
        {
            Thread childThread = new Thread(() => PriorToMatchEvent(matchGroup, doManualUpdate));
            childThread.Start();
            if (doManualUpdate)
                Console.WriteLine($"Thread started at {DateTime.Now} for old matches");
            else
                Console.WriteLine($"Thread started at {DateTime.Now} for match group time {matchGroup[0].DateTime.AddHours(6)}");
        }

        private static async void PriorToMatchEvent(List<MatchesInfoRecord> matchGroup, bool doManualUpdate)
        {
            foreach (var match in matchGroup)
            {
                // Checking if the lineup already exists in our DB
                var existingFormationHome = await formationsRepository.FindByMidTidAsync(match.MatchId, match.HomeId);
                var existingFormationAway = await formationsRepository.FindByMidTidAsync(match.MatchId, match.AwayId);
                if (existingFormationHome == 0 || existingFormationAway == 0)
                {
                    Console.WriteLine($"\nGrabbing lineups from external API for match {match.MatchId}");
                    // Get lineup from external API
                    using (var response = await footballApi.GetAsync($"fixtures/lineups?fixture={match.MatchId}"))
                    {
                        response.EnsureSuccessStatusCode();

                        var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                        {
                            response = new[]
                            {
                            new
                            {
                                team = new { id = 0 },
                                formation = "",
                                startXI = new[]
                                {
                                    new { player = new { id = "", number = 0, pos = "", grid = "" } }
                                },
                                substitutes = new[]
                                {
                                    new { player = new { id = "", number = 0, pos = "", grid = "" } }
                                }
                            }
                        }
                        });

                        if (result.response.Length != 2)
                        {
                            Console.WriteLine($"External API did not return 2 lineups for match {match.MatchId}");
                            return;
                        }

                        var firstLineup = result.response.ToList()[0];
                        var secondLineup = result.response.ToList()[1];

                        await WriteLineup(firstLineup, match);
                        await WriteLineup(secondLineup, match);
                    }
                }
                else
                {
                    Console.WriteLine($"\nMatch: {match.MatchId}, Home/Away lineups already exists");
                }

                if (Startup.Environment.EnvironmentName == "Production") { await RunAllPredictionsEC2(match); }
                else { await RunAllPredictionsAsyncLocal(match); }

                if (doManualUpdate)
                {
                    Console.WriteLine($"Manually updating relevant tables for match {match.MatchId}");

                    EventsTimer.EventsBackfill(match.MatchId);
                    Players_Statistics_Timer.PSBackfill(match.MatchId);
                    TS_Timer.TSBackfill(match.MatchId);
                    CheckMatchEndEvent(null, null, match);
                }
            }
            Console.WriteLine("Thread closed.");
        }

        private static async Task WriteLineup(dynamic lineup, MatchesInfoRecord match)
        {
            Console.WriteLine($"Writing lineup for match {match.MatchId} and team {lineup.team.id}");

            int fid = await formationsRepository.SaveAsync(new FormationsRecord
            {
                MID = match.MatchId,
                TID = lineup.team.id,
                Formation = lineup.formation ?? "N/A"
            });

            List<Task> tasks = new List<Task>();
            foreach (var start in lineup.startXI) { tasks.Add(WritePlayer(start.player, fid, lineup.team.id, match.DateTime.Year)); }
            foreach (var sub in lineup.substitutes) { tasks.Add(WritePlayer(sub.player, fid, lineup.team.id, match.DateTime.Year)); }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task WritePlayer(dynamic player, int fid, int tid, int year)
        {
            if (player.id is null) { return; }

            if (await playersRepository.FindByPIDAsync(int.Parse(player.id.ToString())) is null)
            {
                try
                {
                    await playersRepository.BackfillPlayer(int.Parse(player.id.ToString()), tid, year);
                }
                catch (Exception e) { return; }
            }

            await lineupsRepository.SaveAsync(new LineupsRecord
            {
                FID = fid,
                PID = int.Parse(player.id.ToString()),
                Position = player.pos ?? "N/A",
                Grid = player.grid,
                Number = player.number
            });
        }

        private static async Task RunAllPredictionsAsyncLocal(MatchesInfoRecord match)
        {
            var homeLineups = await lineupsRepository.FindByMIDTIDAsync(match.MatchId, match.HomeId);
            var awayLineups = await lineupsRepository.FindByMIDTIDAsync(match.MatchId, match.AwayId);
            var combinedLineups = homeLineups.Concat(awayLineups).ToList();

            var today = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd");
            var avgHomeTeamStats = await teamStatisticsRepository.FindAvgByTID(match.HomeId, null, 10, today);
            var avgAwayTeamStats = await teamStatisticsRepository.FindAvgByTID(match.AwayId, null, 10, today);

            List<Task> tasks = new List<Task>();
            foreach (var lineup in combinedLineups)
            {
                if (tasks.Count >= 10)
                {
                    Task.WaitAll(tasks.ToArray());
                    tasks.Clear();
                }

                if (await xplayerStatisticsRepository.FindAsync(lineup.LineupId) != null)
                {
                    Console.WriteLine($"Prediction already exists for player {lineup.PlayerID}");
                    continue;
                }

                try 
                {
                    bool isHomeTeam = lineup.TeamID == match.HomeId;

                    tasks.Add(WritePrediction(lineup, isHomeTeam ? match.AwayId : match.HomeId, isHomeTeam ? avgHomeTeamStats : avgAwayTeamStats, isHomeTeam ? avgAwayTeamStats : avgHomeTeamStats)); 
                }
                catch(Exception e)
                {
                    Console.WriteLine($"ERROR occured while doing prediction for match {match.MatchId} and player {lineup.PlayerID}");
                    Console.WriteLine(e.StackTrace);
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task RunAllPredictionsEC2(MatchesInfoRecord match)
        {
            var homeLineups = await lineupsRepository.FindByMIDTIDAsync(match.MatchId, match.HomeId);
            var awayLineups = await lineupsRepository.FindByMIDTIDAsync(match.MatchId, match.AwayId);
            var combinedLineups = homeLineups.Concat(awayLineups).ToList();

            var today = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd");
            var avgHomeTeamStats = await teamStatisticsRepository.FindAvgByTID(match.HomeId, null, 10, today);
            var avgAwayTeamStats = await teamStatisticsRepository.FindAvgByTID(match.AwayId, null, 10, today);

            foreach (var lineup in combinedLineups)
            {
                if (await xplayerStatisticsRepository.FindAsync(lineup.LineupId) != null)
                {
                    Console.WriteLine($"Prediction already exists for player {lineup.PlayerID}");
                    continue;
                }

                try
                {
                    bool isHomeTeam = lineup.TeamID == match.HomeId;

                    await WritePrediction(lineup, isHomeTeam ? match.AwayId : match.HomeId, isHomeTeam ? avgHomeTeamStats : avgAwayTeamStats, isHomeTeam ? avgAwayTeamStats : avgHomeTeamStats);
                } 
                catch(Exception e)
                {
                    Console.WriteLine($"ERROR occured while doing prediction for match {match.MatchId} and player {lineup.PlayerID}");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static async Task WritePrediction(LineupsInfoRecord lineup, int oppTID, Avg_TeamStatisticsInfoRecord avgTeamStats, Avg_TeamStatisticsInfoRecord avgOppStats)
        {
            Console.WriteLine($"Calculating prediction for player {lineup.PlayerID}");
            xPlayer_StatisticsRecord prediction = await RunPredictionAsync(lineup.PlayerID, lineup.TeamID, oppTID, lineup.LineupId, lineup.Grid ?? "0:0", avgTeamStats, avgOppStats);
            Console.WriteLine($"Writing prediction for player {lineup.PlayerID}");
            await xplayerStatisticsRepository.SaveAsync(prediction);
        }

        private static async void MatchStartEvent(object sender, ElapsedEventArgs e, MatchesInfoRecord match)
        {
            int delay = MINUTE * 2;

            Timer matchUpdateTimer = new Timer(delay);
            matchUpdateTimer.Elapsed += (sender, e) => EventsTimer.EventsTimerEvent(sender, e, match.MatchId);
            matchUpdateTimer.Elapsed += (sender, e) => Players_Statistics_Timer.PSTimerEvent(sender, e, match.MatchId);
            matchUpdateTimer.Elapsed += (sender, e) => TS_Timer.TSTimerEvent(sender, e, match.MatchId);
            matchUpdateTimer.Elapsed += (sender, e) => CheckMatchEndEvent(sender, e, match);

            matchUpdateTimer.AutoReset = true;
            matchUpdateTimer.Enabled = true;
        }

        private static async void CheckMatchEndEvent(object? sender, ElapsedEventArgs? e, MatchesInfoRecord match)
        {
            Console.WriteLine($"Fixture check for match {match.MatchId} at {DateTime.Now}");
            using (var response = await footballApi.GetAsync($"fixtures?id={match.MatchId}"))
            {
                response.EnsureSuccessStatusCode();
                var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                {
                    response = new[]
                    {
                            new
                            {
                                fixture = new
                                {
                                    status = new { Short = "", elapsed = "" }
                                },
                                goals = new
                                {
                                    home = 0,
                                    away = 0
                                }
                            }
                        }
                });

                if (result.response == null)
                    Console.WriteLine($"Fixture check failed for end of match {match.MatchId}");

                try
                {
                    await matchesRepository.SaveAsync(new MatchesRecord
                    {
                        ApiID = match.MatchId,
                        Away = match.AwayId,
                        Home = match.HomeId,
                        Competition = match.Competition,
                        DateTime = match.DateTime,
                        Elapsed = result.response[0].fixture.status.elapsed is null ? 0 : int.Parse(result.response[0].fixture.status.elapsed),
                        Status = result.response[0].fixture.status.Short,
                        Result = String.Join('-', result.response[0].goals.home, result.response[0].goals.away)
                    });

                    if (result.response[0].fixture.status.Short.Equals("FT"))
                    {
                        Console.WriteLine($"Match {match.MatchId} completed");

                        if (sender != null)
                            ((Timer)sender).Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Issue updating match {match.MatchId} end result. Error: {ex.StackTrace} Error Message {ex.Message}");
                }
                

                
            }
        }
        
        private static async Task<xPlayer_StatisticsRecord> RunPredictionAsync(int pid, int tid, int oppId, int lid, string grid, Avg_TeamStatisticsInfoRecord avgTeamStats, Avg_TeamStatisticsInfoRecord avgOppStats)
        {
            var today = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd");

            var avgPlayerStats = await playerStatisticsRepository.FindAvgByPID(pid, null, 10, today);
            
            var grid0 = double.Parse(grid.Split(":")[0]);
            var grid1 = double.Parse(grid.Split(":")[1]);

            List<double> rawFeatures = new List<double>
            {
                grid0, grid1, avgPlayerStats.Avgdribbles, avgPlayerStats.Avgdribbles_won, avgPlayerStats.Avgpasses, avgPlayerStats.Avgpasses_accurate, avgPlayerStats.Avgkey_passes, avgPlayerStats.Avgpass_pct, 
                avgPlayerStats.Avgassists, avgPlayerStats.Percent_assisted, avgPlayerStats.Avgshots, avgPlayerStats.Avgshots_on_goal, avgPlayerStats.Avggoals, avgPlayerStats.Percent_scored, 
                avgPlayerStats.Avgsaved, avgPlayerStats.Avgtackles, avgPlayerStats.Avgblocks, avgPlayerStats.Avginterceptions, avgPlayerStats.Avgduels, avgPlayerStats.Avgduels_won, avgPlayerStats.Avgfouls_drawn, 
                avgPlayerStats.Avgfouls_committed, avgPlayerStats.Avgpenalties_conceded, avgPlayerStats.Avgpenalties_saved, avgPlayerStats.Avgyellow, avgPlayerStats.Avgred, avgPlayerStats.Avgminutes,

                avgTeamStats.Avgpasses, avgTeamStats.Avgpasses_accurate, avgTeamStats.Avgpass_pct, avgTeamStats.Avgshots, avgTeamStats.Avgshots_on_goal, avgTeamStats.Avgscored, avgTeamStats.Avgconceded, 
                avgTeamStats.Avgfouls, avgTeamStats.Avgyellows, avgTeamStats.Avgreds, avgTeamStats.Avgpossession,

                avgOppStats.Avgpasses, avgOppStats.Avgpasses_accurate, avgOppStats.Avgpass_pct, avgOppStats.Avgshots, avgOppStats.Avgshots_on_goal, avgOppStats.Avgscored, avgOppStats.Avgconceded, 
                avgOppStats.Avgfouls, avgOppStats.Avgyellows, avgOppStats.Avgreds, avgOppStats.Avgpossession
            };
            
            var env = Startup.Environment.EnvironmentName;

            var result = PythonHelper.ExecutePythonV2(rawFeatures, env).Split(", ").ToList().Select(double.Parse).ToList();

            var xplayer_stats = new xPlayer_StatisticsRecord
            {
                lId = lid,
                xShots = result[0],
                xGoals = result[1],
                xAssists = result[2],
                xSaves = result[3],
                xPasses = result[4],
                xTackles = result[5],
                xInterceptions = result[6],
                xDribbles = result[7],
                xFouls = result[8],
                xYellow = result[9],
                xRed = result[10],
                xRating = result[11]
            };

            return xplayer_stats;
        }
    }
}
