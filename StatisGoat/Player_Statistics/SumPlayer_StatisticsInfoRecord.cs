using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public class SumPlayer_StatisticsInfoRecord
    {
        public int PID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int Nummatches { get; set; }
        public int Sumteamscored { get; set; }
        public int Sumteamconceded { get; set; }
        public int Summinutes { get; set; }
        public int Sumrating { get; set; }
        public int Sumshots { get; set; }
        public int Sumshots_on_goal { get; set; }
        public int Sumgoals { get; set; }
        public int Sumassists { get; set; }
        public int Sumgoal_contributions { get; set; }
        public int Sumsaved { get; set; }
        public int Sumconceded { get; set; }
        public int Sumpasses { get; set; }
        public int Sumkey_passes { get; set; }
        public int Sumpasses_accurate { get; set; }
        public int Sumpass_pct { get; set; }
        public int Sumtackles { get; set; }
        public int Sumblocks { get; set; }
        public int Suminterceptions { get; set; }
        public int Sumduels { get; set; }
        public int Sumduels_won { get; set; }
        public int Sumdribbles { get; set; }
        public int Sumdribbles_won { get; set; }
        public int Sumdribbles_past { get; set; }
        public int Sumfouls_drawn { get; set; }
        public int Sumfouls_committed { get; set; }
        public int Sumyellow { get; set; }
        public int Sumred { get; set; }
        public int Sumpenalties_won { get; set; }
        public int Sumpenalties_conceded { get; set; }
        public int Sumpenalties_scored { get; set; }
        public int Sumpenalties_missed { get; set; }
        public int Sumpenalties_saved { get; set; }
        public double Percent_scored { get; set; }
        public double Percent_assisted { get; set; }
        public double Percent_contributed { get; set; }
    }
}
