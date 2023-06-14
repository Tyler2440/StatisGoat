using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Accounts
{
    public interface IAccountRepository
    {
        public Task<bool> CreateAccount(AccountRecord account);

        public Task<bool> UpdateAcc(string email, string first, string last, int id);
    }
}
