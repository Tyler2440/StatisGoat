using Microsoft.AspNetCore.Mvc;
using StatisGoat.Matches;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using StatisGoat.ExternalApi;
using Newtonsoft.Json;
using StatisGoat.Teams;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StatisGoat.Api.Controllers
{
    public class MatchesController : Controller
    {
        private readonly IMatchesRepository matchesRepository;
        private readonly ITeamsRepository teamsRepository;
        private readonly IConfiguration configuration;
        private readonly IFootballApi footballApi;

        public MatchesController(IMatchesRepository matchesRepository, ITeamsRepository teamsRepository, IConfiguration configuration, IFootballApi footballApi)
        {
            this.matchesRepository = matchesRepository;
            this.teamsRepository = teamsRepository;
            this.configuration = configuration;
            this.footballApi = footballApi;
        }

        [HttpGet]
        [Route("matches")]
        public async Task<IActionResult> GetAll() { return Ok(await matchesRepository.FindAllAsync()); }

        [HttpGet]
        [Route("matches/team")]
        public async Task<IActionResult> GetByTeam(int id, int? limit, string? date)
        {
            return Ok(await matchesRepository.FindByTidAsync(id, limit, date));
        }

        [HttpGet]
        [Route("matches/teams")]
        public async Task<IActionResult> GetByTeams(int id1, int id2)
        {
            return Ok(await matchesRepository.FindByTidTidAsync(id1, id2));
        }

        [HttpGet]
        [Route("matches/day")]
        public async Task<IActionResult> GetByDay(string day) 
        { 
            return Ok(await matchesRepository.FindByDayRangeAsync(day, day)); 
        }

        [HttpGet]
        [Route("matches/dayrange")]
        public async Task<IActionResult> GetByDayRange(string start, string end)
        {
            try
            {
                DateTime.Parse(start);
                DateTime.Parse(end);
                return Ok(await matchesRepository.FindByDayRangeAsync(start, end));
            }
            catch (FormatException) { return BadRequest(); }
        }

        [HttpGet]
        [Route("matches/match")]
        public async Task<IActionResult> GetByMatch(int mid)
        {
            return Ok(await matchesRepository.FindByMidAsync(mid));
        }

        [HttpGet]
        [Route("matches/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnableMatchesBackfill"].Equals("false")) { return NotFound(); }

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            int[] leaguesHC = new int[] { 39, 140, 61, 78, 135, 2 };
            string[] seasonsHC = new string[] { "2020", "2021", "2022" };
            foreach (int league in leaguesHC) 
            { 
                foreach (string season in seasonsHC) 
                {
                    using (var response = await footballApi.GetAsync($"fixtures?league={league}&season={season}"))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeAnonymousType(body, new
                        {
                            response = new[]
                            {
                                new
                                {
                                    fixture = new { id = 0, date = "", status = new { elapsed = "" } },
                                    league = new { name = "" },
                                    teams = new
                                    {
                                        home = new { id = 0 },
                                        away = new { id = 0 }
                                    },
                                    goals = new
                                    {
                                        home = "",
                                        away = ""
                                    }
                                }
                            }
                        });

                        tasks.Add(SaveLeagueSeason(result.response));
                    }
                } 
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> SaveLeagueSeason(dynamic matches)
        {
            foreach (var match in matches)
            {
                await SaveTeamIfNotExists(match.teams.home.id);
                await SaveTeamIfNotExists(match.teams.away.id);
                string status = SwitchElapsed(match.fixture.status.elapsed, match.fixture.date);
                await matchesRepository.SaveAsync(new MatchesRecord
                {
                    ApiID = match.fixture.id,
                    Home = match.teams.home.id,
                    Away = match.teams.away.id,
                    Competition = match.league.name,
                    DateTime = DateTime.Parse(match.fixture.date),
                    Status = status,
                    Elapsed = match.fixture.status.elapsed is null ? 0 : int.Parse(match.fixture.status.elapsed),
                    Result = SwitchResult(status, match.goals.home, match.goals.away)
                });
            }

            return Ok();
        }

        private async Task<IActionResult> SaveTeamIfNotExists(int id)
        {
            var team = await teamsRepository.FindByTIDAsync(id);
            if (team is null)
            {
                using (var response = await footballApi.GetAsync($"teams?id={id}"))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeAnonymousType(body, new
                    {
                        response = new[]
                        {
                            new
                            {
                                team = new { id = 0, name = "", country = "", logo = "" },
                                venue = new { name = "" }
                            }
                        }
                    });

                    var teams = result.response.Select(x => x.team).ToList();
                    var venues = result.response.Select(x => x.venue).ToList();

                    for (int i = 0; i < teams.Count; i++)
                    {
                        await teamsRepository.SaveAsync(new TeamsRecord
                        {
                            ApiID = teams[i].id,
                            Name = teams[i].name,
                            Nation = teams[i].country,
                            Venue = venues[i].name,
                            Badge = teams[i].logo
                        });
                    }
                }
            }

            return Ok();
        }

        private static string SwitchElapsed(string? elapsed, string date)
        {
            if (elapsed is null) 
            { 
                if (DateTime.Parse(date) < DateTime.Now) { return "PST"; }
                else { return "NS";  }
            }
            switch (int.Parse(elapsed))
            {
                case int n when n < 45: return "1H";
                case int n when n < 90: return "2H";
                default: return "FT";
            }
        }

        private static string SwitchResult(string status, string? home, string? away)
        {
            if (status == "PST") { return "N/A"; }
            if (status == "NS") { return "0-0"; }
            if (home is null || away is null) { return "N/A"; }
            return home + "-" + away;
        } 
    }
}
