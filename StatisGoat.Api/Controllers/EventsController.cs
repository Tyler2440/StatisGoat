using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatisGoat.Events;
using StatisGoat.ExternalApi;
using StatisGoat.Matches;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace StatisGoat.Api.Controllers
{
    public class EventsController : Controller
    {
        private readonly IFootballApi footballApi;
        private readonly IEventsRepository eventsRepository;
        private readonly IMatchesRepository matchesRepository;
        private readonly IConfiguration configuration;

        public EventsController(IFootballApi fa, IEventsRepository er, IMatchesRepository mr, IConfiguration config)
        {
            footballApi = fa;
            eventsRepository = er;
            matchesRepository = mr;
            configuration = config;
        }

        [HttpGet]
        [Route("events/match")]
        public async Task<IActionResult> GetByMatch(int mid)
        {
            return Ok(await eventsRepository.FindByMIDAsync(mid));
        }

        [HttpGet]
        [Route("events/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            if (configuration["EnableEventsBackfill"].Equals(false)) { return NotFound(); }

            List<Task<IActionResult>> tasks = new List<Task<IActionResult>>();
            foreach (MatchesInfoRecord match in await matchesRepository.FindAllAsync())
            {
                if ((await eventsRepository.FindByMIDAsync(match.MatchId)).Any()) { continue; }

                using (var response = await footballApi.GetAsync($"fixtures/events?fixture={match.MatchId}"))
                {
                    response.EnsureSuccessStatusCode();
                    var result = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new
                    {
                        response = new[]
                        {
                            new
                            {
                                time = new { elapsed = "" },
                                team = new { id = "" },
                                player = new { id = "" },
                                assist = new { id = "" },
                                type = "", detail = "", comments = ""
                            }
                        }
                    }) ;

                    tasks.Add(WriteMatchEvents(result.response, match.MatchId));
                }
            }

            Task.WaitAll(tasks.ToArray());
            return Ok();
        }

        private async Task<IActionResult> WriteMatchEvents(dynamic[] events, int match)
        {
            foreach (var e in events)
            {
                if (e.player.id is null || e.team.id is null || e.time.elapsed is null) { continue; }

                try
                {
                    await eventsRepository.SaveAsync(new EventsRecord
                    {
                        MID = match,
                        TID = int.Parse(e.team.id),
                        PID = int.Parse(e.player.id),
                        Minute = int.Parse(e.time.elapsed),
                        Type = e.type,
                        Assist = e.assist.id is null ? null : int.Parse(e.assist.id),
                        Detail = e.detail,
                        Comment = e.comments
                    });
                }
                // ran into foreign key exception on player id?
                catch (TimeoutException) { continue; }
            }

            return Ok();
        } 
    }
}
