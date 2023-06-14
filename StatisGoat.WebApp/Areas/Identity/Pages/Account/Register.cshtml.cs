using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DevOne.Security.Cryptography.BCrypt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StatisGoat.Accounts;
using StatisGoat.Areas.Identity.Data;
using StatisGoat.Authentication;

namespace StatisGoat.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        IAuthenticationRepository authenticationRepository;

        private string baseUrl;

        public RegisterModel(IAuthenticationRepository authenticationRepository, IWebHostEnvironment env)
        {
            this.authenticationRepository = authenticationRepository;

            switch (env.EnvironmentName)
            {
                case "Production":
                    baseUrl = "44.241.69.127:90";
                    break;
                case "Development":
                    baseUrl = "localhost:5000";
                    break;
                default:
                    baseUrl = "localhost:5000";
                    break;
            }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            /*
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            */
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var newAccount = new AccountRecord
            {
                Email = Input.Email,
                First = Input.FirstName,
                Last = Input.LastName
            };

            var url = $"http://{baseUrl}/api/accounts/create";

            var requestBody = JsonConvert.SerializeObject(newAccount);
            var requestBodyBytes = Encoding.ASCII.GetBytes(requestBody);

            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = requestBodyBytes.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(requestBodyBytes, 0, requestBodyBytes.Length);

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var bcryptPassword = BCryptHelper.HashPassword(Input.Password, BCryptHelper.GenerateSalt());

                        authenticationRepository.SaveAuthenticationAsync(Input.Email, bcryptPassword);
                        Response.Cookies.Append("StatisgoatSession", (await authenticationRepository.GenerateSessionAsync(Input.Email)).ToString());

                        return LocalRedirect(returnUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                return Page();
            }

            return Page();
        }
    }
}
