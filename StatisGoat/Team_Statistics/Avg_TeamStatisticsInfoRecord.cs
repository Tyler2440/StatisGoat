using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public class Avg_TeamStatisticsInfoRecord
    {
        public int TID { get; set; }
        public string? Teamname { get; set; }
        public double Nummatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double Avgscored { get; set; }
        public double Avgconceded { get; set; }
        public double Avgshots { get; set; }
        public double Avgshots_on_goal { get; set; }
        public double Avgshots_off_goal { get; set; }
        public double Avgshots_inside { get; set; }
        public double Avgshots_outside { get; set; }
        public double Avgblocked { get; set; }
        public double Avgfouls { get; set; }
        public double Avgcorners { get; set; }
        public double Avgoffsides { get; set; }
        public double Avgpossession { get; set; }
        public double Avgyellows { get; set; }
        public double Avgreds { get; set; }
        public double Avgsaves { get; set; }
        public double Avgpasses { get; set; }
        public double Avgpasses_accurate { get; set; }
        public double Avgpass_pct { get; set; }

    }
}
