using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StatisGoat.WebApp.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // Return the 404 not found page   
        public ActionResult Error404()
        {
            return View();
        }

        // Return the 500 not found page   
        public ActionResult Error500()
        {
            return View();
        }
    }
}
