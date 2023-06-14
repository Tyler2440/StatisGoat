using DevOne.Security.Cryptography.BCrypt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StatisGoat.Accounts;
using StatisGoat.Authentication;
using StatisGoat.WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private string baseUrl;
        private Accounts_Pass acc;
        private readonly IAuthenticationRepository authenticationRepository;
        public AccountController(IWebHostEnvironment env, IAuthenticationRepository authenticationRepository)
        {
            this.authenticationRepository = authenticationRepository;
            acc = new Accounts_Pass();
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

        public IActionResult Details()
        {
            var session = Request.Cookies["StatisGoatSession"];

            var url = $"http://{baseUrl}/api/accounts/getAccountsData?session={session}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JObject json = JObject.Parse(eventReader);

            acc.Email = json["email"].ToString();
            acc.First = json["first"].ToString();
            acc.Last = json["last"].ToString();
            acc.Id = int.Parse(json["id"].ToString());
            return View(acc);
        }
        public string Passwords(string email)
        {
            var url = $"http://{baseUrl}/api/accounts/getauth?email={email}";
            var request = WebRequest.Create(url);
            request.Method = "GET";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var readerHome = new StreamReader(webStream);
            string eventReader = readerHome.ReadToEnd();
            JObject json = JObject.Parse(eventReader);

            string password = json["password"].ToString();
            return password;
        }
        [HttpPost]
        public IActionResult Updates(string email, string first, string last, int id)
        {
     
            var url = $"http://{baseUrl}/api/accounts/updateacc?email={email}&first={first}&last={last}&id={id}";
            var request = WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(url);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var webResponse = request.GetResponse();
             

            return Redirect("~/");
        }
        [HttpPost]
        public IActionResult UpdatePasswords(string email, string currpassword, string newpassword, string reenterPassword)
        {
            string pass = Passwords(email);
            if (currpassword != null && BCryptHelper.CheckPassword(currpassword, pass))
            {
                var bcryptPassword = BCryptHelper.HashPassword(newpassword, BCryptHelper.GenerateSalt());

                if (bcryptPassword != null)
                {
                    var url = $"http://{baseUrl}/api/accounts/updateauth?email={email}&password={bcryptPassword}";
                    var request = WebRequest.Create(url);

                    var data = Encoding.ASCII.GetBytes(url);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var webResponse = request.GetResponse();
                }
                return Json(new { success = true});

            }
            else
            {
                Debug.WriteLine(currpassword);
                Debug.WriteLine(email);
                Debug.WriteLine(reenterPassword);
                return Json(new { success = false, message = "Current password is incorrect." });
            }
            return Redirect("~/");
        }
    }
}
