using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xPlayer_Statistics
{
    public class SumxPlayer_StatisticsInfoRecord
    {
        public int PID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int Nummatches { get; set; }
        public double Sum_teamscored { get; set; }
        public double Sum_teamconceded { get; set; }
        public double Sum_xrating { get; set; }
        public double Sum_rating_perf { get; set; }
        public double Sum_xshots { get; set; }
        public double Sum_shots_perf { get; set; }
        public double Sum_xgoals { get; set; }
        public double Sum_goals_perf { get; set; }
        public double Sum_xassists { get; set; }
        public double Sum_assists_perf { get; set; }
        public double Sum_xsaves { get; set; }
        public double Sum_saves_perf { get; set; }
        public double Sum_xpasses { get; set; }
        public double Sum_passes_perf { get; set; }
        public double Sum_xtackles { get; set; }
        public double Sum_tackles_perf { get; set; }
        public double Sum_xinterceptions { get; set; }
        public double Sum_interceptions_perf { get; set; }
        public double Sum_xdribbles { get; set; }
        public double Sum_dribbles_perf { get; set; }
        public double Sum_xfouls { get; set; }
        public double Sum_fouls_perf { get; set; }
        public double Sum_xyellow { get; set; }
        public double Sum_yellow_perf { get; set; }
        public double Sum_xred { get; set; }
        public double Sum_red_perf { get; set; }
    }
}
