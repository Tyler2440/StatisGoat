using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StatisGoat.Events;
using StatisGoat.ExternalApi;
using StatisGoat.Matches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StatisGoat.Api.Timers
{
    public static class EventsTimer
    {
        private static IFootballApi footballApi;
        private static IEventsRepository eventsRepository;
        private static IMatchesRepository matchesRepository;

        public static void Init(IMatchesRepository matchesRepository, IEventsRepository eventsRepository, IFootballApi footballApi)
        {
            EventsTimer.matchesRepository = matchesRepository;
            EventsTimer.eventsRepository = eventsRepository;
            EventsTimer.footballApi = footballApi;
        }

        public static async void EventsTimerEvent(object sender, ElapsedEventArgs e, int mid)
        {
            EventsBackfill(mid);
        }

        public static async void EventsBackfill(int matchID)
        {
            List<Task> tasks = new List<Task>();
            try
            {
                using (var response = await footballApi.GetAsync($"fixtures/events?fixture={matchID}"))
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
                    });

                    tasks.Add(WriteMatchEvents(result.response, matchID));
                }
            }
            catch (TaskCanceledException) { Console.WriteLine($"Unable to contact external API in Events Timer for match {matchID}"); }
            
        }
        public static async Task WriteMatchEvents(dynamic[] events, int match)
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
                catch (TimeoutException) { continue; }
            }
        }
    }
}
