using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.xTeam_Statistics
{
    public interface IxTeam_StatisticsRepository
    {
        Task<xTeam_StatisticsInfoRecord> GetByMID(int mid, int tid);
        Task<IEnumerable<xTeam_StatisticsInfoRecord>> GetByTID(int tid, string? competition, int? limit, string? date);
    }
}
