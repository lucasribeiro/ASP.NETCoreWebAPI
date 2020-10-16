using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Helpers;
using MimicAPI.Models;
using MimicAPI.Models.DTO;
using MimicAPI.Repositories.Contracts;
using Newtonsoft.Json;

namespace MimicAPI.Controllers
{    
    public class PalavrasController : Controller
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)
        {
            var items = _repository.ObterPalavras(query);

            if (items.Results.Count == 0)
                return NotFound();


            if (items.Paginacao != null)
                Response.Headers.Add("X-Paginator", JsonConvert.SerializeObject(items.Paginacao));

            var palavras = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(items);

            foreach (var item in palavras.Results)
            {
                item.Links = new List<LinkDTO>();
                item.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = item.Id }), "GET"));
                item.Links.Add(new LinkDTO("update", Url.ActionLink("Atualizar", "Palavras", new { id = item.Id }), "PUT"));
                item.Links.Add(new LinkDTO("delete", Url.ActionLink("Deletar", "Palavras", new { id = item.Id }), "DELETE"));
            }

            palavras.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", query), "GET"));

            return Ok(palavras);
        }

        [HttpGet]
        public ActionResult Obter(int id)
        {
            var retorno = _repository.Obter(id);

            if (retorno == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(retorno);
            palavraDTO.Links = new List<LinkDTO>();

            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = palavraDTO.Id }), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.ActionLink("Atualizar", "Palavras", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.ActionLink("Deletar", "Palavras", new { id = palavraDTO.Id }), "DELETE"));

            return Ok(palavraDTO);
        }

        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _repository.Cadastrar(palavra);

            return Created($"api/palavras/{palavra.Id}", palavra);
        }

        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {

            var palavraParaAtualizar = _repository.Obter(id);

            if (palavraParaAtualizar == null)
                return NotFound();

            palavra.Id = id;
            palavra.Atualizado = DateTime.Now;
           
            return Ok();

        }

        [HttpDelete]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);

            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);

            return NoContent();

        }

    }
}
