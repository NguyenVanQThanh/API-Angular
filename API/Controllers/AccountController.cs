
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenServices)
        {
            _context = context;
            _tokenService = tokenServices;
        }
        [HttpPost("register")] 
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username)){
                return BadRequest("Username already exists");
            }
            using var hmac = new HMACSHA512();
            var user = new AppUser{
                userName = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
            return Ok(new UserDTO{
                Username = user.userName,
                Token = _tokenService.CreateToken(user)
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){
            var user = await _context.Users.SingleOrDefaultAsync(x =>
                 x.userName == loginDTO.Username);
            if (user == null) {
                return Unauthorized(); 
            }
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for (int i = 0; i < computedHash.Length; i++){
                if (computedHash[i]!= user.PasswordHash[i]){
                    return Unauthorized("Invalid password");
                }
            }
            return Ok(new UserDTO{
                Username = user.userName,
                Token = _tokenService.CreateToken(user)
            });
        }
        private async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(x => x.userName.ToLower() == username.ToLower());
        }
    }
}