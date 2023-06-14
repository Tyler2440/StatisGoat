using StatisGoat.Matches;
using StatisGoat.Player_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.WebApp.Models
{
    public class Home
    {
        public List<MatchesInfoRecord> todaysMatches { get; set; }
        public List<Player_StatisticsInfoRecord> topPlayers { get; set; }
    }
}
