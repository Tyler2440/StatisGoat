using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisGoat.Teams
{
    public interface ITeamsRepository
    {
        Task<List<TeamsRecord>> FindAllAsync();
        //TeamsRecord? Find(int id);
        Task<TeamsRecord?> FindByTIDAsync(int id);
        Task<List<TeamsRecord>> FindByNameAsync(string name);
        Task SaveAsync(TeamsRecord record);
    }
}
