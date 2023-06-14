using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public class Sum_TeamStatisticsInfoRecord
    {
        public int TID { get; set; }
        public string? Teamname { get; set; }
        public int Nummatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Sumscored { get; set; }
        public int Sumconceded { get; set; }
        public int Sumshots { get; set; }
        public int Sumshots_on_goal { get; set; }
        public int Sumshots_off_goal { get; set; }
        public int Sumshots_inside { get; set; }
        public int Sumshots_outside { get; set; }
        public int Sumblocked { get; set; }
        public int Sumfouls { get; set; }
        public int Sumcorners { get; set; }
        public int Sumoffsides { get; set; }
        public int Sumpossession { get; set; }
        public int Sumyellows { get; set; }
        public int Sumreds { get; set; }
        public int Sumsaves { get; set; }
        public int Sumpasses { get; set; }
        public int Sumpasses_accurate { get; set; }
        public int Sumpass_pct { get; set; }
    }
}
