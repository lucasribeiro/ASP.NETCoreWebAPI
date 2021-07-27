using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Repositories.Contracts;
using TalkToApi.V1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TalkToApi.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usuarioRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(IUserRepository usuarioRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository)
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] UserDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");

            if (ModelState.IsValid)
            {
                ApplicationUser usuario = _usuarioRepository.Get(usuarioDTO.Email, usuarioDTO.Password);
                if (usuario != null)
                {
                    //Login no Identity
                    //_signInManager.SignInAsync(usuario, false);

                    return Ok(GetUserToken(usuario));
                }
                else
                {
                    return NotFound("Usuário não localizado!");
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }


        [HttpPost("Renovar")]
        public ActionResult Renew([FromBody] TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.Get(tokenDTO.RefreshToken);

            if (refreshTokenDB == null)
                return NotFound();

            //Desativa token ja utilizado
            refreshTokenDB.UpdatedAt = DateTime.Now;
            refreshTokenDB.Used = true;
            _tokenRepository.Update(refreshTokenDB);

            // gera um novo Token
            var usuario = _usuarioRepository.Get(refreshTokenDB.UserId);
            return Ok(GetUserToken(usuario));

        }

        [HttpPost("")]
        public ActionResult Add([FromBody] UserDTO usuarioDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = new ApplicationUser();
                usuario.FullName = usuarioDTO.Name;
                usuario.UserName = usuarioDTO.Email;
                usuario.Email = usuarioDTO.Email;

                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Password).Result;

                if (!resultado.Succeeded)
                {
                    List<string> erros = new List<string>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    return Ok(usuario);
                }

            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        private TokenDTO BuildToken(ApplicationUser usuario)
        {
            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Aud, "www.meuapp.com.br")
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("key-api-jwt-minhas-tarefas-lucas")); // Recomendad criar no appsettings.json
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var exp = DateTime.UtcNow.AddHours(1);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null, // quem está emitindo o token
                audience: null, // pra quem esta sendo gerado ( normalmente um site ou app)
                claims: claims,
                expires: exp,
                signingCredentials: sign

            );

            var stringToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Guid.NewGuid().ToString();
            var expRefreshToken = DateTime.UtcNow.AddHours(2);

            var tokenDTO = new TokenDTO { Token = stringToken, Expiration = exp, ExpirationRefreshToken = expRefreshToken, RefreshToken = refreshToken };
            return tokenDTO;

        }

        private TokenDTO GetUserToken(ApplicationUser usuario)
        {
            //retorna o Token (JWT)
            var token = BuildToken(usuario);

            //salvar o token na base
            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                ExpitarionToken = token.Expiration,
                User = usuario,
                CreatedAt = DateTime.Now,
                Used = false
            };
            _tokenRepository.Add(tokenModel);
            return token;
        }
    }
}
