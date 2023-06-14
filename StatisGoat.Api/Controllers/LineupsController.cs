using Microsoft.AspNetCore.Mvc;
using StatisGoat.ExternalApi;
using StatisGoat.Lineups;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StatisGoat.Matches;
using Newtonsoft.Json;
using StatisGoat.Players;
using System.Threading;
using StatisGoat.Formations;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;

namespace StatisGoat.Api.Controllers
{
    public class LineupsController : Controller
    {
        private readonly IFootballApi footballApi;
        private readonly ILineupsRepository lineupsRepository;
        private readonly IMatchesRepository matchesRepository;
        private readonly IPlayersRepository playersRepository;
        private readonly IFormationsRepository formationsRepository;
        private readonly IConfiguration configuration;

        public LineupsController(IFootballApi fa, ILineupsRepository lr,
            IMatchesRepository mr, IPlayersRepository pr, IFormationsRepository fr,
            IConfiguration config)
        {
            footballApi = fa;
            lineupsRepository = lr;
            matchesRepository = mr;
            playersRepository = pr;
            formationsRepository = fr;
            configuration = config;
        }

        [HttpGet]
        [Route("lineups/match")]
        public async Task<IActionResult> GetByMatch(int mid, int tid = 0)
        {
            if (tid == 0) { return Ok(await lineupsRepository.FindByMIDAsync(mid)); }
            else { return Ok(await lineupsRepository.FindByMIDTIDAsync(mid, tid)); }
        }

        [HttpGet]
        [Route("lineups/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnableLineupsBackfill"].Equals(false)) { return NotFound(); }

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (MatchesInfoRecord match in await matchesRepository.FindAllAsync())
            {
                if ((await lineupsRepository.FindByMIDAsync(match.MatchId)).Any()) { continue; }

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

                    if (result.response.Length != 2) { continue; }

                    tasks.Add(WriteLineup(result.response.ToList()[0], match));
                    tasks.Add(WriteLineup(result.response.ToList()[1], match));
                }
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WriteLineup(dynamic lineup, MatchesInfoRecord match)
        {
            int fid = await formationsRepository.SaveAsync(new FormationsRecord
            {
                MID = match.MatchId,
                TID = lineup.team.id,
                Formation = lineup.formation ?? "N/A"
            });

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (var start in lineup.startXI) { tasks.Add(WritePlayer(start.player, fid, lineup.team.id, match.DateTime.Year)); }
            foreach (var sub in lineup.substitutes) { tasks.Add(WritePlayer(sub.player, fid, lineup.team.id, match.DateTime.Year)); }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WritePlayer(dynamic player, int fid, int team, int year)
        {
            if (player.id is null) { return NotFound(); }

            if (await playersRepository.FindByPIDAsync(int.Parse(player.id)) is null) 
            {
                using (var response = await footballApi.GetAsync($"players?id={player.id}&season={year}"))
                {
                    response.EnsureSuccessStatusCode();
                    var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                    {
                        response = new[]
                        {
                            new
                            {
                                player = new
                                {
                                    id = 0, firstname = "", lastname = "", nationality = "",
                                    birth = new { date = "" },
                                    height = "", weight = "", photo = ""
                                }
                            }
                        }   
                    });

                    if (!result.response.Any()) { return NotFound(); }

                    var player_info = result.response[0].player;
                    await playersRepository.SaveAsync(new PlayersRecord
                    {
                        ApiID = player_info.id,
                        TID = team,
                        First = player_info.firstname,
                        Last = player_info.lastname,
                        DOB = player_info.birth.date == null ? DateTime.MinValue : DateTime.Parse(player_info.birth.date),
                        Height = player_info.height == null ? 0 : Int32.Parse(player_info.height.Substring(0, player_info.height.IndexOf(" "))),
                        Weight = player_info.weight == null ? 0 : Int32.Parse(player_info.weight.Substring(0, player_info.weight.IndexOf(" "))),
                        Nationality = player_info.nationality ?? "",
                        Headshot = player_info.photo ?? ""
                    });
                }
            }

            await lineupsRepository.SaveAsync(new LineupsRecord
            {
                FID = fid,
                PID = int.Parse(player.id),
                Position = player.pos ?? "N/A",
                Grid = player.grid,
                Number = player.number
            });

            return Ok();
        }
    }
}
