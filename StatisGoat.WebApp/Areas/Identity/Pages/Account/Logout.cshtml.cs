using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StatisGoat.Authentication;

namespace StatisGoat.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly IAuthenticationRepository authenticationRepository;

        public LogoutModel(IAuthenticationRepository authenticationRepository)
        {
            this.authenticationRepository = authenticationRepository;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var session = Request.Cookies["StatisGoatSession"];
            authenticationRepository.SignOutSessionAsync(session);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            Response.Cookies.Append("StatisGoatSession", "logout", cookieOptions);
            

            return LocalRedirect(returnUrl);
        }
    }
}
