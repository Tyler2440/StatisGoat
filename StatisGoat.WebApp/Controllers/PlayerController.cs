using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StatisGoat.WebApp.Models;
using StatisGoat.Player_Statistics;
using StatisGoat.xPlayer_Statistics;

namespace StatisGoat.WebApp.Controllers
{
    [AllowAnonymous]
    public class PlayerController : Controller
    {
        private string baseUrl;

        public PlayerController(IWebHostEnvironment env)
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

        // GET: Players
        public IActionResult Index()
        {
            try
            {
                var url = $"http://{baseUrl}/api/players";

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string gameData = reader.ReadToEnd();
                var json = JArray.Parse(gameData);

                DateTime today = DateTime.Today;
                List<Player> players = new List<Player>();
                foreach (var p in json)
                {
                    players.Add(new Player()
                    {
                        PID = int.Parse(p["apiID"].ToString()),
                        First = p["first"].ToString(),
                        Last = p["last"].ToString(),
                        Nickname = p["nickname"].ToString(),
                        Age = (int.Parse(DateTime.Today.ToString("yyyyMMdd")) - int.Parse(DateTime.Parse(p["dob"].ToString()).ToString("yyyyMMdd"))) / 10000,
                        DOB = DateTime.Parse(p["dob"].ToString()).ToShortDateString(),
                        Height = int.Parse(p["height"].ToString()),
                        Weight = int.Parse(p["weight"].ToString()),
                        Nationality = p["nationality"].ToString(),
                        Headshot = p["headshot"].ToString(),
                        Badge = p["badge"].ToString(),
                        Teamname = p["teamName"].ToString(),
                        TeamNation = p["teamNation"].ToString(),
                        TID = int.Parse(p["tid"].ToString()),
                    });
                }

                return View(players);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        // GET: Players/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var url = $"http://{baseUrl}/api/players/player?id=" + id;

                var request = WebRequest.Create(url);
                request.Method = "GET";

                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();

                using var reader = new StreamReader(webStream);
                string gameData = reader.ReadToEnd();
                var json = JObject.Parse(gameData);

                DateTime today = DateTime.Today;

                Player players = new Player
                {

                        PID = int.Parse(json["apiID"].ToString()),
                        Age = (int.Parse(DateTime.Today.ToString("yyyyMMdd")) - int.Parse(DateTime.Parse(json["dob"].ToString()).ToString("yyyyMMdd"))) / 10000,
                        First = json["first"].ToString(),
                        Last = json["last"].ToString(),
                        DOB = DateTime.Parse(json["dob"].ToString()).ToShortDateString(),
                        Height = int.Parse(json["height"].ToString()),
                        Weight = int.Parse(json["weight"].ToString()),
                        Nationality = json["nationality"].ToString(),
                        Headshot = json["headshot"].ToString(),
                        PreviousPlayerStats = getPreviousPlayerStats(id),
                        xPreviousPlayerStats = getXPreviousPlayerStats(id),
                        Badge = json["badge"].ToString(),
                        TID = int.Parse(json["tid"].ToString()),                  
                };

                return View(players);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error404", "Error");
            }
        }

        private List<Player_StatisticsInfoRecord> getPreviousPlayerStats(int id)
        {
            var url = $"http://{baseUrl}/api/playerstats/player?pid={id}&limit=10";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JArray json = JArray.Parse(eventReader);

            List<Player_StatisticsInfoRecord> previousPlayerStats = new List<Player_StatisticsInfoRecord>();

            foreach (var d in json)
            {
                previousPlayerStats.Add(new Player_StatisticsInfoRecord
                {
                    MID = int.Parse(d["mid"].ToString()),
                    Datetime = DateTime.Parse(d["datetime"].ToString()),
                    Result = d["result"].ToString(),
                    Position = d["position"].ToString(),
                    Teamname = d["teamname"].ToString(),
                    Opponentid = int.Parse(d["opponentid"].ToString()),
                    Opponentname = d["opponentname"].ToString(),
                    Goals = int.Parse(d["goals"].ToString()),
                    Passes = int.Parse(d["passes"].ToString()),
                    Shots = int.Parse(d["shots"].ToString()),
                    Assists = int.Parse(d["assists"].ToString()),
                    Dribbles = int.Parse(d["dribbles"].ToString()),
                    Saves = int.Parse(d["saves"].ToString()),
                });
            }
            previousPlayerStats.Sort((x, y) => DateTime.Compare(x.Datetime, y.Datetime));
            return previousPlayerStats;
        }


        private List<xPlayer_StatisticsInfoRecord> getXPreviousPlayerStats(int id)
        {
            var url = $"http://{baseUrl}/api/xplayer/player?pid={id}&limit=10";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JArray json = JArray.Parse(eventReader);

            List<xPlayer_StatisticsInfoRecord> xPreviousPlayerStats = new List<xPlayer_StatisticsInfoRecord>();

            foreach (var d in json)
            {
                xPreviousPlayerStats.Add(new xPlayer_StatisticsInfoRecord
                {
                    Datetime = DateTime.Parse(d["datetime"].ToString()),
                    xGoals = double.Parse(d["xGoals"].ToString()),
                    xPasses = double.Parse(d["xPasses"].ToString()),
                    xShots = double.Parse(d["xShots"].ToString()),
                    xAssists = double.Parse(d["xAssists"].ToString()),
                    xDribbles = double.Parse(d["xDribbles"].ToString()),
                    xSaves = double.Parse(d["xSaves"].ToString()),
                });
            }
            return xPreviousPlayerStats;
        }
    }
}
