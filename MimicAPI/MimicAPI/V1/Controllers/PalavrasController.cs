using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.V1.Models.DTO;
using MimicAPI.V1.Repositories.Contracts;
using Newtonsoft.Json;

namespace MimicAPI.V1.Controllers
{    
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")] //api/palavras?api-version=1.0 ou passado pelo Header
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]
    public class PalavrasController : Controller
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("", Name ="ObterTodas")]
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)
        {
            var items = _repository.ObterPalavras(query);

            if (items.Results.Count == 0)
                return NotFound();


            var lista = CriarLinksListPalavrasDTO(query, items);

            return Ok(lista);
        }

        private PaginationList<PalavraDTO> CriarLinksListPalavrasDTO(PalavraUrlQuery query, PaginationList<Palavra> items)
        {

            var palavras = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(items);

            foreach (var item in palavras.Results)
            {
                item.Links = new List<LinkDTO>();
                //item.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = item.Id }), "GET"));
                item.Links.Add(new LinkDTO("self", Url.ActionLink("Atualizar", "Palavras", new { id = item.Id }), "GET"));
                item.Links.Add(new LinkDTO("update", Url.ActionLink("Atualizar", "Palavras", new { id = item.Id }), "PUT"));
                item.Links.Add(new LinkDTO("delete", Url.ActionLink("Deletar", "Palavras", new { id = item.Id }), "DELETE"));
            }

            palavras.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", query), "GET"));

            if (items.Paginacao != null)
            {
                Response.Headers.Add("X-Paginator", JsonConvert.SerializeObject(items.Paginacao));

                if (query.PagNumero + 1 <= items.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    palavras.Links.Add(new LinkDTO("next", Url.Link("MimicRoute", queryString), "GET"));
                }

                if (query.PagNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    palavras.Links.Add(new LinkDTO("prev", Url.Link("MimicRoute", queryString), "GET"));
                }

            }

            return palavras;
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("{id}", Name = "Obter")]
        public ActionResult Obter(int id)
        {
            var retorno = _repository.Obter(id);

            if (retorno == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(retorno);           

            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = palavraDTO.Id }), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.ActionLink("Atualizar", "Palavras", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.ActionLink("Deletar", "Palavras", new { id = palavraDTO.Id }), "DELETE"));

            return Ok(palavraDTO);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPost("", Name ="Cadastrar")]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            if (palavra == null)
                return BadRequest();

            if (ModelState.IsValid == false)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criacao = DateTime.Now;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = palavraDTO.Id }), "GET"));


            return Created($"api/palavras/{palavra.Id}", palavraDTO);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPut("{id}", Name =  "Atualizar")]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
          
            var palavraParaAtualizar = _repository.Obter(id);

            if (palavraParaAtualizar == null)
                return NotFound();

            if (palavra == null)
                return BadRequest();

            if (ModelState.IsValid == false)
                return UnprocessableEntity(ModelState);

            palavra.Id = id;
            palavra.Ativo = palavraParaAtualizar.Ativo;
            palavra.Criacao = palavraParaAtualizar.Criacao;
            palavra.Atualizado = DateTime.Now;

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("MimicRoute", new { id = palavraDTO.Id }), "GET"));

            return Ok();

        }

        [MapToApiVersion("1.1")]
        [HttpDelete("{id}", Name = "Deletar")]
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
