using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Player_Statistics
{
    public interface IPlayer_StatisticsRepository
    {
        Task SaveAsync(Player_StatisticsRecord record);
        Task<List<Player_StatisticsInfoRecord>> FindByMIDAsync(int mid, int? pid);
        Task<List<Player_StatisticsInfoRecord>> FindByDay(string day, string? competition);
        Task<List<Player_StatisticsInfoRecord>> FindByPIDAsync(int pid, string? competition, int? limit, string? date);
        Task<AvgPlayer_StatisticsInfoRecord> FindAvgByPID(int pid, string? competition, int? limit, string? date);
        Task<SumPlayer_StatisticsInfoRecord> FindSumByPID(int pid, string? competition, int? limit, string? date);
        Task<List<AvgPlayer_StatisticsInfoRecord>> FindAvgByTID(int tid, string? competition, int? limit, string? date);
        Task<List<SumPlayer_StatisticsInfoRecord>> FindSumByTID(int tid, string? competition, int? limit, string? date);
    }
}
