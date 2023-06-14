using StatisGoat.Authentication;
using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Favoriting
{
    public class FavoriteTeamsRepository : IFavoriteTeamsRepository
    {
        readonly IPostgresConnection postgres;
        readonly IAuthenticationRepository authenticationRepository;
        const string TABLE = "FavoriteTeams";

        public FavoriteTeamsRepository(IPostgresConnection postgres, IAuthenticationRepository authenticationRepository)
        {
            this.postgres = postgres;
            this.authenticationRepository = authenticationRepository;
        }

        public async Task<List<int>> GetFavoriteTeamsBySessionAsync(string session)
        {
            var account = await authenticationRepository.GetAccountBySessionAsync(session);

            if (account == null)
                return new List<int>();

            return (await postgres.ReadDataAsync<int>($"select tid from {TABLE} where aid={account.Id};")).ToList();
        }

        public async Task AddFavoriteTeam(string session, int teamId)
        {
            var account = await authenticationRepository.GetAccountBySessionAsync(session);

            if (account == null)
                return;

            await postgres.WriteDataAsync($"insert into {TABLE} (aid, tid) values ({account.Id}, {teamId});");
        }

        public async Task RemoveFavoriteTeam(string session, int teamId)
        {
            var account = await authenticationRepository.GetAccountBySessionAsync(session);

            if (account == null)
                return;

            await postgres.WriteDataAsync($"delete from {TABLE} where aid={account.Id} and tid={teamId};");
        }
    }
}
