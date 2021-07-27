using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;

namespace TalkToApi.V1.Repositories.Contracts
{
    public interface IUserRepository
    {
        void Add(ApplicationUser usuario, string senha);
        ApplicationUser Get(string email, string senha);
        ApplicationUser Get(string id);
    }
}
