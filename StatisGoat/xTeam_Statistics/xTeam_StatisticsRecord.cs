using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xTeam_Statistics
{
    public class xTeam_StatisticsRecord
    {
        public int xTSId { get; set; }
        public int mId { get; set; }
        public int tId { get; set; }
        public double xGoals { get; set; }
        public double xConceded { get; set; }
        public double xShots { get; set; }
        public double xFouls { get; set; }
        public double xCards { get; set; }
        public double xSaves { get; set; }
        public double xPossession { get; set; }
    }
}
