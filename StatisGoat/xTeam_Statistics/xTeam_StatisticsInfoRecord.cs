using System;

namespace StatisGoat.xTeam_Statistics
{
    public class xTeam_StatisticsInfoRecord
    {
        public int TID { get; set; }
        public string? Teamname { get; set; }
        public int MID { get; set; }
        public int OpponentID { get; set; }
        public string? Opponentname { get; set; }
        public string? Competition { get; set; }
        public DateTime Datetime { get; set; }
        public string? Status { get; set; }
        public int Teamscored { get; set; }
        public int Teamconceded { get; set; }
        public string? Result { get; set; }
        public double xRating { get; set; }
        public double Rating_perf { get; set; }
        public double xShots { get; set; }
        public double Shots_perf { get; set; }
        public double xGoals { get; set; }
        public double Goals_perf { get; set; }
        public double xAssists { get; set; }
        public double Assists_perf { get; set; }
        public double xSaves { get; set; }
        public double Saves_perf { get; set; }
        public double xPasses { get; set; }
        public double Passes_perf { get; set; }
        public double xTackles { get; set; }
        public double Tackles_perf { get; set; }
        public double xInterceptions { get; set; }
        public double Interceptions_perf { get; set; }
        public double xDribbles { get; set; }
        public double Dribbles_perf { get; set; }
        public double xFouls { get; set; }
        public double Fouls_perf { get; set; }
        public double xYellow { get; set; }
        public double Yellow_perf { get; set; }
        public double xRed { get; set; }
        public double Red_perf { get; set; }
    }
}
