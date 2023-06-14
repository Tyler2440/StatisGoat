using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Formations
{
    public interface IFormationsRepository
    {
        Task<int> SaveAsync(FormationsRecord record);
        Task<int> FindByMidTidAsync(int mid, int tid);
    }
}
