using StatisGoat.Postgres;
using StatisGoat.Teams;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.Postgres
{
    public class TeamsRepository : Repository, ITeamsRepository
    {
        public TeamsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Teams} (apiid, name, nation, venue, badge) " +
                $"values (:apiid, :name, :nation, :venue, :badge) " +
                $"on conflict (apiid) " +
                $"do update set apiid=excluded.apiid, name=excluded.name, nation=excluded.nation, venue=excluded.venue, badge=excluded.badge";
            BaseRead = $"select * from {Teams}";
            OrderBy = "order by (apiid, nation)";
        }

        public async Task<List<TeamsRecord>> FindAllAsync()
        {
            return (await postgres.ReadDataAsync<TeamsRecord>(BaseRead + " " + OrderBy)).ToList();
        }

        public async Task<TeamsRecord?> FindByTIDAsync(int id)
        {
            var record = await postgres.ReadDataAsync<TeamsRecord>(BaseRead +
                " where apiid=:id", new { id });
            return record.Any() ? record.First() : null;
        }

        public async Task<List<TeamsRecord>> FindByNameAsync(string name)
        {
            return (await postgres.ReadDataAsync<TeamsRecord>(BaseRead + 
                " where name=:name", new { name })).ToList();
        }

        public async Task SaveAsync(TeamsRecord record) { await postgres.WriteDataAsync(BaseWrite, record); }
    }
}
