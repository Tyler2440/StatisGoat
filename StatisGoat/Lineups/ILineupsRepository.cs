using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Lineups
{
    public interface ILineupsRepository
    {
        Task<List<LineupsInfoRecord>> FindByMIDAsync(int mid);
        Task<List<LineupsInfoRecord>> FindByMIDTIDAsync(int mid, int tid);
        Task<int> FindByPidFidAsync(int pid, int fid);
        Task SaveAsync(LineupsRecord record);

    }
}
