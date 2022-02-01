using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MessageController : ControllerBase
    {

        public ActionResult GetMessage()
        {
            return Ok();
        }

        public ActionResult Add()
        {
            return Ok();
        }
    }
}
