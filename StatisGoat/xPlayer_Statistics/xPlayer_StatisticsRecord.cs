using System.Collections.Generic;

namespace StatisGoat.xPlayer_Statistics
{
    public class xPlayer_StatisticsRecord
    {
        public int lId { get; set; }
        public double xRating { get; set; }
        public double xShots { get; set; }
        public double xGoals { get; set; }
        public double xAssists { get; set; }
        public double xSaves { get; set; }
        public double xPasses { get; set; }
        public double xTackles { get; set; }
        public double xInterceptions { get; set; }
        public double xDribbles { get; set; }
        public double xFouls { get; set; }
        public double xYellow { get; set; }
        public double xRed { get; set; }

        public List<double> xRating_Features()
        {
            return new List<double> { xShots, xGoals, xAssists, xSaves, xPasses, xTackles, 
                                     xInterceptions, xDribbles, xFouls, xYellow, xRed };
        }
    }
}
