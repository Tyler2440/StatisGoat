using Microsoft.AspNetCore.Mvc;
using StatisGoat.Accounts;
using StatisGoat.Authentication;
using StatisGoat.Favoriting;
using System.Threading.Tasks;

namespace StatisGoat.Api.Controllers
{
    public class AccountsController : ControllerBase
    {
        IAccountRepository accountRepository;
        IAuthenticationRepository authenticationRepository;
        IFavoriteTeamsRepository favoriteTeamsRepository;

        public AccountsController(IAccountRepository accountRepository, IAuthenticationRepository authenticationRepository, IFavoriteTeamsRepository favoriteTeamsRepository)
        {
            this.accountRepository = accountRepository;
            this.authenticationRepository = authenticationRepository;
            this.favoriteTeamsRepository = favoriteTeamsRepository;
        }

        [HttpPost]
        [Route("accounts/create")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRecord account)
        {
            var success = await accountRepository.CreateAccount(account);

            if (success)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("accounts/addfavteam")]
        public async Task<IActionResult> AddFavoriteTeam(string session, int teamId)
        {
            await favoriteTeamsRepository.AddFavoriteTeam(session, teamId);

            return Ok();
        }

        [HttpPost]
        [Route("accounts/removefavteam")]
        public async Task<IActionResult> RemoveFavoriteTeam(string session, int teamId)
        {
            await favoriteTeamsRepository.RemoveFavoriteTeam(session, teamId);

            return Ok();
        }

        [HttpGet]
        [Route("accounts/getfavteams")]
        public async Task<IActionResult> GetFavoriteTeams(string session)
        {
            var favTeams = (await favoriteTeamsRepository.GetFavoriteTeamsBySessionAsync(session));

            return Ok(favTeams);
        }

        [HttpGet]
        [Route("accounts/getAccountsData")]
        public async Task<IActionResult> GetAccountsData(string session)
        {
            var account = await authenticationRepository.GetAccountBySessionAsync(session);

            return Ok(account);
        }

        [HttpPost]
        [Route("accounts/updateacc")]
        public async Task<IActionResult> UpdateAccounts(string email, string first, string last, int id)
        {
            await accountRepository.UpdateAcc(email, first, last, id);

            return Ok();
        }

        [HttpGet]
        [Route("accounts/getauth")]
        public async Task<IActionResult> GetAuthenticationData(string email)
        {
            var authentication = await authenticationRepository.FindByEmailAsync(email);

            return Ok(authentication);
        }

        [HttpPost]
        [Route("accounts/updateauth")]
        public async Task<IActionResult> UpdateAuthentications(string email, string password)
        {
            await authenticationRepository.SaveAuthentication(email, password);

            return Ok();
        }

    }
}
