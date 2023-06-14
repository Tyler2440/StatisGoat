using StatisGoat.Accounts;
using StatisGoat.Postgres;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StatisGoat.Authentication
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        IPostgresConnection postgres;
        public AuthenticationRepository(IPostgresConnection postgres)
        {
            this.postgres = postgres;
        }

        public async Task<AuthenticationRecord> FindByEmailAsync(string email)
        {
            return (await postgres.ReadDataAsync<AuthenticationRecord>($"SELECT * FROM authentication WHERE email=:email", new { email })).FirstOrDefault();
        }

        public async Task<Guid> GenerateSessionAsync(string email)
        {
            var session = Guid.NewGuid();

            await postgres.WriteDataAsync("INSERT INTO sessions (email, sessionId) values (:email, :session) ON CONFLICT (email) DO UPDATE SET sessionId=excluded.sessionId;", new {email, session});

            return session;
        }

        public async Task<AccountRecord> GetAccountBySessionAsync(string sessionText)
        {
            Guid.TryParse(sessionText, out var session);

            return (await postgres.ReadDataAsync<AccountRecord>($"SELECT accounts.id, accounts.email, accounts.first, accounts.last FROM accounts LEFT JOIN sessions ON accounts.email = sessions.email WHERE sessionId=:session;", new { session })).FirstOrDefault();
        }

        public async void SignOutSessionAsync(string sessionText)
        {
            Guid.TryParse(sessionText, out var session);

            await postgres.WriteDataAsync("DELETE FROM sessions WHERE sessionId=:session;", new { session });
        }

        public async void SaveAuthenticationAsync(string email, string password)
        {
            await postgres.WriteDataAsync("INSERT INTO authentication (email, password) VALUES (:email, :password) ON CONFLICT (email) DO UPDATE SET password=excluded.password;", new { email, password });
        }

        public async Task<bool> SaveAuthentication(string email, string password)
        {
            await postgres.WriteDataAsync("INSERT INTO authentication (email, password) VALUES (:email, :password) ON CONFLICT (email) DO UPDATE SET password=excluded.password;", new { email, password });
            return true;
        }

        public async Task<bool> IsSessionValidAsync(string sessionText)
        {
            Guid.TryParse(sessionText, out var session);

            return (await postgres.ReadDataAsync<string>($"SELECT email FROM sessions WHERE sessionId=:session;", new {session})).FirstOrDefault() != null;
        }
    }
}
