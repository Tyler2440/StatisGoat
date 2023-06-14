using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StatisGoat.Python;
using StatisGoat.xTeam_Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisGoat.Api.Controllers
{
    public class xTeam_StatisticsController : Controller
    {
        private readonly IWebHostEnvironment env;
        private readonly IxTeam_StatisticsRepository xtsRepository;

        public xTeam_StatisticsController(IxTeam_StatisticsRepository xtsr)
        {
            xtsRepository = xtsr;
            this.env = env;
        }

        [HttpGet]
        [Route("xteam/match")]
        public async Task<IActionResult> GetByMID(int mid, int tid)
        {
            return Ok(await xtsRepository.GetByMID(mid, tid));
        }

        [HttpGet]
        [Route("xteam/team")]
        public async Task<IActionResult> GetByTID(int tid, string? competition, int? limit, string? date)
        {
            return Ok(await xtsRepository.GetByTID(tid, competition, limit, date));
        }

        [HttpGet]
        [Route("xteam")]
        public async Task<IActionResult> GetByMIDTID(int mid, int tid)
        {
            Random random = new Random();

            var randomRecord = new xTeam_StatisticsRecord
            {
                xTSId = 0,
                mId = mid,
                tId = tid,
                xGoals = TempGenRandomFloat(random),
                xConceded = TempGenRandomFloat(random),
                xShots = TempGenRandomFloat(random),
                xFouls = TempGenRandomFloat(random),
                xCards = TempGenRandomFloat(random),
                xSaves = TempGenRandomFloat(random),
                xPossession = 50
            };

            return Ok(randomRecord);
        }

        [HttpGet]
        [Route("xteam/xg")]
        public async Task<IActionResult> TrainxG()
        {
            return Ok("Trying to run Python...\n" +
                PythonHelper.ExecutePython(@"..\..\..\..\Modeling\xTeam_StatisticsTrain.py", new List<double>(), @"..\..\..\..\Modeling\xg_model.joblib", env.EnvironmentName));
        }

        private float TempGenRandomFloat(Random random)
        {
            return (float)Math.Round((random.NextDouble() * 10), 1);
        }

        [HttpGet]
        [Route("xteam/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            return NotFound();
        }
    }
}
