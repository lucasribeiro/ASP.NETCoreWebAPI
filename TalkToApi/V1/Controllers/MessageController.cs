using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;

        public MessageController(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [Authorize]
        [HttpGet("{useroneId}/{usertwoId}")]        
        public ActionResult GetMessage(string useroneId, string usertwoId)
        {
            if (useroneId == usertwoId)
            {
                return UnprocessableEntity();
            }           

            return Ok(_messageRepository.GetMessages(useroneId, usertwoId));
        }

        [Authorize]
        [HttpPost("")]
        public ActionResult Add([FromBody]Message message)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _messageRepository.Add(message);
                    return Ok(message);
                }
                catch (Exception ex)
                {
                    return UnprocessableEntity(ex);
                }                
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }
    }
}
