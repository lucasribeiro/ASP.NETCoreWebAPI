﻿using MinhasTarefasAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinhasTarefasAPI.Repositories.Contracts
{
    public interface ITokenRepository
    {
        void Cadastrar(Token token);

        Token Obter(string refreshToken);

        void Atualizar(Token token);
    }
}
