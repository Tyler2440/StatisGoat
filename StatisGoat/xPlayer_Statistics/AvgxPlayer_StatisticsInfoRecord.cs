using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xPlayer_Statistics
{
    public class AvgxPlayer_StatisticsInfoRecord
    {
        public int PID { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Nickname { get; set; }
        public string? Headshot { get; set; }
        public int Nummatches { get; set; }
        public double Avg_teamscored { get; set; }
        public double Avg_teamconceded { get; set; }
        public double Avg_xrating { get; set; }
        public double Avg_rating_perf { get; set; }
        public double Avg_xshots { get; set; }
        public double Avg_shots_perf { get; set; }
        public double Avg_xgoals { get; set; }
        public double Avg_goals_perf { get; set; }
        public double Avg_xassists { get; set; }
        public double Avg_assists_perf { get; set; }
        public double Avg_xsaves { get; set; }
        public double Avg_saves_perf { get; set; }
        public double Avg_xpasses { get; set; }
        public double Avg_passes_perf { get; set; }
        public double Avg_xtackles { get; set; }
        public double Avg_tackles_perf { get; set; }
        public double Avg_xinterceptions { get; set; }
        public double Avg_interceptions_perf { get; set; }
        public double Avg_xdribbles { get; set; }
        public double Avg_dribbles_perf { get; set; }
        public double Avg_xfouls { get; set; }
        public double Avg_fouls_perf { get; set; }
        public double Avg_xyellow { get; set; }
        public double Avg_yellow_perf { get; set; }
        public double Avg_xred { get; set; }
        public double Avg_red_perf { get; set; }
    }
}
