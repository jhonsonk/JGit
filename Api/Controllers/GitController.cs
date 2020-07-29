using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GitController : ControllerBase
    {
      

        private readonly ILogger<GitController> _logger;

        public GitController(ILogger<GitController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ResponsePerRepository Get()
        {
            Class1 class1 = new Class1();            
            return class1.Clone("https://github.com/dotnet/installer/tree/HelixImages.git");
        }
    }
}
