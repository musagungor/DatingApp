using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repository, IConfiguration config)
        {
            this._config = config;
            this._repository = repository;

        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserForRegisterDto userForRegisterDto)
        {
            //TODO : validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repository.UserExistsAsync(userForRegisterDto.Username))
            {
                return BadRequest("Username allready exists");
            }

            var userToCreate = new User
            {
                UserName = userForRegisterDto.Username
            };

            var createdUser = await _repository.RegisterAsync(userToCreate, userForRegisterDto.Password);


            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repository.LoginAsync(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject=new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials=creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token)});




        }


    }
}