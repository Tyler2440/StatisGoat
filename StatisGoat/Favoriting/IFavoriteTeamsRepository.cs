using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Favoriting
{
    public interface IFavoriteTeamsRepository
    {
        public Task<List<int>> GetFavoriteTeamsBySessionAsync(string session);
        public Task AddFavoriteTeam(string session, int teamId);
        public Task RemoveFavoriteTeam(string session, int teamId);
    }
}
