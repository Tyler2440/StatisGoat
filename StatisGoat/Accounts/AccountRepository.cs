using StatisGoat.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Accounts
{
    public class AccountRepository : IAccountRepository
    {
        IPostgresConnection postgres;

        public AccountRepository(IPostgresConnection postgres)
        {
            this.postgres = postgres;
        }

        public async Task<bool> CreateAccount(AccountRecord account)
        {
            var existing = (await postgres.ReadDataAsync<string>("SELECT email FROM accounts WHERE email=:email", new { account.Email })).FirstOrDefault();
            if (String.IsNullOrEmpty(existing))
            {
                await (postgres.WriteDataAsync("INSERT INTO accounts (email, first, last) VALUES (:email, :first, :last);", account));
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateAcc(string email, string first, string last, int id)
        {
            await postgres.WriteDataAsync("INSERT INTO accounts (email, first, last, id) values (:email, :first, :last, :id) ON CONFLICT (id) DO UPDATE SET email=excluded.email, first=excluded.first, last=excluded.last, id=excluded.id;", new { email, first, last, id });
            return true;
        }
    }
}
