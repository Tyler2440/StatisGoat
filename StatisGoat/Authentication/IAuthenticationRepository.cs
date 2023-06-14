
using StatisGoat.Accounts;
using System;
using System.Threading.Tasks;

namespace StatisGoat.Authentication
{
    public interface IAuthenticationRepository
    {
        Task<AuthenticationRecord> FindByEmailAsync(string email);
        Task<Guid> GenerateSessionAsync(string email);
        Task<AccountRecord> GetAccountBySessionAsync(string session);
        void SignOutSessionAsync(string session);
        void SaveAuthenticationAsync(string email, string password);
        Task<bool> SaveAuthentication(string email, string password);
        Task<bool> IsSessionValidAsync(string sessionText);
    }
}
