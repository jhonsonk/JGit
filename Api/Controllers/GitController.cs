using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service;
using Service.Model;
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
        public ActionResult<ResponsePerRepository> Get(string gitUrl)
        {
            try
            {
                GitRepositoryService repositoryService = new GitRepositoryService();

                return Ok(repositoryService.GetTotal(gitUrl));
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
