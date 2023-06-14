using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Formations
{
    public class FormationsRepository : Repository, IFormationsRepository
    {
        public FormationsRepository(IPostgresConnection ipc) : base(ipc)
        {
            BaseWrite = $"insert into {Formations} (mid, tid, formation) values " +
                $"(:mid, :tid, :formation) " +
                $"on conflict (mid, tid) do update set " +
                $"mid=excluded.mid, tid=excluded.tid, formation=excluded.formation;";
            BaseRead = $"select fid from {Formations} where mid=:mid and tid=:tid;";
        }

        public async Task<int> FindByMidTidAsync(int mid, int tid)
        {
            return (await postgres.ReadDataAsync<int>(BaseRead, new { mid, tid })).FirstOrDefault();
        }

        public async Task<int> SaveAsync(FormationsRecord record)
        {
            await postgres.WriteDataAsync(BaseWrite, record);
            return await FindByMidTidAsync(record.MID, record.TID);
        }
    }
}
