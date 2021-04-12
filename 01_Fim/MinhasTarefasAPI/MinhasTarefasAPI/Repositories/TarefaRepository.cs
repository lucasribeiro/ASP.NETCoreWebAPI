using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Repositories
{
    public class TarefaRepository : ITarefaRepository
    {
        private readonly MinhasTarefasContext _banco;

        public TarefaRepository(MinhasTarefasContext context)
        {
            _banco = context;
        }

        public List<Tarefa> Restauracao(ApplicationUser usuario, DateTime dataUlimaSincronizacao)
        {
            var query = _banco.Tarefas.Where(u => u.UsuarioId == usuario.Id).AsQueryable();

            if (dataUlimaSincronizacao != null)
            {
                query.Where(a => a.Criado >= dataUlimaSincronizacao ||
                    a.Atualizado >= dataUlimaSincronizacao);

                _banco.Tarefas.Where(a => a.UsuarioId == usuario.Id);
            }

            return query.ToList<Tarefa>();
            
        }

        public List<Tarefa> Sincronizacao(List<Tarefa> tarefas)
        {

            // Cadastrar de novos registros
            var tarefasNovas = tarefas.Where(t => t.IdTarefaApi == 0);
            foreach (var tarefa in tarefasNovas)
                _banco.Tarefas.Add(tarefa);


            // Atualização de novos registros
            var tarefasExcluidasAtualizadas = tarefas.Where(t => t.IdTarefaApi != 0);
            foreach (var tarefa in tarefasExcluidasAtualizadas)
                _banco.Tarefas.Update(tarefa);

            _banco.SaveChanges();


            return tarefasNovas.ToList();
        }
    }
}
