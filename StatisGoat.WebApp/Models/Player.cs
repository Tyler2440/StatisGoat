using System;
using System.Collections.Generic;
using StatisGoat.Players;
using StatisGoat.Player_Statistics;
using StatisGoat.xPlayer_Statistics;

namespace StatisGoat.WebApp.Models
{
    public class Player : Player_StatisticsInfoRecord
    {
        public string DOB { get; set; }
        public int Age { get; set; }
        public string? TeamNation { get; set; }
        public string? Badge { get; set; }
        public int ApiID { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? Nationality { get; set; }
        public List<Player_StatisticsInfoRecord> PreviousPlayerStats { get; set; }
        public List<xPlayer_StatisticsInfoRecord> xPreviousPlayerStats { get; set; }
    }
}
