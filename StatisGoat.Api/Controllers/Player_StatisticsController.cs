using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using StatisGoat.Lineups;
using Microsoft.AspNetCore.Mvc;
using StatisGoat.ExternalApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using StatisGoat.Formations;
using Microsoft.AspNetCore.Http.Features;
using StatisGoat.Players;

namespace StatisGoat.Api.Controllers
{
    public class Player_StatisticsController : Controller
    {
        private readonly IFootballApi footballApi;
        private readonly IPlayer_StatisticsRepository psRepository;
        private readonly IMatchesRepository matchesRepository;
        private readonly IFormationsRepository formationsRepository;
        private readonly ILineupsRepository lineupsRepository;
        private readonly IPlayersRepository playersRepository;
        private readonly IConfiguration configuration;

        public Player_StatisticsController(IFootballApi fa, IPlayer_StatisticsRepository psr, IMatchesRepository mr, 
            IFormationsRepository fr, ILineupsRepository lr, IPlayersRepository pr, IConfiguration config)
        {
            footballApi = fa;
            psRepository = psr;
            matchesRepository = mr;
            formationsRepository = fr;
            lineupsRepository = lr;
            playersRepository = pr;
            configuration = config;
        }

        [HttpGet]
        [Route("playerstats/match")]
        public async Task<IActionResult> GetByMID(int mid, int? pid)
        {
            return Ok(await psRepository.FindByMIDAsync(mid, pid));
        }

        [HttpGet]
        [Route("playerstats/player")]
        public async Task<IActionResult> GetByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await psRepository.FindByPIDAsync(pid, competition, limit, date));
        }

        [HttpGet]
        [Route("playerstats/avg")]
        public async Task<IActionResult> GetAvgByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await psRepository.FindAvgByPID(pid, competition, limit,date));
        }

        [HttpGet]
        [Route("playerstats/teamavgs")]
        public async Task<IActionResult> GetAvgByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await psRepository.FindAvgByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("playerstats/sum")]
        public async Task<IActionResult> GetSumByPID(int pid, string? competition, int? limit, string? date)
        {
            return Ok(await psRepository.FindSumByPID(pid, competition, limit, date));
        }

        [HttpGet]
        [Route("playerstats/teamsums")]
        public async Task<IActionResult> GetSumByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await psRepository.FindSumByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("playerstats/day")]
        public async Task<IActionResult> GetByDay(string day, string? competition)
        {
            return Ok(await psRepository.FindByDay(day, competition));
        }

        [HttpGet]
        [Route("playerstats/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnablePlayerStatsBackfill"].Equals(false)) { return NotFound(); }

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (MatchesInfoRecord match in await matchesRepository.FindAllAsync())
            {
                if ((await psRepository.FindByMIDAsync(match.MatchId, null)).Any()) { continue; }
                if (match.Status != "FT") { continue; }

                using (var response = await footballApi.GetAsync($"fixtures/players?fixture={match.MatchId}"))
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

                    if (result.response.Length != 2) { continue; }

                    tasks.Add(WriteTeam(result.response[0].players, match.MatchId, result.response[0].team.id));
                    tasks.Add(WriteTeam(result.response[1].players, match.MatchId, result.response[1].team.id));
                }
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WriteTeam(dynamic[] players, int match, int team)
        {
            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (var player in players) 
            {
                tasks.Add(WritePlayer(player.statistics[0], player.player.id, await formationsRepository.FindByMidTidAsync(match, team)));
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WritePlayer(dynamic stats, int pid, int fid)
        {
            if (await playersRepository.FindByPIDAsync(pid) is null) { return Ok(); }
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
                                stats.passes.accuracy.EndsWith('%') ? int.Parse(stats.passes.accuracy.Trim('%')) * ParseCheck<int>(stats.passes.total) : 
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

            return Ok();
        }

        private static dynamic ParseCheck<T>(string str)
        {
            if (typeof(T) == typeof(int)) { return str is null ? 0 : int.Parse(str); }
            else { return str is null ? 0.0 : double.Parse(str); }
        }
    }
}
