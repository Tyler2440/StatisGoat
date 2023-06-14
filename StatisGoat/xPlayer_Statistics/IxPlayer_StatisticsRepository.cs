using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xPlayer_Statistics
{
    public interface IxPlayer_StatisticsRepository
    {
        Task SaveAsync(xPlayer_StatisticsRecord record);
        Task<xPlayer_StatisticsRecord> FindAsync(int lid);
        Task<IEnumerable<xPlayer_StatisticsInfoRecord>> GetByMID(int mid, int? pid = null);
        Task<IEnumerable<xPlayer_StatisticsInfoRecord>> GetByPID(int pid, string? competition, int? limit, string? date);
        Task<AvgxPlayer_StatisticsInfoRecord> GetAvgByPID(int pid, string? competition, int? limit, string? date);
        Task<SumxPlayer_StatisticsInfoRecord> GetSumByPID(int pid, string? competition, int? limit, string? date);
        Task<IEnumerable<AvgxPlayer_StatisticsInfoRecord>> GetAvgByTID(int tid, string? competition, int? limit, string? date);
        Task<IEnumerable<SumxPlayer_StatisticsInfoRecord>> GetSumByTID(int tid, string? competition, int? limit, string? date);
    }
}
