using Microsoft.AspNetCore.Mvc;
using StatisGoat.xPlayer_Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatisGoat.Player_Statistics;
using StatisGoat.Team_Statistics;
using StatisGoat.Matches;
using System.Linq;
using System.IO;
using StatisGoat.Players;
using System.Text;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using StatisGoat.Lineups;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using StatisGoat.Python;
using System.Security.Cryptography;
using StatisGoat.Api.Timers;

namespace StatisGoat.Api.Controllers
{
    public class xPlayer_StatisticsController : Controller
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration configuration;
        private readonly IPlayer_StatisticsRepository psRepository;
        private readonly IxPlayer_StatisticsRepository xpsRepository;
        private readonly ITeam_StatisticsRepository tsRepository;
        private readonly ILineupsRepository lineupsRepository;
        private readonly IMatchesRepository matchesRepository;
        private readonly IPlayersRepository playersRepository;

        public xPlayer_StatisticsController(IWebHostEnvironment environment, IConfiguration config, IPlayer_StatisticsRepository psr,
            IxPlayer_StatisticsRepository xpsr, ITeam_StatisticsRepository tsr, IMatchesRepository mr, ILineupsRepository lr, IPlayersRepository ipr)
        {
            env = environment;
            configuration = config;
            psRepository = psr;
            xpsRepository = xpsr;
            tsRepository = tsr;
            matchesRepository = mr;
            lineupsRepository = lr;
            playersRepository = ipr;
        }

        [HttpGet]
        [Route("xplayer/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnablexPlayerBackfill"].Equals("false")) { return NotFound(); }

            foreach (MatchesInfoRecord match in await matchesRepository.FindAllAsync())
            {
                if (match.DateTime.Year < 2022) { continue; }
                await WriteMatch(match);
            }

            return Ok();
        }

        [HttpGet]
        [Route("xplayer/backfillrange")]
        public async Task<IActionResult> BackfillRange(string start, string end)
        {
            if (configuration["EnablexPlayerBackfill"].Equals("false")) { return NotFound(); }

            var matchGroup = await matchesRepository.FindByDayRangeAsync(start, end);

            Console.WriteLine($"Manual backfill started from {start} to {end}, {matchGroup.Count} matches found");

            if (matchGroup.Count > 0)
            {
                DailyMatchTimer.PriorToMatchEventThreader(null, null, matchGroup, true);
            }

            return Ok();
        }

        public async Task<IActionResult> WriteMatch(MatchesInfoRecord match)
        {
            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (LineupsInfoRecord lineup in await lineupsRepository.FindByMIDAsync(match.MatchId))
            {
                tasks.Add(WritePlayer(match, lineup));
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WritePlayer(MatchesInfoRecord match, LineupsInfoRecord lineup)
        {
            var prediction = await GeneratePrediction(match.MatchId, lineup.PlayerID);
            if (prediction is not NotFoundResult)
            {
                await xpsRepository.SaveAsync((xPlayer_StatisticsRecord)((OkObjectResult)prediction).Value);
            }
            return Ok();
        }

        [HttpGet]
        [Route("xplayer/xgloadcsv")]
        public async Task<IActionResult> LoadxGCSV()
        {
            Random r = new Random();
            StringBuilder contents = new StringBuilder();
            int limit = 10, perplayer = 10;

            foreach (PlayersInfoRecord player in await playersRepository.FindAllAsync())
            {
                List<Player_StatisticsInfoRecord> matches = await psRepository.FindByPIDAsync(player.ApiID, null, null, null);
                HashSet<int> picked = new HashSet<int>();

                if (matches.Count > limit + perplayer)
                {
                    while (picked.Count < perplayer)
                    {
                        int index = r.Next(limit, matches.Count);
                        if (!picked.Contains(index)) { picked.Add(index); }
                    }
                } else if (matches.Count > limit)
                {
                    for (int i = 0; i < matches.Count - limit; i++) { picked.Add(i); }
                }

                foreach (int pickedidx in picked)
                {
                    Player_StatisticsInfoRecord matchstats = matches[pickedidx];
                    AvgPlayer_StatisticsInfoRecord avgstats = await psRepository.FindAvgByPID(player.ApiID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgoppstats = await tsRepository.FindAvgByTID(matchstats.Opponentid, null, limit, matchstats.Datetime.ToString());

                    if (avgstats.Avgminutes < 10) { continue; }
                    if (avgstats.Nummatches < 10) { continue; }

                    List<string> features = new List<string>()
                    {
                        avgstats.Avggoals.ToString(),
                        avgstats.Percent_scored.ToString(),
                        avgstats.Avgshots.ToString(),
                        avgstats.Avgshots_on_goal.ToString(),
                        avgstats.Avgminutes.ToString(),
                        avgstats.Avgdribbles.ToString(),
                        avgstats.Avgdribbles_won.ToString(),
                        avgstats.Avgteamscored.ToString(),
                        avgoppstats.Avgconceded.ToString(),
                        matchstats.Grid is null ? "0": matchstats.Grid.Split(":")[0],
                        matchstats.Grid is null ? "0" : matchstats.Grid.Split(":")[1],
                        matchstats.Goals.ToString()
                    };

                    string line = string.Join(",", features);
                    contents.AppendLine(line);
                }
            }

            System.IO.File.WriteAllText(@"..\..\..\..\Modeling\Goals\xg_train_positions.csv", contents.ToString());
            return Ok();
        }

        [HttpGet]
        [Route("xplayer/xassistloadcsv")]
        public async Task<IActionResult> AssistLoadCSV()
        {
            Random r = new Random();
            StringBuilder contents = new StringBuilder();
            int limit = 10, perplayer = 10;

            foreach (PlayersInfoRecord player in await playersRepository.FindAllAsync())
            {
                List<Player_StatisticsInfoRecord> matches = await psRepository.FindByPIDAsync(player.ApiID, null, null, null);
                HashSet<int> picked = new HashSet<int>();

                if (matches.Count > limit + perplayer)
                {
                    while (picked.Count < perplayer)
                    {
                        int index = r.Next(limit, matches.Count);
                        if (!picked.Contains(index)) { picked.Add(index); }
                    }
                }
                else if (matches.Count > limit)
                {
                    for (int i = 0; i < matches.Count - limit; i++) { picked.Add(i); }
                }

                foreach (int pickedidx in picked)
                {
                    Player_StatisticsInfoRecord matchstats = matches[pickedidx];
                    AvgPlayer_StatisticsInfoRecord avgstats = await psRepository.FindAvgByPID(player.ApiID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgoppstats = await tsRepository.FindAvgByTID(matchstats.Opponentid, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgteamstats = await tsRepository.FindAvgByTID(matchstats.TID, null, limit, matchstats.Datetime.ToString());

                    if (avgstats.Avgminutes < 10) { continue; }
                    if (avgstats.Nummatches < 10) { continue; }

                    List<string> features = new List<string>()
                    {
                        avgstats.Avgpasses.ToString(),
                        avgstats.Avgkey_passes.ToString(),
                        avgstats.Avgpass_pct.ToString(),
                        avgstats.Avgassists.ToString(),
                        avgstats.Avgdribbles.ToString(),
                        avgstats.Avgdribbles_won.ToString(),
                        avgstats.Avgminutes.ToString(),
                        avgstats.Percent_assisted.ToString(),
                        avgstats.Avgteamscored.ToString(),
                        avgoppstats.Avgconceded.ToString(),
                        avgteamstats.Avgshots.ToString(),
                        avgteamstats.Avgshots_on_goal.ToString(),
                        avgteamstats.Avgpossession.ToString(),
                        matchstats.Grid is null ? "0": matchstats.Grid.Split(":")[0],
                        matchstats.Grid is null ? "0" : matchstats.Grid.Split(":")[1],
                        matchstats.Assists.ToString()
                    };

                    string line = string.Join(",", features);
                    contents.AppendLine(line);
                    /*                    if (contents.ToString().Split("\r\n").Count() % 100 == 0) { Console.WriteLine(contents.ToString().Split("\r\n").Count()); }
                    */
                }
            }

            System.IO.File.WriteAllText(@"..\..\..\..\Modeling\Assists\xassists_train_positions.csv", contents.ToString());
            //File.WriteAllText(@"..\..\..\..\Modeling\xg_train_positions.csv", contents.ToString());

            return Ok();
        }

        [HttpGet]
        [Route("xplayer/xshotsloadcsv")]
        public async Task<IActionResult> LoadxShotsCSV()
        {
            Random r = new Random();
            StringBuilder contents = new StringBuilder();
            int limit = 10, perplayer = 10;

            foreach (PlayersInfoRecord player in await playersRepository.FindAllAsync())
            {
                List<Player_StatisticsInfoRecord> matches = await psRepository.FindByPIDAsync(player.ApiID, null, null, null);
                HashSet<int> picked = new HashSet<int>();

                if (matches.Count > limit + perplayer)
                {
                    while (picked.Count < perplayer)
                    {
                        int index = r.Next(limit, matches.Count);
                        if (!picked.Contains(index)) { picked.Add(index); }
                    }
                }
                else if (matches.Count > limit)
                {
                    for (int i = 0; i < matches.Count - limit; i++) { picked.Add(i); }
                }

                foreach (int pickedidx in picked)
                {
                    Player_StatisticsInfoRecord matchstats = matches[pickedidx];
                    AvgPlayer_StatisticsInfoRecord avgstats = await psRepository.FindAvgByPID(player.ApiID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgteamstats = await tsRepository.FindAvgByTID(matchstats.TID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgoppstats = await tsRepository.FindAvgByTID(matchstats.Opponentid, null, limit, matchstats.Datetime.ToString());

                    if (avgstats.Avgminutes < 10) { continue; }
                    if (avgstats.Nummatches < 10) { continue; }

                    List<string> features = new List<string>()
                    {
                        avgstats.Avgshots.ToString(),
                        avgstats.Avgshots_on_goal.ToString(),
                        avgstats.Avggoals.ToString(),
                        avgstats.Avgminutes.ToString(),
                        avgstats.Avgdribbles.ToString(),
                        avgstats.Avgdribbles_won.ToString(),
                        avgstats.Percent_scored.ToString(),
                        avgstats.Avgteamscored.ToString(),
                        avgoppstats.Avgconceded.ToString(),
                        avgteamstats.Avgshots.ToString(),
                        avgteamstats.Avgshots_on_goal.ToString(),
                        avgteamstats.Avgpossession.ToString(),
                        matchstats.Grid is null ? "0": matchstats.Grid.Split(":")[0],
                        matchstats.Grid is null ? "0" : matchstats.Grid.Split(":")[1],
                        matchstats.Shots.ToString()
                    };

                    string line = string.Join(",", features);
                    contents.AppendLine(line);
                }
            }

            System.IO.File.WriteAllText(@"..\..\..\..\Modeling\Shots\xshots_train.csv", contents.ToString());
            return Ok();
        }

        [HttpGet]
        [Route("xplayer/xPassesloadcsv")]
        public async Task<IActionResult> LoadxPassesCSV()
        {
            Random r = new Random();
            StringBuilder contents = new StringBuilder();
            int limit = 10, perplayer = 10;

            foreach (PlayersInfoRecord player in await playersRepository.FindAllAsync())
            {
                List<Player_StatisticsInfoRecord> matches = await psRepository.FindByPIDAsync(player.ApiID, null, null, null);
                HashSet<int> picked = new HashSet<int>();

                if (matches.Count > limit + perplayer)
                {
                    while (picked.Count < perplayer)
                    {
                        int index = r.Next(limit, matches.Count);
                        if (!picked.Contains(index)) { picked.Add(index); }
                    }
                }
                else if (matches.Count > limit)
                {
                    for (int i = 0; i < matches.Count - limit; i++) { picked.Add(i); }
                }

                foreach (int pickedidx in picked)
                {
                    Player_StatisticsInfoRecord matchstats = matches[pickedidx];
                    AvgPlayer_StatisticsInfoRecord avgstats = await psRepository.FindAvgByPID(player.ApiID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgteamstats = await tsRepository.FindAvgByTID(matchstats.TID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgoppstats = await tsRepository.FindAvgByTID(matchstats.Opponentid, null, limit, matchstats.Datetime.ToString());

                    if (avgstats.Avgminutes < 10) { continue; }
                    if (avgstats.Nummatches < 10) { continue; }

                    List<string> features = new List<string>()
                    {
                        avgstats.Avgpasses.ToString(),
                        avgstats.Avgpasses_accurate.ToString(),
                        avgstats.Avgpass_pct.ToString(),
                        avgstats.Avgminutes.ToString(),
                        avgteamstats.Avgpasses.ToString(),
                        avgteamstats.Avgpasses_accurate.ToString(),
                        avgteamstats.Avgpossession.ToString(),
                        avgoppstats.Avgpossession.ToString(),
                        avgoppstats.Avgpasses_accurate.ToString(),
                        matchstats.Grid is null ? "0": matchstats.Grid.Split(":")[0],
                        matchstats.Grid is null ? "0" : matchstats.Grid.Split(":")[1],
                        matchstats.Passes.ToString()
                    };

                    string line = string.Join(",", features);
                    contents.AppendLine(line);
                }
            }

            System.IO.File.WriteAllText(@"..\..\..\..\Modeling\Shots\xpasses_train.csv", contents.ToString());
            return Ok();
        }

        [HttpGet]
        [Route("xplayer/xSavesloadcsv")]
        public async Task<IActionResult> LoadxSavesCSV()
        {
            Random r = new Random();
            StringBuilder contents = new StringBuilder();
            int limit = 10, perplayer = 10;

            foreach (PlayersInfoRecord player in await playersRepository.FindAllAsync())
            {
                List<Player_StatisticsInfoRecord> matches = await psRepository.FindByPIDAsync(player.ApiID, null, null, null);
                HashSet<int> picked = new HashSet<int>();

                if (matches.Count > limit + perplayer)
                {
                    while (picked.Count < perplayer)
                    {
                        int index = r.Next(limit, matches.Count);
                        if (!picked.Contains(index)) { picked.Add(index); }
                    }
                }
                else if (matches.Count > limit)
                {
                    for (int i = 0; i < matches.Count - limit; i++) { picked.Add(i); }
                }

                foreach (int pickedidx in picked)
                {
                    Player_StatisticsInfoRecord matchstats = matches[pickedidx];
                    AvgPlayer_StatisticsInfoRecord avgstats = await psRepository.FindAvgByPID(player.ApiID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgteamstats = await tsRepository.FindAvgByTID(matchstats.TID, null, limit, matchstats.Datetime.ToString());
                    Avg_TeamStatisticsInfoRecord avgoppstats = await tsRepository.FindAvgByTID(matchstats.Opponentid, null, limit, matchstats.Datetime.ToString());

                    if (avgstats.Avgminutes < 10) { continue; }
                    if (avgstats.Nummatches < 10) { continue; }

                    List<string> features = new List<string>()
                    {
                        avgstats.Avgsaved.ToString(),
                        avgstats.Avgblocks.ToString(),
                        avgstats.Avgpenalties_saved.ToString(),
                        avgstats.Avgpenalties_conceded.ToString(),
                        avgstats.Avgminutes.ToString(),
                        avgteamstats.Avgconceded.ToString(),
                        avgteamstats.Avgpossession.ToString(),
                        avgoppstats.Avgshots.ToString(),
                        avgoppstats.Avgshots_on_goal.ToString(),
                        avgoppstats.Avgscored.ToString(),
                        matchstats.Grid is null ? "0": matchstats.Grid.Split(":")[0],
                        matchstats.Grid is null ? "0" : matchstats.Grid.Split(":")[1],
                        matchstats.Saves.ToString()
                    };

                    string line = string.Join(",", features);
                    contents.AppendLine(line);
                }
            }

            System.IO.File.WriteAllText(@"..\..\..\..\Modeling\Saves\xsaves_train.csv", contents.ToString());
            return Ok();
        }

        [HttpGet]
        [Route("xplayer/match")]
        public async Task<IActionResult> GetByMID(int mid, int? pid)
        {
            return Ok(await xpsRepository.GetByMID(mid, pid));
        }

        [HttpGet]
        [Route("xplayer/player")]
        public async Task<IActionResult> GetByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await xpsRepository.GetByPID(pid, competition, limit, date));
        }

        [HttpGet]
        [Route("xplayer/avg")]
        public async Task<IActionResult> GetAvgByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await xpsRepository.GetAvgByPID(pid, competition, limit, date));  
        }

        [HttpGet]
        [Route("xplayer/teamavgs")]
        public async Task<IActionResult> GetAvgByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await xpsRepository.GetAvgByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("xplayer/sum")]
        public async Task<IActionResult> GetSumByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await xpsRepository.GetSumByPID(pid, competition, limit, date));
        }

        [HttpGet]
        [Route("xplayer/teamsums")]
        public async Task<IActionResult> GetSumByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await xpsRepository.GetSumByTID(tid, competition, limit, date));
        }

        public async Task<IActionResult> GeneratePrediction(int mid, int pid)
        {
            var stats = await psRepository.FindByMIDAsync(mid, pid);
            if (!stats.Any()) { return NotFound(); }

            var playerStats = stats.First();
            var avgPlayerStats = await psRepository.FindAvgByPID(pid, null, 10, playerStats.Datetime.ToString());
            var avgTeamStats = await tsRepository.FindAvgByTID(playerStats.TID, null, 10, playerStats.Datetime.ToString());
            var avgOppStats = await tsRepository.FindAvgByTID(playerStats.Opponentid, null, 10, playerStats.Datetime.ToString());

            var grid0 = playerStats.Grid is null ? 0 : double.Parse(playerStats.Grid.Split(":")[0]);
            var grid1 = playerStats.Grid is null ? 0 : double.Parse(playerStats.Grid.Split(":")[1]);

            List<double> xg_features = new List<double> { avgPlayerStats.Avggoals, avgPlayerStats.Percent_scored, avgPlayerStats.Avgshots, avgPlayerStats.Avgshots_on_goal, avgPlayerStats.Avgminutes, 
                                                          avgPlayerStats.Avgdribbles, avgPlayerStats.Avgdribbles_won, avgTeamStats.Avgscored, avgOppStats.Avgconceded, grid0, grid1 };

            List<double> xshots_features = new List<double> { avgPlayerStats.Avgshots, avgPlayerStats.Avgshots_on_goal, avgPlayerStats.Avggoals, avgPlayerStats.Avgminutes, avgPlayerStats.Avgdribbles, 
                                                              avgPlayerStats.Avgdribbles_won, avgPlayerStats.Percent_scored, avgTeamStats.Avgscored, avgOppStats.Avgconceded, 
                                                              avgTeamStats.Avgshots, avgTeamStats.Avgshots_on_goal, avgTeamStats.Avgpossession,
                                                              grid0 };

            List<double> xa_features = new List<double> { avgPlayerStats.Avgpasses, avgPlayerStats.Avgkey_passes, avgPlayerStats.Avgpass_pct, avgPlayerStats.Avgassists, avgPlayerStats.Avgdribbles,
                                                              avgPlayerStats.Avgdribbles_won, avgPlayerStats.Avgminutes, avgPlayerStats.Percent_assisted, avgTeamStats.Avgscored, avgOppStats.Avgconceded,
                                                              avgTeamStats.Avgshots, avgTeamStats.Avgshots_on_goal, avgTeamStats.Avgpossession,
                                                              grid0, grid1 };

            List<double> xt_features = new List<double> { avgOppStats.Avgconceded, avgPlayerStats.Avgminutes, avgPlayerStats.Avgpasses, avgPlayerStats.Avgtackles, avgPlayerStats.Avgblocks,
                                                        avgPlayerStats.Avginterceptions, avgPlayerStats.Avgduels, avgPlayerStats.Avgduels_won, avgPlayerStats.Avgfouls_committed, avgPlayerStats.Avgyellow,
                                                        avgPlayerStats.Avgred, avgTeamStats.Avgpossession, grid0, grid1 };

            List<double> xi_features = new List<double>(xt_features);
            xi_features.RemoveAt(10); // remove red
            xi_features.RemoveAt(9); // remove yellow

            List<double> xfoulplay_features = new List<double> { avgPlayerStats.Avgminutes, avgPlayerStats.Avggoals, avgPlayerStats.Avgshots, avgPlayerStats.Avgassists, avgPlayerStats.Avgpasses, 
                                                                avgPlayerStats.Avgpass_pct, avgPlayerStats.Avgtackles, avgPlayerStats.Avginterceptions, avgPlayerStats.Avgduels, avgPlayerStats.Avgfouls_drawn,
                                                                avgPlayerStats.Avgfouls_committed, avgPlayerStats.Avgpenalties_conceded, avgPlayerStats.Avgred, avgPlayerStats.Avgyellow, grid0, grid1,
                                                                avgTeamStats.Avgconceded, avgTeamStats.Avgfouls, avgTeamStats.Avgpossession, avgTeamStats.Avgpasses, avgTeamStats.Avgpass_pct,
                                                                avgTeamStats.Avgyellows, avgTeamStats.Avgreds, 
                                                                avgOppStats.Avgconceded, avgOppStats.Avgfouls, avgOppStats.Avgpossession, avgOppStats.Avgpasses, avgOppStats.Avgpass_pct,
                                                                avgOppStats.Avgyellows, avgOppStats.Avgreds };

            List<double> xdribbles_features = new List<double> { avgPlayerStats.Avgdribbles, avgPlayerStats.Avgminutes, avgPlayerStats.Avggoals, avgPlayerStats.Avgassists, avgPlayerStats.Avgtackles, 
                                                                avgTeamStats.Avgpossession, grid0, grid1 };

            List<double> xsaves_features = new List<double> { avgPlayerStats.Avgsaved, avgPlayerStats.Avgblocks, avgPlayerStats.Avgpenalties_saved, avgPlayerStats.Avgpenalties_conceded, avgPlayerStats.Avgminutes,
                                                              avgTeamStats.Avgconceded, avgTeamStats.Avgpossession, avgOppStats.Avgshots, avgOppStats.Avgshots_on_goal, avgOppStats.Avgscored, 
                                                              grid0, grid1 };

            List<double> xpasses_features = new List<double> { avgPlayerStats.Avgpasses, avgPlayerStats.Avgpasses_accurate, avgPlayerStats.Avgpass_pct, avgPlayerStats.Avgminutes,
                                                               avgTeamStats.Avgpasses, avgTeamStats.Avgpasses_accurate, avgTeamStats.Avgpossession, 
                                                               avgOppStats.Avgpossession, avgOppStats.Avgpasses_accurate, 
                                                               grid0, grid1 };

            var xplayer_stats = new xPlayer_StatisticsRecord
            {
                lId = playerStats.LID,
                xShots = double.Parse(PythonHelper.ExecutePython(@"Shots\xshots_predict.py", xshots_features, @"Shots\xshots_model.joblib", env.EnvironmentName)),
                xGoals = double.Parse(PythonHelper.ExecutePython(@"Goals\xg_predict.py", xg_features, @"Goals\xg_model.joblib", env.EnvironmentName)),
                xAssists = double.Parse(PythonHelper.ExecutePython(@"Assists\xassists_predict.py", xa_features, @"Assists\xassists_model.joblib", env.EnvironmentName)),
                xSaves = double.Parse(PythonHelper.ExecutePython(@"Saves\xsaves_predict.py", xsaves_features, @"Saves\xsaves_model.joblib", env.EnvironmentName)),
                xPasses = double.Parse(PythonHelper.ExecutePython(@"Passes\xpasses_predict.py", xpasses_features, @"Passes\xpasses_model.joblib", env.EnvironmentName)),
                xTackles = double.Parse(PythonHelper.ExecutePython(@"Defensive\xtack_predict.py", xt_features, @"Defensive\xtack_model.joblib", env.EnvironmentName)),
                xInterceptions = double.Parse(PythonHelper.ExecutePython(@"Defensive\xint_predict.py", xi_features, @"Defensive\xint_model.joblib", env.EnvironmentName)),
                xDribbles = double.Parse(PythonHelper.ExecutePython(@"Dribbles\xdribbles_predict.py", xdribbles_features, @"Dribbles\xdribbles_model.joblib", env.EnvironmentName)),
                xFouls = double.Parse(PythonHelper.ExecutePython(@"FoulPlay\xfouls_predict.py", xfoulplay_features, @"FoulPlay\xfouls_model.joblib", env.EnvironmentName)),
                xYellow = double.Parse(PythonHelper.ExecutePython(@"FoulPlay\xyellows_predict.py", xfoulplay_features, @"FoulPlay\xyellows_model.joblib", env.EnvironmentName)),
                xRed = double.Parse(PythonHelper.ExecutePython(@"FoulPlay\xreds_predict.py", xfoulplay_features, @"FoulPlay\xreds_model.joblib", env.EnvironmentName))
            };
            xplayer_stats.xRating = double.Parse(PythonHelper.ExecutePython(@"Rating\xrating_predict.py", xplayer_stats.xRating_Features(), @"Rating\xrating_model.joblib", env.EnvironmentName));

            return Ok(xplayer_stats);
        }

        [HttpGet]
        [Route("xplayerstats/match")]
        public async Task<IActionResult> GetByMIDRand(int mid, int? pid)
        {
            Random random = new Random();

            List<xPlayer_StatisticsRecord> randomRecords = new List<xPlayer_StatisticsRecord>();

            for (int i = 0; i < 20; i++)
            {
                var randomRecord = new xPlayer_StatisticsRecord
                {
                    lId = mid,
                    xRating = TempGenRandomFloat(random),
                    xShots = TempGenRandomFloat(random),
                    xGoals = TempGenRandomFloat(random),
                    xAssists = TempGenRandomFloat(random),
                    xSaves = TempGenRandomFloat(random),
                    xPasses = TempGenRandomFloat(random),
                    xTackles = TempGenRandomFloat(random),
                    xInterceptions = TempGenRandomFloat(random),
                    xDribbles = TempGenRandomFloat(random),
                    xFouls = TempGenRandomFloat(random),
                    xYellow = TempGenRandomFloat(random),
                    xRed = TempGenRandomFloat(random)
                };
                randomRecords.Add(randomRecord);
            }
            return Ok(randomRecords);
        }

        private float TempGenRandomFloat(Random random)
        {
            return (float)Math.Round((random.NextDouble() * 10), 1);
        }



    }
}
