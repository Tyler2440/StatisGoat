using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using StatisGoat.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace StatisGoat.WebApp.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string baseUrl;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;

            switch (env.EnvironmentName)
            {
                case "Production":
                    baseUrl = "44.241.69.127:90";
                    break;
                case "Development":
                    baseUrl = "localhost:5000";
                    break;
                default:
                    baseUrl = "localhost:5000";
                    break;
            }
        }

        public IActionResult Index()
        {
            try
            {
                Home home = new Home();
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                var url = $"http://{baseUrl}/api/matches/day?day=" + day;

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string matchData = reader.ReadToEnd();
                var json = JArray.Parse(matchData);

            
                List<MatchesInfoRecord> todayMatches = new List<MatchesInfoRecord>();
                foreach (var item in json)
                {
                    MatchesInfoRecord match = new MatchesInfoRecord()
                    {
                        MatchId = int.Parse(item["matchId"].ToString()),
                        Competition = item["competition"].ToString(),
                        DateTime = DateTime.Parse(item["dateTime"].ToString()).ToLocalTime(),
                        Venue = item["venue"].ToString(),
                        HomeId = int.Parse(item["homeId"].ToString()),
                        HomeName = item["homeName"].ToString(),
                        HomeNation = item["homeNation"].ToString(),
                        Homepct = 0.0,
                        Homexg = 0.0,
                        Drawpct = 0.0,
                        Awaypct = 0.0,
                        Awayxg = 0.0,
                        //homePlayers = new List<Player>(),
                        HomeFormation = item["homeFormation"] is null ? "N/A" : item["homeFormation"].ToString(),
                        HomeBadge = item["homeBadge"].ToString(),
                        Result = item["result"].ToString(),
                        AwayId = int.Parse(item["awayId"].ToString()),
                        AwayName = item["awayName"].ToString(),
                        AwayNation = item["awayNation"].ToString(),
                        //awayPlayers = new List<Player>(),
                        AwayFormation = item["awayFormation"] is null ? "N/A" : item["homeFormation"].ToString(),
                        AwayBadge = item["awayBadge"].ToString(),
                        //awayScore = item["status"].ToString() == "NS" ? "0" : item["result"].ToString().Split('-')[1]
                    };
                    double homepct = 0.0;
                    double homexg = 0.0;
                    double drawpct = 0.0;
                    double awaypct = 0.0;
                    double awayxg = 0.0;

                    if (double.TryParse(item["homepct"].ToString(), out homepct))
                    {
                        match.Homepct = homepct;
                    }
                    if (double.TryParse(item["homexg"].ToString(), out homexg))
                    {
                        match.Homexg = homexg;
                    }
                    if (double.TryParse(item["drawpct"].ToString(), out drawpct))
                    {
                        match.Drawpct = drawpct;
                    }
                    if (double.TryParse(item["awaypct"].ToString(), out awaypct))
                    {
                        match.Awaypct = awaypct;
                    }
                    if (double.TryParse(item["awayxg"].ToString(), out awayxg))
                    {
                        match.Awayxg = awayxg;
                    }

                    todayMatches.Add(match);
                }

                todayMatches.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
                home.todaysMatches = todayMatches;

                if (todayMatches.Count > 0)
                {
                    home.topPlayers = GetTopPlayers(todayMatches[0].DateTime.ToString("MM/dd/yyyy"));
                }
                else
                {
                    home.topPlayers = new List<Player_StatisticsInfoRecord>();
                }

                return View(home);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        private List<Player_StatisticsInfoRecord> GetTopPlayers(string day)
        {
            List<Player_StatisticsInfoRecord> topPlayers = new List<Player_StatisticsInfoRecord>();

            var url = $"http://{baseUrl}/api/playerstats/day?day=" + day;

            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string matchData = reader.ReadToEnd();
            var json = JArray.Parse(matchData);

            foreach (var p in json)
            {
                    Player_StatisticsInfoRecord player = new Player_StatisticsInfoRecord
                    {
                        PID = int.Parse(p["pid"].ToString()),
                        First = p["first"].ToString(),
                        Last = p["last"].ToString(),
                        Nickname = p["nickname"].ToString(),
                        TID = int.Parse(p["tid"].ToString()),
                        Teamname = p["teamname"].ToString(),
                        MID = int.Parse(p["mid"].ToString()),
                        Opponentid = int.Parse(p["opponentid"].ToString()),
                        Opponentname = p["opponentname"].ToString(),
                        Competition = p["competition"].ToString(),
                        Datetime = DateTime.Parse(p["datetime"].ToString()),
                        Status = p["status"].ToString(),
                        Teamscored = int.Parse(p["teamscored"].ToString()),
                        Teamconceded = int.Parse(p["teamconceded"].ToString()),
                        Result = p["result"].ToString(),
                        Position = p["position"].ToString(),
                        Grid = p["grid"].ToString(),
                        Pass_pct = double.Parse(p["pass_pct"].ToString()),
                        LID = int.Parse(p["lid"].ToString()),
                        Minutes = int.Parse(p["minutes"].ToString()),
                        Rating = double.Parse(p["rating"].ToString()),
                        Substitute = bool.Parse(p["substitute"].ToString()),
                        Shots = int.Parse(p["shots"].ToString()),
                        Shots_on_goal = int.Parse(p["shots_on_goal"].ToString()),
                        Goals = int.Parse(p["goals"].ToString()),
                        Assists = int.Parse(p["assists"].ToString()),
                        Saves = int.Parse(p["saves"].ToString()),
                        Conceded = int.Parse(p["conceded"].ToString()),
                        Passes = int.Parse(p["passes"].ToString()),
                        Key_passes = int.Parse(p["key_passes"].ToString()),
                        Tackles = int.Parse(p["tackles"].ToString()),
                        Blocks = int.Parse(p["blocks"].ToString()),
                        Interceptions = int.Parse(p["interceptions"].ToString()),
                        Duels = int.Parse(p["duels"].ToString()),
                        Duels_won = int.Parse(p["duels_won"].ToString()),
                        Dribbles = int.Parse(p["dribbles"].ToString()),
                        Dribbles_won = int.Parse(p["dribbles_won"].ToString()),
                        Dribbles_past = int.Parse(p["dribbles_past"].ToString()),
                        Fouls_drawn = int.Parse(p["fouls_drawn"].ToString()),
                        Fouls_committed = int.Parse(p["fouls_committed"].ToString()),
                        Yellow = int.Parse(p["yellow"].ToString()),
                        Red = int.Parse(p["red"].ToString()),
                        Penalties_won = int.Parse(p["penalties_won"].ToString()),
                        Penalties_conceded = int.Parse(p["penalties_conceded"].ToString()),
                        Penalties_scored = int.Parse(p["penalties_scored"].ToString()),
                        Penalties_missed = int.Parse(p["penalties_missed"].ToString()),
                        Penalties_saved = int.Parse(p["penalties_saved"].ToString()),
                    };
                    if (topPlayers.Count < 3)
                    {
                        topPlayers.Add(player);
                    }
                    else if (topPlayers[0].Rating < player.Rating)
                    {
                        topPlayers[0] = player;
                    }
                    else if (topPlayers[1].Rating < player.Rating)
                    {
                        topPlayers[1] = player;
                    }
                    else if (topPlayers[2].Rating < player.Rating)
                    {
                        topPlayers[2] = player;
                    }
            }

            return topPlayers;
        }

        [HttpGet]
        public Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>> ChangeDate(string date)
        {
            var url = $"http://{baseUrl}/api/matches/day?day=" + date;

            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string matchData = reader.ReadToEnd();
            var json = JArray.Parse(matchData);
            
            try
            {
                List<MatchesInfoRecord> changedMatches = new List<MatchesInfoRecord>();
                foreach (var item in json)
                {
                    MatchesInfoRecord match = new MatchesInfoRecord()
                    {
                        MatchId = int.Parse(item["matchId"].ToString()),
                        Competition = item["competition"].ToString(),
                        DateTime = DateTime.Parse(item["dateTime"].ToString()).ToUniversalTime(),
                        Venue = item["venue"].ToString(),
                        HomeId = int.Parse(item["homeId"].ToString()),
                        HomeName = item["homeName"].ToString(),
                        HomeNation = item["homeNation"].ToString(),
                        HomeFormation = item["homeFormation"] is null ? "N/A" : item["homeFormation"].ToString(),
                        HomeBadge = item["homeBadge"].ToString(),
                        Homepct = 0.0,
                        Homexg = 0.0,
                        Drawpct = 0.0,
                        Awaypct = 0.0,
                        Awayxg = 0.0,
                        Result = item["result"].ToString(),
                        AwayId = int.Parse(item["awayId"].ToString()),
                        AwayName = item["awayName"].ToString(),
                        AwayNation = item["awayNation"].ToString(),
                        AwayFormation = item["awayFormation"] is null ? "N/A" : item["homeFormation"].ToString(),
                        AwayBadge = item["awayBadge"].ToString(),
                        Status = item["status"].ToString(),
                    };

                    double homepct = 0.0;
                    double homexg = 0.0;
                    double drawpct = 0.0;
                    double awaypct = 0.0;
                    double awayxg = 0.0;

                    if (double.TryParse(item["homepct"].ToString(), out homepct))
                    {
                        match.Homepct = homepct;
                    }
                    if (double.TryParse(item["homexg"].ToString(), out homexg))
                    {
                        match.Homexg = homexg;
                    }
                    if (double.TryParse(item["drawpct"].ToString(), out drawpct))
                    {
                        match.Drawpct = drawpct;
                    }
                    if (double.TryParse(item["awaypct"].ToString(), out awaypct))
                    {
                        match.Awaypct = awaypct;
                    }
                    if (double.TryParse(item["awayxg"].ToString(), out awayxg))
                    {
                        match.Awayxg = awayxg;
                    }

                    changedMatches.Add(match);
                }



                if (changedMatches.Count != 0)
                {
                    Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>> data 
                        = new Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>>(changedMatches, GetTopPlayers(changedMatches[0].DateTime.ToString("MM/dd/yyyy")));
                    return data;
                }
                else
                {
                    Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>> data 
                        = new Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>>(changedMatches, new List<Player_StatisticsInfoRecord>());
                    return data;
                }               
            }
            catch (Exception e)
            {
                Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>> data 
                    = new Tuple<List<MatchesInfoRecord>, List<Player_StatisticsInfoRecord>>(new List<MatchesInfoRecord>(), new List<Player_StatisticsInfoRecord>());
                return data;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Tutorial()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
