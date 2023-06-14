using Microsoft.AspNetCore.Mvc;
using StatisGoat.ExternalApi;
using StatisGoat.Lineups;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StatisGoat.Matches;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using StatisGoat.Team_Statistics;
using System.Numerics;
using System.Linq.Expressions;
using Microsoft.Extensions.FileProviders;

namespace StatisGoat.Api.Controllers
{
    public class Team_StatisticsController : Controller
    {
        private readonly IFootballApi footballApi;
        private readonly ITeam_StatisticsRepository tsRepository;
        private readonly IMatchesRepository matchesRepository;
        private readonly IConfiguration configuration;

        public Team_StatisticsController(IFootballApi fa, ITeam_StatisticsRepository tsr, IMatchesRepository mr, IConfiguration config)
        {
            footballApi = fa;
            tsRepository = tsr;
            matchesRepository = mr;
            configuration = config;
        }

        [HttpGet]
        [Route("teamstats/avg")]
        public async Task<IActionResult> GetAvg(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await tsRepository.FindAvgByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("teamstats/sum")]
        public async Task<IActionResult> GetSum(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await tsRepository.FindSumByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("teamstats/match")]
        public async Task<IActionResult> GetByMIDTID(int mid, int tid)
        {
            return Ok(await tsRepository.FindByMIDTIDAsync(mid, tid));
        }

        [HttpGet]
        [Route("teamstats/team")]
        public async Task<IActionResult> GetByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await tsRepository.FindByTID(tid, competition, limit, date)); 
        }

        [HttpGet]
        [Route("teamstats/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnableTeamStatsBackfill"].Equals(false)) { return NotFound(); }

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (MatchesInfoRecord match in await matchesRepository.FindAllAsync()) 
            {
                if (match.Status != "FT") { continue; }

                using (var response = await footballApi.GetAsync($"fixtures/statistics?fixture={match.MatchId}"))
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

                    if (result.response.Length != 2) { continue; }

                    tasks.Add(WriteTeamStats(result.response[0], match.MatchId));
                    tasks.Add(WriteTeamStats(result.response[1], match.MatchId));
                }
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WriteTeamStats(dynamic stats, int match) 
        {
            Team_StatisticsRecord record = new Team_StatisticsRecord
            {
                MID = match,
                TID = stats.team.id,
            };
            foreach (var stat in stats.statistics) { record.SwitchStat(stat.type, stat.value); }

            await tsRepository.SaveAsync(record);
            return Ok();
        }
    }
}
