using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.V2.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")] // api/palavras?api-version=1.0 ou passado pelo Header
    [ApiVersion("2.0")]
    public class PalavrasController : ControllerBase
    {

        /// <summary>
        /// Recupera todas as palavras existentes
        /// </summary>
        /// <param name="query">Filtro de pesquisa</param>
        /// <returns>Listagem de palavras</returns>
        [HttpGet("", Name = "ObterTodas")]
        public string ObterTodas()
        {
            return "Versão 2.0";
        }

    }
}
