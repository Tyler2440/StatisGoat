using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StatisGoat.Chats;
using StatisGoat.Events;
using StatisGoat.Lineups;
using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using StatisGoat.Team_Statistics;
using StatisGoat.WebApp.Models;
using StatisGoat.xPlayer_Statistics;
using StatisGoat.xTeam_Statistics;
using StatisGoat.Authentication;

namespace StatisGoat.WebApp.Controllers
{
    [AllowAnonymous]
    public class MatchController : Controller
    {
        private string baseUrl;
        private int MatchID;
        private Match match;

        public MatchController(IWebHostEnvironment env)
        {
            match = new Match();
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

        // GET: Matches
        public IActionResult Index()
        {
            return View();
        }

        // GET: Matches/Details    
        public IActionResult Details(int id)
        {
            try
            {
                var url = $"http://{baseUrl}/api/matches/match?mid=" + id;

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string matchData = reader.ReadToEnd();

                JToken obj = JObject.Parse(matchData);

                match.homePlayers = new List<Player>();
                match.awayPlayers = new List<Player>();
                match.homeScore = obj["status"].ToString() == "NS" ? "0" : obj["result"].ToString().Split('-')[0];
                match.awayScore = obj["status"].ToString() == "NS" ? "0" : obj["result"].ToString().Split('-')[1];

                MatchesInfoRecord matchInfoRecord = new MatchesInfoRecord()
                {
                    MatchId = int.Parse(obj["matchId"].ToString()),
                    Competition = obj["competition"].ToString(),
                    DateTime = DateTime.Parse(obj["dateTime"].ToString()).ToUniversalTime(),
                    Venue = obj["venue"].ToString(),
                    HomeId = int.Parse(obj["homeId"].ToString()),
                    HomeName = obj["homeName"].ToString(),
                    HomeNation = obj["homeNation"].ToString(),
                    HomeFormation = obj["homeFormation"] is null ? "N/A" : obj["homeFormation"].ToString(),
                    AwayId = int.Parse(obj["awayId"].ToString()),
                    AwayName = obj["awayName"].ToString(),
                    AwayNation = obj["awayNation"].ToString(),
                    AwayFormation = obj["awayFormation"] is null ? "N/A" : obj["homeFormation"].ToString(),                    
                    Status = obj["status"].ToString(),
                    Result = obj["result"].ToString(),
                    Elapsed = int.Parse(obj["elapsed"].ToString()),
                    HomeBadge = obj["homeBadge"].ToString(),
                    AwayBadge = obj["awayBadge"].ToString(),
                };

                match.matchesInfoRecord = matchInfoRecord;

                match.homePlayers = GetHomePlayers(matchInfoRecord.MatchId, matchInfoRecord.HomeId);
                match.awayPlayers = GetAwayPlayers(matchInfoRecord.MatchId, matchInfoRecord.AwayId);
                
                match.homePreviousMatchesInfoRecords = GetPreviousMatches(matchInfoRecord.HomeId);
                match.awayPreviousMatchesInfoRecords = GetPreviousMatches(matchInfoRecord.AwayId);

                match.teamsPreviousMatchesInfoRecords = GetTeamsPreviousMatches(matchInfoRecord.HomeId, matchInfoRecord.AwayId);
                
                match.xHomeStatistics = GetxHomeStatistics(matchInfoRecord.MatchId, matchInfoRecord.HomeId);
                match.xAwayStatistics = GetxAwayStatistics(matchInfoRecord.MatchId, matchInfoRecord.AwayId);

                match.homeLineups = GetHomeLineup(matchInfoRecord.MatchId, matchInfoRecord.HomeId);
                match.awayLineups = GetAwayLineup(matchInfoRecord.MatchId, matchInfoRecord.AwayId);
                GetXPlayerStatistics(matchInfoRecord.MatchId);
                if (match.matchesInfoRecord.Status != "NS")
                {

                    match.eventsInfoRecords = GetMatchEvents(matchInfoRecord.MatchId);

                    match.homeLineups = UpdateHomeSubs(match.homeLineups, match.homeLineups);
                    match.awayLineups = UpdateAwaySubs(match.awayLineups, match.awayLineups);

                    match.homeStatistics = GetHomeStatistics(matchInfoRecord.MatchId, matchInfoRecord.HomeId);
                    match.awayStatistics = GetAwayStatistics(matchInfoRecord.MatchId, matchInfoRecord.AwayId);                                      
                    
                    match.allPlayerStatistics = GetPlayerStatistics(matchInfoRecord.MatchId);
                    

                    GetSortedPlayerStatistics();

                    MatchID = match.matchesInfoRecord.MatchId;
                    //GetXPlayerStatistics(MatchID);
                }

                match.comments = GetComments(matchInfoRecord.MatchId);

                return View(match);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        private List<LineupsInfoRecord> UpdateHomeSubs(List<LineupsInfoRecord> homePlayers, List<LineupsInfoRecord> homeLineup)
        {
            foreach (LineupsInfoRecord p in homePlayers)
            {
                if (p.Grid == "")
                {
                    LineupsInfoRecord player = homeLineup.Find(player => p.PlayerID == player.PlayerID);

                    if (player != null)
                    {
                        homeLineup.Find(player => p.PlayerID == player.PlayerID).Position = "Sub";
                    }
                }
            }

            homeLineup.Sort((p1, p2) => p1.Position.Length.CompareTo(p2.Position.Length));

            return homeLineup;
        }

        private List<LineupsInfoRecord> UpdateAwaySubs(List<LineupsInfoRecord> awayPlayers, List<LineupsInfoRecord> awayLineup)
        {
            foreach (LineupsInfoRecord p in awayPlayers)
            {
                if (p.Grid == "")
                {
                    LineupsInfoRecord player = awayLineup.Find(player => p.PlayerID == player.PlayerID);

                    if (player != null)
                    {
                        awayLineup.Find(player => p.PlayerID == player.PlayerID).Position = "Sub";
                    }
                }
            }

            awayLineup.Sort((p1, p2) => p1.Position.Length.CompareTo(p2.Position.Length));

            return awayLineup;
        }

        private List<MatchesInfoRecord> GetTeamsPreviousMatches(int homeId, int awayId)
        {
            int limit = 5;
            var url = $"http://{baseUrl}/api/matches/teams?id1={homeId}&id2={awayId}";
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


            return previousMatchInfoRecords.Take(limit).ToList();
        }

        private xTeam_StatisticsInfoRecord GetxAwayStatistics(int matchId, int awayId)
        {
            var url = $"http://{baseUrl}/api/xteam/match?mid={matchId}&tid={awayId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JObject json = JObject.Parse(stream);

            xTeam_StatisticsInfoRecord team_xStatisticsInfoRecord = new xTeam_StatisticsInfoRecord
            {
                TID = int.Parse(json["tid"].ToString()),
                Teamname = json["teamname"].ToString(),
                MID = int.Parse(json["mid"].ToString()),
                OpponentID = int.Parse(json["opponentID"].ToString()),
                Opponentname = json["opponentname"].ToString(),
                Competition = json["competition"].ToString(),
                Datetime = DateTime.Parse(json["datetime"].ToString()),
                Status = json["status"].ToString(),
                Teamscored = int.Parse(json["teamscored"].ToString()),
                Result = json["result"].ToString(),
                xRating = double.Parse(json["xRating"].ToString()),
                Rating_perf = double.Parse(json["rating_perf"].ToString()),
                xShots = double.Parse(json["xShots"].ToString()),
                Shots_perf = double.Parse(json["shots_perf"].ToString()),
                xGoals = double.Parse(json["xGoals"].ToString()),
                Goals_perf = double.Parse(json["goals_perf"].ToString()),
                xAssists = double.Parse(json["xAssists"].ToString()),
                Assists_perf = double.Parse(json["assists_perf"].ToString()),
                xSaves = double.Parse(json["xSaves"].ToString()),
                Saves_perf = double.Parse(json["saves_perf"].ToString()),
                xPasses = double.Parse(json["xPasses"].ToString()),
                Passes_perf = double.Parse(json["passes_perf"].ToString()),
                xTackles = double.Parse(json["xTackles"].ToString()),
                Tackles_perf = double.Parse(json["tackles_perf"].ToString()),
                xInterceptions = double.Parse(json["xInterceptions"].ToString()),
                Interceptions_perf = double.Parse(json["interceptions_perf"].ToString()),
                xDribbles = double.Parse(json["xDribbles"].ToString()),
                Dribbles_perf = double.Parse(json["dribbles_perf"].ToString()),
                xFouls = double.Parse(json["xFouls"].ToString()),
                Fouls_perf = double.Parse(json["fouls_perf"].ToString()),
                xYellow = double.Parse(json["xYellow"].ToString()),
                Yellow_perf = double.Parse(json["yellow_perf"].ToString()),
                xRed = double.Parse(json["xRed"].ToString()),
                Red_perf = double.Parse(json["red_perf"].ToString()),
            };
            return team_xStatisticsInfoRecord;
        }


        private xTeam_StatisticsInfoRecord GetxHomeStatistics(int matchId, int homeId)
        {
            var url = $"http://{baseUrl}/api/xteam/match?mid={matchId}&tid={homeId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JObject json = JObject.Parse(stream);

            xTeam_StatisticsInfoRecord team_xStatisticsInfoRecord = new xTeam_StatisticsInfoRecord
            {
                TID = int.Parse(json["tid"].ToString()),
                Teamname = json["teamname"].ToString(),
                MID = int.Parse(json["mid"].ToString()),
                OpponentID = int.Parse(json["opponentID"].ToString()),
                Opponentname = json["opponentname"].ToString(),
                Competition = json["competition"].ToString(),                
                Datetime = DateTime.Parse(json["datetime"].ToString()),
                Status = json["status"].ToString(),               
                Teamscored = int.Parse(json["teamscored"].ToString()),
                Result = json["result"].ToString(),
                xRating = double.Parse(json["xRating"].ToString()),
                Rating_perf = double.Parse(json["rating_perf"].ToString()),
                xShots = double.Parse(json["xShots"].ToString()),
                Shots_perf = double.Parse(json["shots_perf"].ToString()),
                xGoals = double.Parse(json["xGoals"].ToString()),
                Goals_perf = double.Parse(json["goals_perf"].ToString()),
                xAssists = double.Parse(json["xAssists"].ToString()),
                Assists_perf = double.Parse(json["assists_perf"].ToString()),
                xSaves = double.Parse(json["xSaves"].ToString()),
                Saves_perf = double.Parse(json["saves_perf"].ToString()),
                xPasses = double.Parse(json["xPasses"].ToString()),
                Passes_perf = double.Parse(json["passes_perf"].ToString()),
                xTackles = double.Parse(json["xTackles"].ToString()),
                Tackles_perf = double.Parse(json["tackles_perf"].ToString()),
                xInterceptions = double.Parse(json["xInterceptions"].ToString()),
                Interceptions_perf = double.Parse(json["interceptions_perf"].ToString()),
                xDribbles = double.Parse(json["xDribbles"].ToString()),
                Dribbles_perf = double.Parse(json["dribbles_perf"].ToString()),
                xFouls = double.Parse(json["xFouls"].ToString()),
                Fouls_perf = double.Parse(json["fouls_perf"].ToString()),
                xYellow = double.Parse(json["xYellow"].ToString()),
                Yellow_perf = double.Parse(json["yellow_perf"].ToString()),
                xRed = double.Parse(json["xRed"].ToString()),
                Red_perf = double.Parse(json["red_perf"].ToString()),
            };
            return team_xStatisticsInfoRecord;
        }

        private List<LineupsInfoRecord> GetAwayLineup(int matchId, int awayId)
        {
            var url = $"http://{baseUrl}/api/lineups/match?mid={matchId}&tid={awayId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JArray json = JArray.Parse(stream);

            List<LineupsInfoRecord> lineup = new List<LineupsInfoRecord>();

            foreach (var l in json)
            {
                lineup.Add(new LineupsInfoRecord
                {
                    MatchID = int.Parse(l["matchID"].ToString()),
                    TeamID = int.Parse(l["teamID"].ToString()),
                    TeamName = l["teamName"].ToString(),
                    PlayerID = int.Parse(l["playerID"].ToString()),
                    FirstName = l["firstName"].ToString(),
                    LastName = l["lastName"].ToString(),
                    Number = int.Parse(l["number"].ToString()),
                    Nickname = l["nickname"].ToString(),
                    DOB = DateTime.Parse(l["dob"].ToString()),
                    Height = int.Parse(l["height"].ToString()),
                    Weight = int.Parse(l["weight"].ToString()),
                    Nationality = l["nationality"].ToString(),
                    Headshot = l["headshot"].ToString(),
                    Position = l["position"].ToString(),
                    Grid = l["grid"].ToString(),
                });
            }
            return lineup;
        }

        private List<LineupsInfoRecord> GetHomeLineup(int matchId, int homeId)
        {
            var url = $"http://{baseUrl}/api/lineups/match?mid={matchId}&tid={homeId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JArray json = JArray.Parse(stream);

            List<LineupsInfoRecord> lineup = new List<LineupsInfoRecord>();

            foreach (var l in json)
            {
                lineup.Add(new LineupsInfoRecord
                {
                    MatchID = int.Parse(l["matchID"].ToString()),
                    TeamID = int.Parse(l["teamID"].ToString()),
                    TeamName = l["teamName"].ToString(),
                    PlayerID = int.Parse(l["playerID"].ToString()),
                    FirstName = l["firstName"].ToString(),
                    Number = int.Parse(l["number"].ToString()),
                    LastName = l["lastName"].ToString(),
                    Nickname = l["nickname"].ToString(),
                    DOB = DateTime.Parse(l["dob"].ToString()),
                    Height = int.Parse(l["height"].ToString()),
                    Weight = int.Parse(l["weight"].ToString()),
                    Nationality = l["nationality"].ToString(),
                    Headshot = l["headshot"].ToString(),
                    Position = l["position"].ToString(),
                    Grid = l["grid"].ToString(),
                });
            }
            return lineup;
        }

        private Team_StatisticsInfoRecord GetAwayStatistics(int matchId, int awayId)
        {
            var url = $"http://{baseUrl}/api/teamstats/match?mid={matchId}&tid={awayId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JObject json = JObject.Parse(stream);

            Team_StatisticsInfoRecord team_StatisticsInfoRecord = new Team_StatisticsInfoRecord
            {
                Teamname = json["teamname"].ToString(),
                Competition = json["competition"].ToString(),
                Result = json["result"].ToString(),
                Datetime = DateTime.Parse(json["datetime"].ToString()),
                Opponentid = int.Parse(json["opponentid"].ToString()),
                Opponentname = json["opponentname"].ToString(),
                Scored = int.Parse(json["scored"].ToString()),
                Conceded = int.Parse(json["conceded"].ToString()),
                Pass_pct = double.Parse(json["pass_pct"].ToString()),
                MID = int.Parse(json["mid"].ToString()),
                TID = int.Parse(json["tid"].ToString()),
                Shots = int.Parse(json["shots"].ToString()),
                Shots_on_goal = int.Parse(json["shots_on_goal"].ToString()),
                Shots_off_goal = int.Parse(json["shots_off_goal"].ToString()),
                Shots_inside = int.Parse(json["shots_inside"].ToString()),
                Shots_outside = int.Parse(json["shots_outside"].ToString()),
                Blocked = int.Parse(json["blocked"].ToString()),
                Fouls = int.Parse(json["fouls"].ToString()),
                Corners = int.Parse(json["corners"].ToString()),
                Offsides = int.Parse(json["offsides"].ToString()),
                Possession = int.Parse(json["possession"].ToString()),
                Yellows = int.Parse(json["yellows"].ToString()),
                Reds = int.Parse(json["reds"].ToString()),
                Saves = int.Parse(json["saves"].ToString()),
                Passes = int.Parse(json["passes"].ToString()),
                Passes_accurate = int.Parse(json["passes_accurate"].ToString()),
            };
            return team_StatisticsInfoRecord;
        }

        private Team_StatisticsInfoRecord GetHomeStatistics(int matchId, int homeId)
        {
            var url = $"http://{baseUrl}/api/teamstats/match?mid={matchId}&tid={homeId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JObject json = JObject.Parse(stream);

            Team_StatisticsInfoRecord team_StatisticsInfoRecord = new Team_StatisticsInfoRecord
            {
                Teamname = json["teamname"].ToString(),
                Competition = json["competition"].ToString(),
                Result = json["result"].ToString(),
                Datetime = DateTime.Parse(json["datetime"].ToString()),
                Opponentid = int.Parse(json["opponentid"].ToString()),
                Opponentname = json["opponentname"].ToString(),
                Scored = int.Parse(json["scored"].ToString()),
                Conceded = int.Parse(json["conceded"].ToString()),
                Pass_pct = double.Parse(json["pass_pct"].ToString()),
                MID = int.Parse(json["mid"].ToString()),
                TID = int.Parse(json["tid"].ToString()),
                Shots = int.Parse(json["shots"].ToString()),
                Shots_on_goal = int.Parse(json["shots_on_goal"].ToString()),
                Shots_off_goal = int.Parse(json["shots_off_goal"].ToString()),
                Shots_inside = int.Parse(json["shots_inside"].ToString()),
                Shots_outside = int.Parse(json["shots_outside"].ToString()),
                Blocked = int.Parse(json["blocked"].ToString()),
                Fouls = int.Parse(json["fouls"].ToString()),
                Corners = int.Parse(json["corners"].ToString()),
                Offsides = int.Parse(json["offsides"].ToString()),
                Possession = int.Parse(json["possession"].ToString()),
                Yellows = int.Parse(json["yellows"].ToString()),
                Reds = int.Parse(json["reds"].ToString()),
                Saves = int.Parse(json["saves"].ToString()),
                Passes = int.Parse(json["passes"].ToString()),
                Passes_accurate = int.Parse(json["passes_accurate"].ToString()),
            };
            return team_StatisticsInfoRecord;
        }

        private void GetSortedPlayerStatistics()
        {
            // Rating
            match.sortedTopRatings = match.allPlayerStatistics;
            match.sortedTopRatings.Sort((x, y) => y.Rating.CompareTo(x.Rating));
            match.sortedTopRatings = match.sortedTopRatings.Take(3).ToList();

            // Goals
            match.sortedTopGoals = match.allPlayerStatistics;
            match.sortedTopGoals.Sort((x, y) => y.Goals.CompareTo(x.Goals));
            match.sortedTopGoals = match.sortedTopGoals.Take(3).ToList();

            // Assists
            match.sortedTopAssists = match.allPlayerStatistics;
            match.sortedTopAssists.Sort((x, y) => y.Assists.CompareTo(x.Assists));
            match.sortedTopAssists = match.sortedTopAssists.Take(3).ToList();

            // Goals + Assists
            match.sortedTopGoalsAssists = match.allPlayerStatistics;
            match.sortedTopGoalsAssists.Sort((x, y) => (y.Goals + y.Assists).CompareTo(x.Goals + x.Assists));
            match.sortedTopGoalsAssists = match.sortedTopGoalsAssists.Take(3).ToList();

            // Saves
            match.sortedTopSaves = match.allPlayerStatistics;
            match.sortedTopSaves.Sort((x, y) => y.Saves.CompareTo(x.Saves));
            match.sortedTopSaves = match.sortedTopSaves.Take(3).ToList();

            // Passes
            match.sortedTopPasses = match.allPlayerStatistics;
            match.sortedTopPasses.Sort((x, y) => y.Passes.CompareTo(x.Passes));
            match.sortedTopPasses = match.sortedTopPasses.Take(3).ToList();

            // Key Passes
            match.sortedTopKeyPasses = match.allPlayerStatistics;
            match.sortedTopKeyPasses.Sort((x, y) => y.Key_passes.CompareTo(x.Key_passes));
            match.sortedTopKeyPasses = match.sortedTopKeyPasses.Take(3).ToList();

            // Pass %
            match.sortedTopPassPct = match.allPlayerStatistics;
            match.sortedTopPassPct.Sort((x, y) => y.Pass_pct.CompareTo(x.Pass_pct));
            match.sortedTopPassPct = match.sortedTopPassPct.Take(3).ToList();

            // Tackles
            match.sortedTopTackles = match.allPlayerStatistics;
            match.sortedTopTackles.Sort((x, y) => y.Tackles.CompareTo(x.Tackles));
            match.sortedTopTackles = match.sortedTopTackles.Take(3).ToList();

            // Interceptions
            match.sortedTopInterceptions = match.allPlayerStatistics;
            match.sortedTopInterceptions.Sort((x, y) => y.Interceptions.CompareTo(x.Interceptions));
            match.sortedTopInterceptions = match.sortedTopInterceptions.Take(3).ToList();

            // Dribbles
            match.sortedTopDribbles = match.allPlayerStatistics;
            match.sortedTopDribbles.Sort((x, y) => y.Dribbles.CompareTo(x.Dribbles));
            match.sortedTopDribbles = match.sortedTopDribbles.Take(3).ToList();

            // Fouls
            match.sortedTopFouls = match.allPlayerStatistics;
            match.sortedTopFouls.Sort((x, y) => y.Fouls_committed.CompareTo(x.Fouls_committed));
            match.sortedTopFouls = match.sortedTopFouls.Take(3).ToList();
        }

        private void GetXPlayerStatistics(int matchId)
        {
            var url = $"http://{baseUrl}/api/xplayer/match?mid={matchId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JArray json = JArray.Parse(stream);

            List<xPlayer_StatisticsInfoRecord> xplayers = new List<xPlayer_StatisticsInfoRecord>();
            foreach (var p in json)
            {
                xplayers.Add(new xPlayer_StatisticsInfoRecord
                {
                    PID = int.Parse(p["pid"].ToString()),
                    First = p["first"].ToString(),
                    Last = p["last"].ToString(),
                    Nickname = p["nickname"].ToString(),
                    Headshot = p["headshot"].ToString(),
                    TID = int.Parse(p["tid"].ToString()),
                    Teamname = p["teamname"].ToString(),
                    MID = int.Parse(p["mid"].ToString()),
                    Opponentname = p["opponentname"].ToString(),
                    Competition = p["competition"].ToString(),
                    Datetime = DateTime.Parse(p["datetime"].ToString()),
                    Status = p["status"].ToString(),
                    Teamscored = int.Parse(p["teamscored"].ToString()),
                    Result = p["result"].ToString(),
                    xRating = double.Parse(p["xRating"].ToString()),
                    Rating_perf = double.Parse(p["rating_perf"].ToString()),
                    xShots = double.Parse(p["xShots"].ToString()),
                    Shots_perf = double.Parse(p["shots_perf"].ToString()),
                    xGoals = double.Parse(p["xGoals"].ToString()),
                    Goals_perf = double.Parse(p["goals_perf"].ToString()),
                    xAssists = double.Parse(p["xAssists"].ToString()),
                    Assists_perf = double.Parse(p["assists_perf"].ToString()),
                    xSaves = double.Parse(p["xSaves"].ToString()),
                    Saves_perf = double.Parse(p["saves_perf"].ToString()),
                    xPasses = double.Parse(p["xPasses"].ToString()),
                    Passes_perf = double.Parse(p["passes_perf"].ToString()),
                    xTackles = double.Parse(p["xTackles"].ToString()),
                    Tackles_perf = double.Parse(p["tackles_perf"].ToString()),
                    xInterceptions = double.Parse(p["xInterceptions"].ToString()),
                    Interceptions_perf = double.Parse(p["interceptions_perf"].ToString()),
                    xDribbles = double.Parse(p["xDribbles"].ToString()),
                    Dribbles_perf = double.Parse(p["dribbles_perf"].ToString()),
                    xFouls = double.Parse(p["xFouls"].ToString()),
                    Fouls_perf = double.Parse(p["fouls_perf"].ToString()),
                    xYellow = double.Parse(p["xYellow"].ToString()),
                    Yellow_perf = double.Parse(p["yellow_perf"].ToString()),
                    xRed = double.Parse(p["xRed"].ToString()),
                    Red_perf = double.Parse(p["red_perf"].ToString()),
                });
            }

            match.allxPlayerStatistics = xplayers;

            // Get the sorted top xratings, xgoals, xassists, xgoals+xassists
            match.sortedxTopRatings = match.allxPlayerStatistics;
            match.sortedxTopRatings.Sort((x, y) => y.xRating.CompareTo(x.xRating));
            match.sortedxTopRatings = match.sortedxTopRatings.Take(3).ToList();

            // xGoals
            match.sortedxTopGoals = match.allxPlayerStatistics;
            match.sortedxTopGoals.Sort((x, y) => y.xGoals.CompareTo(x.xGoals));
            match.sortedxTopGoals = match.sortedxTopGoals.Take(3).ToList();

            // xAssists
            match.sortedxTopAssists = match.allxPlayerStatistics;
            match.sortedxTopAssists.Sort((x, y) => y.xAssists.CompareTo(x.xAssists));
            match.sortedxTopAssists = match.sortedxTopAssists.Take(3).ToList();

            // xGoals + xAssists
            match.sortedxTopGoalsAssists = match.allxPlayerStatistics;
            match.sortedxTopGoalsAssists.Sort((x, y) => (y.xGoals + y.xAssists).CompareTo(x.xGoals + x.xAssists));
            match.sortedxTopGoalsAssists = match.sortedxTopGoalsAssists.Take(3).ToList();

            // xSaves
            match.sortedxTopSaves = match.allxPlayerStatistics;
            match.sortedxTopSaves.Sort((x, y) => y.xSaves.CompareTo(x.xSaves));
            match.sortedxTopSaves = match.sortedxTopSaves.Take(3).ToList();

            // xPasses
            match.sortedxTopPasses = match.allxPlayerStatistics;
            match.sortedxTopPasses.Sort((x, y) => y.xPasses.CompareTo(x.xPasses));
            match.sortedxTopPasses = match.sortedxTopPasses.Take(3).ToList();

            // xTackles
            match.sortedxTopTackles = match.allxPlayerStatistics;
            match.sortedxTopTackles.Sort((x, y) => y.xTackles.CompareTo(x.xTackles));
            match.sortedxTopTackles = match.sortedxTopTackles.Take(3).ToList();

            // xInterceptions
            match.sortedxTopInterceptions = match.allxPlayerStatistics;
            match.sortedxTopInterceptions.Sort((x, y) => y.xInterceptions.CompareTo(x.xInterceptions));
            match.sortedxTopInterceptions = match.sortedxTopInterceptions.Take(3).ToList();

            // xDribbles
            match.sortedxTopDribbles = match.allxPlayerStatistics;
            match.sortedxTopDribbles.Sort((x, y) => y.xDribbles.CompareTo(x.xDribbles));
            match.sortedxTopDribbles = match.sortedxTopDribbles.Take(3).ToList();

            // xFouls
            match.sortedxTopFouls = match.allxPlayerStatistics;
            match.sortedxTopFouls.Sort((x, y) => y.xFouls.CompareTo(x.xFouls));
            match.sortedxTopFouls = match.sortedxTopFouls.Take(3).ToList();
        }

        private List<MatchesInfoRecord> GetPreviousMatches(int teamId)
        {
            int limit = 5;
            string date = match.matchesInfoRecord.DateTime.ToString("MM/dd/yyyy");
            var url = $"http://{baseUrl}/api/matches/team?id={teamId}&limit={limit}&date=\"{date}\"";
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


            return previousMatchInfoRecords.Take(limit).ToList();
        }

        private List<EventsInfoRecord> GetMatchEvents(int matchId)
        {
            var url = $"http://{baseUrl}/api/events/match?mid={matchId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JArray json = JArray.Parse(eventReader);

            List<EventsInfoRecord> events = new List<EventsInfoRecord>();

            foreach (var e in json)
            {
                events.Add(new EventsInfoRecord
                {
                    MID = matchId,
                    Minute = int.Parse(e["minute"].ToString()),
                    Type = e["type"].ToString(),
                    Detail = e["detail"].ToString(),
                    Comment = e["comment"].ToString(),
                    TeamID = int.Parse(e["teamID"].ToString()),
                    TeamName = e["teamName"].ToString(),
                    Badge = e["badge"].ToString(),
                    PlayerID = int.Parse(e["playerID"].ToString()),
                    First = e["first"].ToString(),
                    Last = e["last"].ToString(),
                    Headshot = e["headshot"].ToString(),
                    AssistID = int.Parse(e["assistID"].ToString()),
                    AssistFirst = e["assistFirst"].ToString(),
                    AssistLast = e["assistLast"].ToString(),
                    AssistHeadshot = e["assistHeadshot"].ToString()
                });
            }

            return events;
        }

        public List<Player_StatisticsInfoRecord> GetPlayerStatistics(int matchId)
        {
            var url = $"http://{baseUrl}/api/playerstats/match?mid={matchId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            string stream = reader.ReadToEnd();
            JArray json = JArray.Parse(stream);

            List<Player_StatisticsInfoRecord> players = new List<Player_StatisticsInfoRecord>();
            foreach (var p in json)
            {
                players.Add(new Player_StatisticsInfoRecord { 
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
                    Headshot = p["headshot"].ToString(),
                });
            }

            return players;
        }

        public List<Player> GetHomePlayers(int matchId, int homeId)
        {
            var url = $"http://{baseUrl}/api/lineups/match?mid={matchId}&tid={homeId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponseHome = request.GetResponse();
            using var webStreamHome = webResponseHome.GetResponseStream();

            using var readerHome = new StreamReader(webStreamHome);
            string home = readerHome.ReadToEnd();
            JArray jsonHome = JArray.Parse(home);

            List<Player> homePlayers = new List<Player>();
            foreach (var p in jsonHome)
            {
                homePlayers.Add(new Player
                {
                    PID = int.Parse(p["playerID"].ToString()),
                    First = p["firstName"].ToString(),
                    Last = p["lastName"].ToString(),
                    Nickname = p["nickname"].ToString(),
                    TID = int.Parse(p["teamID"].ToString()),
                    Headshot = p["headshot"].ToString(),

                });
            }

            return homePlayers;
        }

        public List<Player> GetAwayPlayers(int matchId, int awayId)
        {
            var url = $"http://{baseUrl}/api/lineups/match?mid={matchId}&tid={awayId}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponseAway = request.GetResponse();
            using var webStreamAway = webResponseAway.GetResponseStream();

            using var readerAway = new StreamReader(webStreamAway);
            string away = readerAway.ReadToEnd();
            JArray jsonAway = JArray.Parse(away);

            List<Player> awayPlayers = new List<Player>();
            foreach (var p in jsonAway)
            {
                awayPlayers.Add(new Player
                {
                    PID = int.Parse(p["playerID"].ToString()),
                    First = p["firstName"].ToString(),
                    Last = p["lastName"].ToString(),
                    Nickname = p["nickname"].ToString(),
                    TID = int.Parse(p["teamID"].ToString()),
                    Headshot = p["headshot"].ToString()
                });
            }
            return awayPlayers;
        }

        [HttpPost]
        public void PostNewComment(int mid, int accountid, string message, DateTime timeStamp, int threadid)
        {
            var session = Request.Cookies["StatisGoatSession"];
            //var account = await AuthenticationRepository.GetAccountBy
            //var aid = 3;
            using (var client = new WebClient())
            {
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var values = new NameValueCollection();                
                values["mid"] = mid.ToString();
                values["session"] = session;
                values["message"] = message;
                values["timestamp"] = timeStamp.ToString();
               

                if (threadid != 0)
                {
                    values["threadid"] = threadid.ToString();
                }

                var response = client.UploadValues($"http://{baseUrl}/api/chats/post", values);

                var responseString = Encoding.Default.GetString(response);
            }
        }

        [HttpPost]
        public void LikeComment(int chatID)
        {
            var session = Request.Cookies["StatisGoatSession"];
            //var account = await AuthenticationRepository.GetAccountBy
            //var aid = 3;
            using (var client = new WebClient())
            {
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var values = new NameValueCollection();
                values["chatid"] = chatID.ToString();
                values["session"] = session;


                var response = client.UploadValues($"http://{baseUrl}/api/chats/like", values);

                var responseString = Encoding.Default.GetString(response);
            }
        }
        [HttpPost]
        public void DislikeComment(int chatID)
        {
            var session = Request.Cookies["StatisGoatSession"];
            //var account = await AuthenticationRepository.GetAccountBy
            //var aid = 3;
            using (var client = new WebClient())
            {
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var values = new NameValueCollection();
                values["chatid"] = chatID.ToString();
                values["session"] = session;



                var response = client.UploadValues($"http://{baseUrl}/api/chats/dislike", values);

                var responseString = Encoding.Default.GetString(response);
            }
        }

        [HttpPost]
        public void UnlikeComment(int chatID)
        {
            var session = Request.Cookies["StatisGoatSession"];
            //var account = await AuthenticationRepository.GetAccountBy
            //var aid = 3;
            using (var client = new WebClient())
            {
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var values = new NameValueCollection();
                values["chatid"] = chatID.ToString();
                values["session"] = session;

                var response = client.UploadValues($"http://{baseUrl}/api/chats/unlike", values);

                var responseString = Encoding.Default.GetString(response);
            }
        }
        [HttpPost]
        public void UndislikeComment(int chatID)
        {
            var session = Request.Cookies["StatisGoatSession"];
            //var account = await AuthenticationRepository.GetAccountBy
            //var aid = 3;
            using (var client = new WebClient())
            {
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var values = new NameValueCollection();
                values["chatid"] = chatID.ToString();
                values["session"] = session;

            

            var response = client.UploadValues($"http://{baseUrl}/api/chats/undislike", values);

                var responseString = Encoding.Default.GetString(response);
            }
        }
        [HttpGet]
        public List<ChatInfoRecord> GetComments(int mid)
        {
            var json = new JArray();
            try
            {
                var session = Request.Cookies["StatisGoatSession"];
                var url = $"http://{baseUrl}/api/chats/match?mid=" + mid + "&session=" + session;

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string chatData = reader.ReadToEnd();
                json = JArray.Parse(chatData);
            }
            catch (Exception e)
            {
                var session = Request.Cookies["StatisGoatSession"];
                var url = $"http://{baseUrl}/api/chats/match?mid=" + mid;

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string chatData = reader.ReadToEnd();
                json = JArray.Parse(chatData);
            }
            try
            {
                List<ChatInfoRecord> comments = new List<ChatInfoRecord>();
                foreach (var item in json)
                {
                    ChatInfoRecord comment = new ChatInfoRecord()
                    {
                        First = item["first"].ToString(),
                        Last = item["last"].ToString(),
                        Email = item["email"].ToString(),
                        Likes = int.Parse(item["likes"].ToString()),
                        Dislikes = int.Parse(item["dislikes"].ToString()),
                        Liked = bool.Parse(item["liked"].ToString()),
                        Disliked = bool.Parse(item["disliked"].ToString()),
                        ChatID = int.Parse(item["chatID"].ToString()),
                        MatchID = int.Parse(item["matchID"].ToString()),
                        AccountID = int.Parse(item["accountID"].ToString()),
                        Message = item["message"].ToString(),
                        Timestamp = DateTime.Parse(item["timestamp"].ToString()),
                        ThreadID = null
                    };

                    if (int.TryParse(item["threadID"].ToString(), out var num))
                    {
                        comment.ThreadID = int.Parse(item["threadID"].ToString());
                    }

                    comments.Add(comment);
                }

                return comments;
            }
            catch (Exception e)
            {
                return new List<ChatInfoRecord>();
            }            
        }
    }
}

