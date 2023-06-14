using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisGoat.Players
{
    public interface IPlayersRepository
    {
        Task<List<PlayersInfoRecord>> FindAllAsync();
        Task<PlayersInfoRecord?> FindByPIDAsync(int id);
        Task<List<PlayersInfoRecord>> FindByNameAsync(string first, string last);
        Task<List<PlayersInfoRecord>> FindByTeamAsync(int id);
        Task SaveAsync(PlayersRecord record);
        Task BackfillPlayer(int id, int tid, int year);
    }
}
