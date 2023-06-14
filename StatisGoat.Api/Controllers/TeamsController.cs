using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StatisGoat.Teams;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using StatisGoat.ExternalApi;

namespace StatisGoat.Api.Controllers
{

    public class TeamsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ITeamsRepository teamsRepository;
        private readonly IFootballApi footballApi;

        public TeamsController(IConfiguration configuration, ITeamsRepository teamsRepository, IFootballApi footballApi)
        {
            this.configuration = configuration;
            this.teamsRepository = teamsRepository;
            this.footballApi = footballApi;
        }

        [HttpGet]
        [Route("teams")]
        public async Task<IActionResult> GetAll() { return Ok(await teamsRepository.FindAllAsync()); }

        [HttpGet]
        [Route("teams/team")]
        public async Task<IActionResult> GetByTID(int id)
        {
            return Ok(await teamsRepository.FindByTIDAsync(id));
        }

        /*[HttpGet]
        [Route("teams")]
        public async Task<IActionResult> Get(string id, string name)
        {
            if (id != null)
            {
                try
                {
                    int _id = int.Parse(id);

                    var result = await teamsRepository.FindAsync(_id);

                    return Ok(result);
                }
                catch
                {
                    return BadRequest();
                }
            }
            else if (name != null)
            {
                var result = await teamsRepository.FindByNameAsync(name);

                return Ok(result);
            }
            else
            {
                var result = await teamsRepository.FindAllAsync();

                return Ok(result);
            }
        }*/


        [HttpGet]
        [Route("teams/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnableTeamsBackfill"].Equals("false"))
            {
                return NotFound();
            }

            var leagues = new[] { 39, 140, 61, 78, 135, 2 };

            foreach (var league in leagues)
            {
                using (var response = await footballApi.GetAsync($"teams?league={league}&season=2022"))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeAnonymousType(body, new
                    {
                        response = new[]
                        {
                            new
                            {
                                team = new
                                {
                                    id = 0,
                                    name = "",
                                    country = "",
                                    logo = ""
                                },
                                venue = new
                                {
                                    name = ""
                                }
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
    }
}
