using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTOs>>> GetUsers(){
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }
        // [HttpGet("{id}")]
        // public async Task<ActionResult<AppUser>> GetUser(int id){
        //     var user = await _userRepository.GetByIdAsync(id);
        //     if (user == null){
        //         return NotFound();
        //     }
        //     return Ok(user);
        // }
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTOs>> GetUserByUserName(string username){
            var user = await _userRepository.GetMemberAsync(username);
            if (user == null){
                return NotFound();
            }
            return Ok(user);
        }
    }
}