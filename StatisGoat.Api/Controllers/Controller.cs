using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;

namespace StatisGoat.Api.Controllers
{
    public abstract class Controller : ControllerBase
    {
        public abstract Task<IActionResult> Backfill();
    }
}
