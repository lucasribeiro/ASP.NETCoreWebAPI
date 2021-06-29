﻿using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Repositories
{
    public class TokenRepository : ITokenRepository
    {

        private readonly MinhasTarefasContext _banco;
        public TokenRepository(MinhasTarefasContext banco)
        {
            _banco = banco;
        }

        public void Atualizar(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }

        public void Cadastrar(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        public Token Obter(string refreshToken)
        {
            return _banco.Token.FirstOrDefault(t => t.RefreshToken == refreshToken && t.Utilizado == false);
        }
    }
}
