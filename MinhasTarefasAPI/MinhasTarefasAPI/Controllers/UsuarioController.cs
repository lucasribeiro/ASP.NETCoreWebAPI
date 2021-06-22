using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories.Contracts;

namespace MinhasTarefasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public UsuarioController(IUsuarioRepository usuarioRepository, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ITokenRepository tokenRepository)
        {
            _usuarioRepository = usuarioRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody]UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");
            
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);
                if (usuario != null)
                {
                    //Login no Identity
                    //_signInManager.SignInAsync(usuario, false);

                    //retorna o Token (JWT)
                    var token = BuildToken(usuario);

                    //salvar o token na base
                    var tokenModel = new Token()
                    {
                        RefreshToken = token.RefreshToken,
                        ExpirationRefreshToken = token.ExpirationRefreshToken,
                        ExpitarionToken = token.Expiration,
                        Usuario = usuario,
                        Criado = DateTime.Now,
                        Utilizado = false
                    };
                    _tokenRepository.Cadastrar(tokenModel);
                    return Ok(BuildToken(usuario));
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

        [HttpPost("")]
        public ActionResult Cadastrar([FromBody]UsuarioDTO usuarioDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = new ApplicationUser();
                usuario.FullName = usuarioDTO.Nome;
                usuario.UserName = usuarioDTO.Email;
                usuario.Email = usuarioDTO.Email;

                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

                if (!resultado.Succeeded){
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

            var tokenDTO = new  TokenDTO { Token = stringToken, Expiration = exp, ExpirationRefreshToken = expRefreshToken, RefreshToken = refreshToken };
            return tokenDTO;

        }
    }
}