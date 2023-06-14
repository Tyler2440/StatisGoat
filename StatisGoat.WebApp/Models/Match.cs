using StatisGoat.Matches;
using StatisGoat.Events;
using StatisGoat.Player_Statistics;
using StatisGoat.xPlayer_Statistics;
using StatisGoat.Team_Statistics;
using StatisGoat.xTeam_Statistics;
using StatisGoat.Lineups;
using System;
using System.Collections.Generic;
using StatisGoat.Chats;

namespace StatisGoat.WebApp.Models
{
    public class Match
    {
        public List<Player> homePlayers { get; set; }
        public string homeScore { get; set; }
        public List<Player> awayPlayers { get; set; }
        public string awayScore { get; set; }
        public Team_StatisticsInfoRecord homeStatistics { get; set; }
        public xTeam_StatisticsInfoRecord xHomeStatistics { get; set; }
        public Team_StatisticsInfoRecord awayStatistics { get; set; }
        public xTeam_StatisticsInfoRecord xAwayStatistics { get; set; }
        public List<LineupsInfoRecord> homeLineups { get; set; }
        public List<LineupsInfoRecord> awayLineups { get; set; }
        public MatchesInfoRecord matchesInfoRecord { get; set; }
        public List<EventsInfoRecord> eventsInfoRecords { get; set; }
        public List<MatchesInfoRecord> homePreviousMatchesInfoRecords { get; set; }
        public List<MatchesInfoRecord> awayPreviousMatchesInfoRecords { get; set; }
        public List<MatchesInfoRecord> teamsPreviousMatchesInfoRecords { get; set; }
        public List<Player_StatisticsInfoRecord> allPlayerStatistics { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopRatings { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopGoals { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopAssists { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopGoalsAssists { get; set; }

        public List<xPlayer_StatisticsInfoRecord> allxPlayerStatistics { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopRatings { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopGoals { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopAssists { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopGoalsAssists { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopSaves { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopSaves { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopPasses { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopPasses { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopKeyPasses { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopPassPct { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopTackles { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopTackles { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopInterceptions { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopInterceptions { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopDribbles { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopDribbles { get; set; }
        public List<Player_StatisticsInfoRecord> sortedTopFouls { get; set; }
        public List<xPlayer_StatisticsInfoRecord> sortedxTopFouls { get; set; }
        public List<ChatInfoRecord> comments { get; set; }
    }
}
