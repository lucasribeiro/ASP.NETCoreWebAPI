using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;

namespace TalkToApi.V1.Repositories.Contracts
{
    public interface ITokenRepository
    {
        void Add(Token token);

        Token Get(string refreshToken);

        void Update(Token token);
    }
}
