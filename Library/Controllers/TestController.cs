using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/infos")]
    public class TestController : Controller
    {
        [HttpGet()]
        public IActionResult GetInfos()
        {
            var infos = new List<string>();

            infos.Add("Test1");
            infos.Add("Test2");
            infos.Add("Test3");
            infos.Add("Test4");

            return Ok(infos);

        }
    }
}
