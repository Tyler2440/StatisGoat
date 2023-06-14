using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StatisGoat.Team_Statistics;
using StatisGoat.Player_Statistics;
using StatisGoat.xTeam_Statistics;
using StatisGoat.Matches;

namespace StatisGoat.WebApp.Models
{
    public class Team : Team_StatisticsInfoRecord
    {
        public string Badge { get; set; }
        public string Nation { get; set; }
        public string Venue { get; set; }
        public List<Team_StatisticsInfoRecord> previousMatchesStats { get; set; }
        public List<xTeam_StatisticsInfoRecord> previousxMatchesStats { get; set; }
        public List<MatchesInfoRecord> upcomingMatches { get; set; }
        public List<xTeam_StatisticsRecord> upcomingMatchesStats { get; set; }
        public List<Player> Roster { get; set; }
        // Rating/Goals/Assists is key, value is player list
        public List<AvgPlayer_StatisticsInfoRecord> topAvgPlayerStats { get; set; }
        public List<SumPlayer_StatisticsInfoRecord> topSumPlayerStats { get; set; }
        public List<AvgPlayer_StatisticsInfoRecord> topPlayerRatings { get; set; }
        public List<SumPlayer_StatisticsInfoRecord> topPlayerGoals { get; set; }
        public List<SumPlayer_StatisticsInfoRecord> topPlayerAssists { get; set; }
        public Avg_TeamStatisticsInfoRecord avgStats { get; set; }
        public List<MatchesInfoRecord> previousMatches { get; set; }

    }
}
