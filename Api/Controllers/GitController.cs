using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatisticsGitController : ControllerBase
    {
        private readonly ILogger<StatisticsGitController> _logger;

        public StatisticsGitController(ILogger<StatisticsGitController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Get(string gitUrl)
        {
            try
            {
                GitRepositoryServices repositoryService = new GitRepositoryServices();

                return Ok(await Task.Run(() => repositoryService.GetTotal(gitUrl)));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
