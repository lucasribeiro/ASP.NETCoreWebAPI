using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.Helpers.Contants;
using TalkToApi.V1.Models;
using TalkToApi.V1.Models.DTO;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessageController(IMessageRepository messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("{useroneId}/{usertwoId}" , Name = "GetMessage")]        
        public ActionResult GetMessage(string useroneId, string usertwoId, [FromHeader(Name = "Accept")]string mediaType)
        {
            if (useroneId == usertwoId)
            {
                return UnprocessableEntity();
            }
            var messages = _messageRepository.GetMessages(useroneId, usertwoId);

            if (mediaType == CustomMediaType.Hateoas)
            {
                var messageList = _mapper.Map<List<Message>, List<MessageDTO>>(messages);

                var list = new ListDTO<MessageDTO>() { List = messageList };
                list.Links.Add(new LinkDTO("_self", Url.Link("GetMessage", new { useroneId = useroneId, usertwoId = usertwoId }), "GET"));

                return Ok(list);
            }
            else
            {
                return Ok(messages);
            }

           
        }

        [Authorize]
        [HttpPost("", Name = "AddMessage")]
        public ActionResult Add([FromBody]Message message, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _messageRepository.Add(message);

                    if (mediaType == CustomMediaType.Hateoas)
                    {
                        var messageDTO = _mapper.Map<Message, MessageDTO>(message);

                        messageDTO.Links.Add(new LinkDTO("_self", Url.Link("AddMessage", null), "POST"));
                        messageDTO.Links.Add(new LinkDTO("_update", Url.Link("PartialUpdate", new { id = message.Id }), "PATCH"));

                        return Ok(messageDTO);
                    }
                    else
                    {
                        return Ok(message);
                    }
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

        [Authorize]
        [HttpPatch("{id}", Name = "PartialUpdate")]
        public ActionResult PartialUpdate(int id, [FromBody]JsonPatchDocument<Message> jsonPatch, [FromHeader(Name = "Accept")] string mediaType)
        {

            // op = Operações
            // path = campo a ser alterado/adicionado/substituido
            // values = valor do campo
            // JSONPatch = { "op": "add|remove|replace", "path": "text", "value" : "Mensagem Substituida!" }

            if (jsonPatch == null)
                return BadRequest();

            var message = _messageRepository.Get(id);

            jsonPatch.ApplyTo(message);

            message.UpdateAt = DateTime.UtcNow;

            _messageRepository.Update(message);

            if (mediaType == CustomMediaType.Hateoas)
            {

                var messageDTO = _mapper.Map<Message, MessageDTO>(message);

                messageDTO.Links.Add(new LinkDTO("_self", Url.Link("Add", null), "POST"));
                messageDTO.Links.Add(new LinkDTO("_self", Url.Link("PartialUpdate", new { id = message.Id }), "PATCH"));

                return Ok(messageDTO);
            }
            else
            {
                return Ok(message);
            }
        }
    }
}
