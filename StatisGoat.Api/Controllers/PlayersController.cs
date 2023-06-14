using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StatisGoat.ExternalApi;
using StatisGoat.Players;
using StatisGoat.Teams;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatisGoat.Api.Controllers
{
    public class PlayersController : Controller
    {
        private readonly IPlayersRepository playersRepository;
        private readonly ITeamsRepository teamsRepository;
        private readonly IConfiguration configuration;
        private readonly IFootballApi footballApi;

        public PlayersController(IPlayersRepository playersRepository, ITeamsRepository teamsRepository, IConfiguration configuration, IFootballApi footballApi)
        {
            this.playersRepository = playersRepository;
            this.teamsRepository = teamsRepository;
            this.configuration = configuration;
            this.footballApi = footballApi;
        }

        [HttpGet]
        [Route("players")]
        public async Task<IActionResult> GetAll() { return Ok(await playersRepository.FindAllAsync()); }

        [HttpGet]
        [Route("players/player")]
        public async Task<IActionResult> GetByPID(int id) 
        { 
            return Ok(await playersRepository.FindByPIDAsync(id)); 
        }

        [HttpGet]
        [Route("players/name")]
        public async Task<IActionResult> GetByName(string first, string last) 
        { 
            return Ok(await playersRepository.FindByNameAsync(first, last));
        }

        [HttpGet]
        [Route("players/team")]
        public async Task<IActionResult> GetByTeam(int id)
        {
            return Ok(await playersRepository.FindByTeamAsync(id));
        }

        [HttpGet]
        [Route("players/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnablePlayersBackfill"].Equals("false")) { return NotFound(); }

            var teams = await teamsRepository.FindAllAsync();
            for(int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];
                for (int page = 1; page <= 3; page++)
                {
                    using (var response = await footballApi.GetAsync($"players?team={team.ApiID}&season=2022&page={page}"))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeAnonymousType(body, new
                        {
                            response = new[]
                            {
                                new
                                {
                                    player = new
                                    {
                                        id = 0, firstname = "", lastname = "", name = "", nationality = "",
                                        birth = new { date = "" },
                                        height = "", weight = "", photo = ""
                                    }
                                }
                            }
                        });

                        var players = result.response.Select(x => x.player).ToList();

                        foreach (var player in players)
                        {
                            await playersRepository.SaveAsync(new PlayersRecord
                            {
                                ApiID = player.id,
                                TID = team.ApiID,
                                First = player.firstname,
                                Last = player.lastname,
                                Nickname = player.name,
                                DOB = player.birth.date == null ? DateTime.MinValue : DateTime.Parse(player.birth.date),
                                Height = player.height == null ? 0 : Int32.Parse(player.height.Substring(0, player.height.IndexOf(" "))),
                                Weight = player.weight == null ? 0 : Int32.Parse(player.weight.Substring(0, player.weight.IndexOf(" "))),
                                Nationality = player.nationality ?? "",
                                Headshot = player.photo ?? ""
                            });
                        }
                    }
                }
            }

            return Ok();
        }
    }
}
