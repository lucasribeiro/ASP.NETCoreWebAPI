using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Database;
using MimicAPI.Models;

namespace MimicAPI.Controllers
{    
    public class PalavrasController : Controller
    {
        private readonly MimicContext _database;

        public PalavrasController(MimicContext database)
        {
            _database = database;
        }

        [HttpGet]
        public ActionResult ObterTodas()
        {
            return Ok(_database.Palavras);
        }

        [HttpGet]
        public ActionResult Obter(int id)
        {
            var retorno = _database.Palavras.Find(id);
            if (retorno == null)
                return NotFound();

            return Ok();
        }

        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            _database.Palavras.Add(palavra);
            _database.SaveChanges();
            return Created($"api/palavras/{palavra.Id}", palavra);
        }

        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {

            var palavraParaAtualizar = _database.Palavras.AsNoTracking().FirstOrDefault(a => a.Id == id);
            if (palavraParaAtualizar == null)
                return NotFound();

            palavra.Id = id;
            palavra.Atualizado = DateTime.Now;
            _database.Palavras.Update(palavra);
            _database.SaveChanges();
            return Ok();

        }

        [HttpDelete]
        public ActionResult Deletar(int id)
        {

            var palavra = _database.Palavras.Find(id);
            if (palavra == null)
                return NotFound();

            palavra.Ativo = false;
            _database.Palavras.Update(palavra);
            _database.SaveChanges();
            return NoContent();

        }

    }
}
