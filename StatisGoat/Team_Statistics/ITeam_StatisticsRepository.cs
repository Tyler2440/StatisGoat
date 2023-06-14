using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Team_Statistics
{
    public interface ITeam_StatisticsRepository
    {
        Task SaveAsync(Team_StatisticsRecord record);
        Task<Team_StatisticsInfoRecord> FindByMIDTIDAsync(int mid, int tid);
        Task<List<Team_StatisticsInfoRecord>> FindByTID(int tid, string? competition, int? limit, string? date);
        Task<Avg_TeamStatisticsInfoRecord> FindAvgByTID(int tid, string? competition, int? limit, string? date);
        Task<Sum_TeamStatisticsInfoRecord> FindSumByTID(int tid, string? competition, int? limit, string? date);
    }
}
