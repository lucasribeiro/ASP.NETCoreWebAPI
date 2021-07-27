using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.Repositories.Context;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Repositories
{
    public class TokenRepository : ITokenRepository
    {

        private readonly TalkToApiContext _banco;
        public TokenRepository(TalkToApiContext banco)
        {
            _banco = banco;
        }

        public void Update(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }

        public void Add(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        public Token Get(string refreshToken)
        {
            return _banco.Token.FirstOrDefault(t => t.RefreshToken == refreshToken && t.Used == false);
        }
    }
}
