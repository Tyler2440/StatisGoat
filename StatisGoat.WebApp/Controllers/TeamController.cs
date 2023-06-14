using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StatisGoat.Matches;
using StatisGoat.Team_Statistics;
using StatisGoat.WebApp.Models;
using StatisGoat.xTeam_Statistics;
using StatisGoat.Player_Statistics;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace StatisGoat.WebApp.Controllers
{
    [AllowAnonymous]
    public class TeamController : Controller
    {
        private string baseUrl;

        public TeamController(IWebHostEnvironment env)
        {
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

        // GET: Teams
        public IActionResult Index()
        {
            try
            {
                var url = $"http://{baseUrl}/api/teams";

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string gameData = reader.ReadToEnd();
                var json = JArray.Parse(gameData);

                //Dictionary<string, List<Team>> teams = new Dictionary<string, List<Team>>();
                List<Team> teams = new List<Team>();
                foreach(var item in json)
                {               
                    Team team = new Team();
                    team.TID = Int32.Parse(item["apiID"]?.ToString());
                    team.Teamname = item["name"]?.ToString();
                    team.Badge = item["badge"]?.ToString();
                    team.Nation = item["nation"].ToString();
                    teams.Add(team);
                    //if (teams.ContainsKey(team.Nation))
                    //{
                    //    teams[team.Nation].Add(team);
                    //}
                    //else
                    //{
                    //    teams.Add(team.Nation, new List<Team>() { team });
                    //}                   
                }
                teams.Sort((x, y) => x.Nation.CompareTo(y.Nation));
                return View(teams);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        // GET: Teams/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                int limit = 10;
                DateTime end = DateTime.Today;
                DateTime start = end.AddDays(-30);

                var urlTeam = $"http://{baseUrl}/api/teams/team?id={id}";

                var requestTeam = WebRequest.Create(urlTeam);

                requestTeam.Method = "GET";

                // Get the team data
                using var webResponseTeam = requestTeam.GetResponse();
                using var webStreamTeam = webResponseTeam.GetResponseStream();

                using var readerTeam = new StreamReader(webStreamTeam);
                string gameDataTeam = readerTeam.ReadToEnd();
                var jsonTeam = JObject.Parse(gameDataTeam);

                Team team = new Team
                {
                    TID = int.Parse(jsonTeam["apiID"]?.ToString()),
                    Teamname = jsonTeam["name"].ToString(),
                    Badge = jsonTeam["badge"].ToString(),
                    Nation = jsonTeam["nation"].ToString(),
                    Venue = jsonTeam["venue"].ToString(),
                    previousMatches = new List<MatchesInfoRecord>(),
                    previousMatchesStats = new List<Team_StatisticsInfoRecord>(),
                    upcomingMatches = new List<MatchesInfoRecord>(),
                    upcomingMatchesStats = new List<xTeam_StatisticsRecord>(),
                    topAvgPlayerStats = new List<AvgPlayer_StatisticsInfoRecord>(),
                    topSumPlayerStats = new List<SumPlayer_StatisticsInfoRecord>(),
                    topPlayerRatings = new List<AvgPlayer_StatisticsInfoRecord>(),
                    topPlayerGoals = new List<SumPlayer_StatisticsInfoRecord>(),
                    topPlayerAssists = new List<SumPlayer_StatisticsInfoRecord>(),
                    Roster = new List<Player>(),
                };

                team.Roster = GetTeamRoster(id);
                team.previousMatchesStats = GetPreviousMatchesStats(id, limit);
                team.avgStats = GetAvgStats(id, limit);
                GetTopPlayerStats(id, limit, team);
                team.previousMatches = GetPreviousMatches(id, limit);

                team.previousxMatchesStats = GetXPreviousMatchesStats(id, limit);
                //team.upcomingMatchesStats = GetUpcomingMatchStats(id, limit);

                return View(team);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        private List<xTeam_StatisticsInfoRecord> GetXPreviousMatchesStats(int id, int limit)
        {
            var url = $"http://{baseUrl}/api/xteam/team?tid={id}&limit={limit}";
            var request = WebRequest.Create(url);

            // Get the upcoming game predictions
            using var webRespone = request.GetResponse();
            using var webStream = webRespone.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string gameData = reader.ReadToEnd();
            var json = JArray.Parse(gameData);

            List<xTeam_StatisticsInfoRecord> MatchStats = new List<xTeam_StatisticsInfoRecord>();

            foreach (var match in json)
            {
                MatchStats.Add(new xTeam_StatisticsInfoRecord
                {
                    TID = int.Parse(match["tid"].ToString()),
                    Teamname = match["teamname"].ToString(),
                    MID = int.Parse(match["mid"].ToString()),
                    OpponentID = int.Parse(match["opponentID"].ToString()),
                    Opponentname = match["opponentname"].ToString(),
                    Competition = match["competition"].ToString(),
                    Datetime = DateTime.Parse(DateTime.Parse(match["datetime"].ToString()).ToLocalTime().ToShortDateString()),
                    Status = match["status"].ToString(),
                    Teamscored = int.Parse(match["teamscored"].ToString()),
                    Result = match["result"].ToString(),
                    xRating = double.Parse(match["xRating"].ToString()),
                    Rating_perf = double.Parse(match["rating_perf"].ToString()),
                    xShots = double.Parse(match["xShots"].ToString()),
                    Shots_perf = double.Parse(match["shots_perf"].ToString()),
                    xGoals = double.Parse(match["xGoals"].ToString()),
                    Goals_perf = double.Parse(match["goals_perf"].ToString()),
                    xAssists = double.Parse(match["xAssists"].ToString()),
                    Assists_perf = double.Parse(match["assists_perf"].ToString()),
                    xSaves = double.Parse(match["xSaves"].ToString()),
                    Saves_perf = double.Parse(match["saves_perf"].ToString()),
                    xPasses = double.Parse(match["xPasses"].ToString()),
                    Passes_perf = double.Parse(match["passes_perf"].ToString()),
                    xTackles = double.Parse(match["xTackles"].ToString()),
                    Tackles_perf = double.Parse(match["tackles_perf"].ToString()),
                    xInterceptions = double.Parse(match["xInterceptions"].ToString()),
                    Interceptions_perf = double.Parse(match["interceptions_perf"].ToString()),
                    xDribbles = double.Parse(match["xDribbles"].ToString()),
                    Dribbles_perf = double.Parse(match["dribbles_perf"].ToString()),
                    xFouls = double.Parse(match["xFouls"].ToString()),
                    Fouls_perf = double.Parse(match["fouls_perf"].ToString()),
                    xYellow = double.Parse(match["xYellow"].ToString()),
                    Yellow_perf = double.Parse(match["yellow_perf"].ToString()),
                    xRed = double.Parse(match["xRed"].ToString()),
                    Red_perf = double.Parse(match["red_perf"].ToString()),
                });
            }

            return MatchStats;
        }

        //private List<xTeam_StatisticsRecord> GetUpcomingMatchStats(int id, int limit)
        //{
        //    var urlUpcoming = $"http://{baseUrl}/api/xteam/team?tid={id}&limit={limit}";
        //    var requestUpcoming = WebRequest.Create(urlUpcoming);

        //    // Get the upcoming game predictions
        //    using var webResponeUpcoming = requestUpcoming.GetResponse();
        //    using var webStreamUpcoming = webResponeUpcoming.GetResponseStream();

        //    using var readerUpcoming = new StreamReader(webStreamUpcoming);
        //    string gameDataUpcoming = readerUpcoming.ReadToEnd();
        //    var jsonUpcoming = JArray.Parse(gameDataUpcoming);

        //    List<xTeam_StatisticsInfoRecord> upcomingMatchStats = new List<xTeam_StatisticsInfoRecord>();

        //    foreach (var match in jsonUpcoming)
        //    {
        //        upcomingMatchStats.Add(new xTeam_StatisticsInfoRecord
        //        {
        //            TID = int.Parse(match["tid"].ToString()),
        //            Teamname = match["teamname"].ToString(),
        //            MID = int.Parse(match["mid"].ToString()),
        //            OpponentID = int.Parse(match["opponentID"].ToString()),
        //            Opponentname = match["opponentname"].ToString(),
        //            Competition = match["competition"].ToString(),
        //            Datetime = DateTime.Parse(match["datetime"].ToString()),
        //            Status = match["status"].ToString(),
        //            Teamscored = int.Parse(match["teamscored"].ToString()),
        //            Result = match["result"].ToString(),
        //            xRating = double.Parse(match["xRating"].ToString()),
        //            Rating_perf = double.Parse(match["rating_perf"].ToString()),
        //            xShots = double.Parse(match["xShots"].ToString()),
        //            Shots_perf = double.Parse(match["shots_perf"].ToString()),
        //            xGoals = double.Parse(match["xGoals"].ToString()),
        //            Goals_perf = double.Parse(match["goals_perf"].ToString()),
        //            xAssists = double.Parse(match["xAssists"].ToString()),
        //            Assists_perf = double.Parse(match["assists_perf"].ToString()),
        //            xSaves = double.Parse(match["xSaves"].ToString()),
        //            Saves_perf = double.Parse(match["saves_perf"].ToString()),
        //            xPasses = double.Parse(match["xPasses"].ToString()),
        //            Passes_perf = double.Parse(match["passes_perf"].ToString()),
        //            xTackles = double.Parse(match["xTackles"].ToString()),
        //            Tackles_perf = double.Parse(match["tackles_perf"].ToString()),
        //            xInterceptions = double.Parse(match["xInterceptions"].ToString()),
        //            Interceptions_perf = double.Parse(match["interceptions_perf"].ToString()),
        //            xDribbles = double.Parse(match["xDribbles"].ToString()),
        //            Dribbles_perf = double.Parse(match["dribbles_perf"].ToString()),
        //            xFouls = double.Parse(match["xFouls"].ToString()),
        //            Fouls_perf = double.Parse(match["fouls_perf"].ToString()),
        //            xYellow = double.Parse(match["xYellow"].ToString()),
        //            Yellow_perf = double.Parse(match["yellow_perf"].ToString()),
        //            xRed = double.Parse(match["xRed"].ToString()),
        //            Red_perf = double.Parse(match["red_perf"].ToString()),
        //        });
        //    }

        //    return upcomingMatchStats;
        //}

        private List<MatchesInfoRecord> GetPreviousMatches(int id, int limit)
        {
            var url = $"http://{baseUrl}/api/matches/team?id={id}&limit={limit}&date=\"{DateTime.Today}\"";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JArray json = JArray.Parse(eventReader);

            List<MatchesInfoRecord> previousMatchInfoRecords = new List<MatchesInfoRecord>();
            foreach (var obj in json)
            {
                if (obj["status"].ToString() != "NS")
                {
                    previousMatchInfoRecords.Add(new MatchesInfoRecord
                    {
                        MatchId = int.Parse(obj["matchId"].ToString()),
                        Competition = obj["competition"].ToString(),
                        DateTime = DateTime.Parse(obj["dateTime"].ToString()),
                        Venue = obj["venue"].ToString(),
                        HomeId = int.Parse(obj["homeId"].ToString()),
                        HomeName = obj["homeName"].ToString(),
                        HomeNation = obj["homeNation"].ToString(),
                        HomeFormation = obj["homeFormation"] is null ? "N/A" : obj["homeFormation"].ToString(),
                        HomeBadge = obj["homeBadge"].ToString(),
                        Result = obj["result"].ToString(),
                        Status = obj["status"].ToString(),
                        AwayId = int.Parse(obj["awayId"].ToString()),
                        AwayName = obj["awayName"].ToString(),
                        AwayNation = obj["awayNation"].ToString(),
                        AwayFormation = obj["awayFormation"] is null ? "N/A" : obj["homeFormation"].ToString(),
                        AwayBadge = obj["awayBadge"].ToString(),
                    });
                }
            }

            previousMatchInfoRecords.Sort((x, y) => DateTime.Compare(y.DateTime, x.DateTime));


            return previousMatchInfoRecords;
        }

        private void GetTopPlayerStats(int id, int limit, Team team)
        {
            Dictionary<string, List<object>> topPlayerStats = new Dictionary<string, List<object>>();
            var urlAvg = $"http://{baseUrl}/api/playerstats/teamavgs?tid={id}&limit={limit}";
            var requestAvg = WebRequest.Create(urlAvg);

            // Get the average player stats of the team
            using var webResponseAvg = requestAvg.GetResponse();
            using var webStreamAvg = webResponseAvg.GetResponseStream();

            using var readerAvg = new StreamReader(webStreamAvg);
            string gameDataAvg = readerAvg.ReadToEnd();
            var jsonAvg = JArray.Parse(gameDataAvg);

            // Get the avg stats for each player
            foreach (var item in jsonAvg)
            {
                team.topAvgPlayerStats.Add(new AvgPlayer_StatisticsInfoRecord
                {
                    PID = int.Parse(item["pid"].ToString()),
                    First = item["first"].ToString(),
                    Last = item["last"].ToString(),
                    Nickname = item["nickname"].ToString(),
                    Headshot = item["headshot"].ToString(),
                    Nummatches = int.Parse(item["nummatches"].ToString()),
                    Avgteamscored = double.Parse(item["avgteamscored"].ToString()),
                    Avgteamconceded = double.Parse(item["avgteamconceded"].ToString()),
                    Avgminutes = double.Parse(item["avgminutes"].ToString()),
                    Avgrating = Math.Round(double.Parse(item["avgrating"].ToString()), 2),
                    Avgshots = double.Parse(item["avgshots"].ToString()),
                    Avgshots_on_goal = double.Parse(item["avgshots_on_goal"].ToString()),
                    Avggoals = Math.Round(double.Parse(item["avggoals"].ToString()), 2),
                    Avgassists = Math.Round(double.Parse(item["avgassists"].ToString()), 2),
                    Avggoal_contributions = double.Parse(item["avggoal_contributions"].ToString()),
                    Avgsaved = double.Parse(item["avgsaved"].ToString()),
                    Avgconceded = double.Parse(item["avgconceded"].ToString()),
                    Avgpasses = double.Parse(item["avgpasses"].ToString()),
                    Avgkey_passes = double.Parse(item["avgkey_passes"].ToString()),
                    Avgpasses_accurate = double.Parse(item["avgpasses_accurate"].ToString()),
                    Avgpass_pct = double.Parse(item["avgpass_pct"].ToString()),
                    Avgtackles = double.Parse(item["avgtackles"].ToString()),
                    Avgblocks = double.Parse(item["avgblocks"].ToString()),
                    Avginterceptions = double.Parse(item["avginterceptions"].ToString()),
                    Avgduels = double.Parse(item["avgduels"].ToString()),
                    Avgduels_won = double.Parse(item["avgduels_won"].ToString()),
                    Avgdribbles = double.Parse(item["avgdribbles"].ToString()),
                    Avgdribbles_won = double.Parse(item["avgdribbles_won"].ToString()),
                    Avgdribbles_past = double.Parse(item["avgdribbles_past"].ToString()),
                    Avgfouls_drawn = double.Parse(item["avgfouls_drawn"].ToString()),
                    Avgfouls_committed = double.Parse(item["avgfouls_committed"].ToString()),
                    Avgyellow = double.Parse(item["avgyellow"].ToString()),
                    Avgred = double.Parse(item["avgred"].ToString()),
                    Avgpenalties_won = double.Parse(item["avgpenalties_won"].ToString()),
                    Avgpenalties_conceded = double.Parse(item["avgpenalties_conceded"].ToString()),
                    Avgpenalties_scored = double.Parse(item["avgpenalties_scored"].ToString()),
                    Avgpenalties_missed = double.Parse(item["avgpenalties_missed"].ToString()),
                    Avgpenalties_saved = double.Parse(item["avgpenalties_saved"].ToString()),
                    Percent_scored = double.Parse(item["percent_scored"].ToString()),
                    Percent_assisted = double.Parse(item["percent_assisted"].ToString()),
                    Percent_contributed = double.Parse(item["percent_contributed"].ToString()),
                });
            }

            // Now get player sum stats
            var urlSum = $"http://{baseUrl}/api/playerstats/teamsums?tid={id}&limit=10";
            var requestSum = WebRequest.Create(urlSum);

            // Get the average player stats of the team
            using var webResponseSum = requestSum.GetResponse();
            using var webStreamSum = webResponseSum.GetResponseStream();

            using var readerSum = new StreamReader(webStreamSum);
            string gameDataSum = readerSum.ReadToEnd();
            var jsonSum = JArray.Parse(gameDataSum);

            // Get the sum stats for each player
            foreach (var item in jsonSum)
            {
                team.topSumPlayerStats.Add(new SumPlayer_StatisticsInfoRecord
                {
                    PID = int.Parse(item["pid"].ToString()),
                    First = item["first"].ToString(),
                    Last = item["last"].ToString(),
                    Nickname = item["nickname"].ToString(),
                    Headshot = item["headshot"].ToString(),                    
                    Nummatches = int.Parse(item["nummatches"].ToString()),                    
                    Sumteamscored = int.Parse(item["sumteamscored"].ToString()),                 
                    Sumteamconceded = int.Parse(item["sumteamconceded"].ToString()),
                    Summinutes = int.Parse(item["summinutes"].ToString()),
                    Sumrating = int.Parse(item["sumrating"].ToString()),
                    Sumshots = int.Parse(item["sumshots"].ToString()),
                    Sumshots_on_goal = int.Parse(item["sumshots_on_goal"].ToString()),
                    Sumgoals = int.Parse(item["sumgoals"].ToString()),
                    Sumassists = int.Parse(item["sumassists"].ToString()),
                    Sumgoal_contributions = int.Parse(item["sumgoal_contributions"].ToString()),
                    Sumsaved = int.Parse(item["sumsaved"].ToString()),
                    Sumconceded = int.Parse(item["sumconceded"].ToString()),
                    Sumpasses = int.Parse(item["sumpasses"].ToString()),
                    Sumkey_passes = int.Parse(item["sumkey_passes"].ToString()),
                    Sumpasses_accurate = int.Parse(item["sumpasses_accurate"].ToString()),
                    Sumpass_pct = int.Parse(item["sumpass_pct"].ToString()),
                    Sumtackles = int.Parse(item["sumtackles"].ToString()),
                    Sumblocks = int.Parse(item["sumblocks"].ToString()),
                    Suminterceptions = int.Parse(item["suminterceptions"].ToString()),
                    Sumduels = int.Parse(item["sumduels"].ToString()),
                    Sumduels_won = int.Parse(item["sumduels_won"].ToString()),
                    Sumdribbles = int.Parse(item["sumdribbles"].ToString()),
                    Sumdribbles_won = int.Parse(item["sumdribbles_won"].ToString()),
                    Sumdribbles_past = int.Parse(item["sumdribbles_past"].ToString()),
                    Sumfouls_drawn = int.Parse(item["sumfouls_drawn"].ToString()),
                    Sumfouls_committed = int.Parse(item["sumfouls_committed"].ToString()),
                    Sumyellow = int.Parse(item["sumyellow"].ToString()),
                    Sumred = int.Parse(item["sumred"].ToString()),
                    Sumpenalties_won = int.Parse(item["sumpenalties_won"].ToString()),
                    Sumpenalties_conceded = int.Parse(item["sumpenalties_conceded"].ToString()),
                    Sumpenalties_scored = int.Parse(item["sumpenalties_scored"].ToString()),
                    Sumpenalties_missed = int.Parse(item["sumpenalties_missed"].ToString()),
                    Sumpenalties_saved = int.Parse(item["sumpenalties_saved"].ToString()),
                    Percent_scored = double.Parse(item["percent_scored"].ToString()),
                    Percent_assisted = double.Parse(item["percent_assisted"].ToString()),
                    Percent_contributed = double.Parse(item["percent_contributed"].ToString()),
                });
            }

            if (team.topAvgPlayerStats.Count > 0)
            {
                // Sort avgPlayerStats by ratings and save it
                team.topAvgPlayerStats.Sort((p1, p2) => p2.Avgrating.CompareTo(p1.Avgrating));
                team.topPlayerRatings = team.topAvgPlayerStats.GetRange(0, 3);

                // Sort avgPlayerStats by goals and save it
                team.topSumPlayerStats.Sort((p1, p2) => p2.Sumgoals.CompareTo(p1.Sumgoals));
                team.topPlayerGoals = team.topSumPlayerStats.GetRange(0, 3);

                // Sort avgPlayerStats by assists and save it
                team.topSumPlayerStats.Sort((p1, p2) => p2.Sumassists.CompareTo(p1.Sumassists));
                team.topPlayerAssists = team.topSumPlayerStats.GetRange(0, 3);
            }
        }

        private Avg_TeamStatisticsInfoRecord GetAvgStats(int id, int limit)
        {
            var urlTeamAvg = $"http://{baseUrl}/api/teamstats/avg?tid={id}&limit={limit}";
            var requestTeamAvg = WebRequest.Create(urlTeamAvg);

            // Get the average team stats
            using var webResponseTeamAvg = requestTeamAvg.GetResponse();
            using var webStreamTeamAvg = webResponseTeamAvg.GetResponseStream();

            using var readerTeamAvg = new StreamReader(webStreamTeamAvg);
            string gameDataTeamAvg = readerTeamAvg.ReadToEnd();
            var jsonTeamAvg = JObject.Parse(gameDataTeamAvg);

            Avg_TeamStatisticsInfoRecord avgTeamStats = new Avg_TeamStatisticsInfoRecord()
            {
                TID = int.Parse(jsonTeamAvg["tid"].ToString()),
                Teamname = jsonTeamAvg["teamname"].ToString(),
                Nummatches = double.Parse(jsonTeamAvg["nummatches"].ToString()),
                Wins = int.Parse(jsonTeamAvg["wins"].ToString()),
                Losses = int.Parse(jsonTeamAvg["losses"].ToString()),
                Draws = int.Parse(jsonTeamAvg["draws"].ToString()),
                Avgscored = double.Parse(jsonTeamAvg["avgscored"].ToString()),
                Avgconceded = double.Parse(jsonTeamAvg["avgconceded"].ToString()),
                Avgshots = double.Parse(jsonTeamAvg["avgshots"].ToString()),
                Avgshots_on_goal = double.Parse(jsonTeamAvg["avgshots_on_goal"].ToString()),
                Avgshots_off_goal = double.Parse(jsonTeamAvg["avgshots_off_goal"].ToString()),
                Avgshots_inside = double.Parse(jsonTeamAvg["avgshots_inside"].ToString()),
                Avgshots_outside = double.Parse(jsonTeamAvg["avgshots_outside"].ToString()),
                Avgblocked = double.Parse(jsonTeamAvg["avgblocked"].ToString()),
                Avgfouls = double.Parse(jsonTeamAvg["avgfouls"].ToString()),
                Avgcorners = double.Parse(jsonTeamAvg["avgcorners"].ToString()),
                Avgoffsides = double.Parse(jsonTeamAvg["avgoffsides"].ToString()),
                Avgpossession = double.Parse(jsonTeamAvg["avgpossession"].ToString()),
                Avgyellows = double.Parse(jsonTeamAvg["avgyellows"].ToString()),
                Avgreds = double.Parse(jsonTeamAvg["avgreds"].ToString()),
                Avgsaves = double.Parse(jsonTeamAvg["avgsaves"].ToString()),
                Avgpasses = double.Parse(jsonTeamAvg["avgpasses"].ToString()),
                Avgpasses_accurate = double.Parse(jsonTeamAvg["avgpasses_accurate"].ToString()),
                Avgpass_pct = double.Parse(jsonTeamAvg["avgpass_pct"].ToString()),
            };

            return avgTeamStats;
        }

        private List<Team_StatisticsInfoRecord> GetPreviousMatchesStats(int id, int limit)
        {
            var urlStats = $"http://{baseUrl}/api/teamstats/team?tid={id}&limit={limit}";
            var requestStats = WebRequest.Create(urlStats);

            // Get the stats of the team
            using var webResponseStats = requestStats.GetResponse();
            using var webStreamStats = webResponseStats.GetResponseStream();

            using var readerStats = new StreamReader(webStreamStats);
            string gameDataStats = readerStats.ReadToEnd();
            var jsonStats = JArray.Parse(gameDataStats);

            List<Team_StatisticsInfoRecord> matches = new List<Team_StatisticsInfoRecord>();
            // Get data for previous matches
            foreach (var match in jsonStats)
            {
                Team_StatisticsInfoRecord teamStats = new Team_StatisticsInfoRecord()
                {
                    Teamname = match["teamname"].ToString(),
                    Competition = match["competition"].ToString(),
                    Result = match["result"].ToString(),
                    Datetime = DateTime.Parse(DateTime.Parse(match["datetime"].ToString()).ToLocalTime().ToShortDateString()),
                    Opponentid = int.Parse(match["opponentid"].ToString()),
                    Opponentname = match["opponentname"].ToString(),
                    Scored = int.Parse(match["scored"].ToString()),
                    Conceded = int.Parse(match["conceded"].ToString()),
                    Pass_pct = double.Parse(match["pass_pct"].ToString()),
                    MID = int.Parse(match["mid"].ToString()),
                    TID = int.Parse(match["tid"].ToString()),
                    Shots = int.Parse(match["shots"].ToString()),
                    Shots_on_goal = int.Parse(match["shots_on_goal"].ToString()),
                    Shots_off_goal = int.Parse(match["shots_off_goal"].ToString()),
                    Shots_inside = int.Parse(match["shots_inside"].ToString()),
                    Shots_outside = int.Parse(match["shots_outside"].ToString()),
                    Blocked = int.Parse(match["blocked"].ToString()),
                    Fouls = int.Parse(match["fouls"].ToString()),
                    Corners = int.Parse(match["corners"].ToString()),
                    Offsides = int.Parse(match["offsides"].ToString()),
                    Possession = int.Parse(match["possession"].ToString()),
                    Yellows = int.Parse(match["yellows"].ToString()),
                    Reds = int.Parse(match["reds"].ToString()),
                    Saves = int.Parse(match["saves"].ToString()),
                    Passes = int.Parse(match["passes"].ToString()),
                    Passes_accurate = int.Parse(match["passes_accurate"].ToString()),
                };
                matches.Add(teamStats);
            }

            return matches;
        }

        private List<Player> GetTeamRoster(int id)
        {
            var urlPlayers = $"http://{baseUrl}/api/players/team?id={id}";
            var requestPlayers = WebRequest.Create(urlPlayers);

            // Get the players on the team
            using var webResponsePlayers = requestPlayers.GetResponse();
            using var webStreamPlayers = webResponsePlayers.GetResponseStream();

            using var readerPlayers = new StreamReader(webStreamPlayers);
            string gameDataPlayers = readerPlayers.ReadToEnd();
            var jsonPlayers = JArray.Parse(gameDataPlayers);

            // Get the data for roster/avg player stats
            List<Player> players = new List<Player>();
            foreach (var item in jsonPlayers)
            {
                players.Add(new Player
                {
                    Teamname = item["teamName"].ToString(),
                    TeamNation = item["teamNation"].ToString(),
                    Badge = item["badge"].ToString(),
                    PID = int.Parse(item["apiID"].ToString()),
                    TID = int.Parse(item["tid"].ToString()),
                    First = item["first"].ToString(),
                    Last = item["last"].ToString(),
                    Nickname = item["nickname"].ToString(),
                    DOB = item["dob"].ToString(),
                    Height = int.Parse(item["height"].ToString()),
                    Weight = int.Parse(item["weight"].ToString()),
                    Nationality = item["nationality"].ToString(),
                    Headshot = item["headshot"].ToString(),
                });
            }

            return players;
        }

        [HttpPost]
        public void AddFavoriteTeam(int TID)
        {
            var session = Request.Cookies["StatisGoatSession"];

            var url = $"http://{baseUrl}/api/accounts/addfavteam";//?session={session}&teamId={TID}";
            var request = WebRequest.Create(url);

            var postData = "session=" + session + "&teamId=" + TID;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var webResponse = request.GetResponse();
        }

        [HttpPost]
        public void RemoveFavoriteTeam(int TID)
        {
            var session = Request.Cookies["StatisGoatSession"];

            var url = $"http://{baseUrl}/api/accounts/removefavteam";//?session={session}&teamId={TID}";
            var request = WebRequest.Create(url);
        
            var postData = "session=" + session + "&teamId=" + TID;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var webResponse = request.GetResponse();
        }
    }
}
