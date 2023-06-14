using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public class AvgPlayer_StatisticsInfoRecord
    {
        public int PID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int Nummatches { get; set; }
        public double Avgteamscored { get; set; }
        public double Avgteamconceded { get; set; }
        public double Avgminutes { get; set; }
        public double Avgrating { get; set; }
        public double Avgshots { get; set; }
        public double Avgshots_on_goal { get; set; }
        public double Avggoals { get; set; }
        public double Avgassists { get; set; }
        public double Avggoal_contributions { get; set; }
        public double Avgsaved { get; set; }
        public double Avgconceded { get; set; }
        public double Avgpasses { get; set; }
        public double Avgkey_passes { get; set; }
        public double Avgpasses_accurate { get; set; }
        public double Avgpass_pct{ get; set; }
        public double Avgtackles { get; set; }
        public double Avgblocks { get; set; }
        public double Avginterceptions { get; set; }
        public double Avgduels { get; set; }
        public double Avgduels_won { get; set; }
        public double Avgdribbles { get; set; }
        public double Avgdribbles_won { get; set; }
        public double Avgdribbles_past { get; set; }
        public double Avgfouls_drawn { get; set; }
        public double Avgfouls_committed { get; set; }
        public double Avgyellow { get; set; }
        public double Avgred { get; set; }
        public double Avgpenalties_won { get; set; }
        public double Avgpenalties_conceded { get; set; }
        public double Avgpenalties_scored { get; set; }
        public double Avgpenalties_missed { get; set; }
        public double Avgpenalties_saved { get; set; }
        public double Percent_scored { get; set; }
        public double Percent_assisted { get; set; }
        public double Percent_contributed { get; set; }
    }
}
