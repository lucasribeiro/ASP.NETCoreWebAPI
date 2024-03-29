﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser Get(string email, string senha)
        {
            var usuario = _userManager.FindByEmailAsync(email).Result;
            if(_userManager.CheckPasswordAsync(usuario, senha).Result)
            {
                return usuario;
            }
            else
            {
                /*
                 * Domain Notification
                 */
                throw new Exception("Usuário não localizado!");
            }
        }
        public void Add(ApplicationUser usuario, string senha)
        {
            var result = _userManager.CreateAsync(usuario, senha).Result;
            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var erro in result.Errors)
                {
                    sb.Append(erro.Description);
                }
                ;
                /*
                 * Domain Notification
                 */
                throw new Exception($"Usuário não cadastrado! {sb.ToString()}");
            }
        }

        public ApplicationUser Get(string id)
        {
            return _userManager.FindByIdAsync(id).Result;
        }
    }
}
