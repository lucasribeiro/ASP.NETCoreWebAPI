using Microsoft.EntityFrameworkCore;
using MimicAPI.Database;
using MimicAPI.Helpers;
using MimicAPI.Models;
using MimicAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.Repositories
{
    public class PalavraReporitory : IPalavraRepository
    {
        private readonly MimicContext _context;

        public PalavraReporitory(MimicContext context)
        {
            _context = context;
        }

        public PaginationList<Palavra> ObterPalavras(PalavraUrlQuery query)
        {
            var lista = new PaginationList<Palavra>();

            var items = _context.Palavras.AsNoTracking().AsQueryable();
            if (query.Data.HasValue)
                items = items.Where(a => a.Criacao > query.Data.Value || a.Atualizado > query.Data.Value);

            //if (query.PagRegistro.HasValue == false)
            //    query.PagRegistro = 10;

            if (query.PagNumero.HasValue)
            {
                var qtdTotalRegistros = items.Count();
                items = items.Skip(((query.PagNumero.Value - 1) * query.PagRegistro.Value)).Take(query.PagRegistro.Value);

                var paginacao = new Paginacao();
                paginacao.NumeroPagina = query.PagNumero.Value;
                paginacao.RegistrosPorPagina = query.PagRegistro.Value;
                paginacao.TotalRegistros = qtdTotalRegistros;
                paginacao.TotalPaginas = (int)Math.Ceiling((double)qtdTotalRegistros / query.PagRegistro.Value);

                lista.Paginacao = paginacao;
                
            }

            lista.Results.AddRange(items.ToList());

            return lista;
        }

        public Palavra Obter(int id)
        {
            return _context.Palavras.AsNoTracking().FirstOrDefault(a => a.Id == id);
        }

        public void Cadastrar(Palavra palavra)
        {
            _context.Palavras.Add(palavra);
            _context.SaveChanges();
        }

        public void Atualizar(Palavra palavra)
        {
            _context.Palavras.Update(palavra);
            _context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var palavra = Obter(id);
            _context.Palavras.Update(palavra);
            _context.SaveChanges();
        }

   
    }
}
