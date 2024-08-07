using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
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
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }
        // [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTOs>>> GetUsers()
        {
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
        public async Task<ActionResult<MemberDTOs>> GetUserByUserName(string username)
        {
            var user = await _userRepository.GetMemberAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        [HttpPut]
        public async Task<ActionResult<MemberDTOs>> UpdateUser(MemberUpdateDto memberUpdateDTO)
        {
            var username = User.GetUserName();
            var user = await _userRepository.GetUserByUserName(username);

            if (user == null) return NotFound();
            _mapper.Map(memberUpdateDTO, user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhotoAsync(IFormFile file)
        {
            var user = await _userRepository.GetUserByUserName(User.GetUserName());
            if (user == null) return NotFound("User not found");
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUserByUserName),
                 new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem saving photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUserName(User.GetUserName());
            if (user == null) return NotFound("User not found");
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound("Photo not found");
            if (photo.IsMain) return BadRequest("This is already the main photo");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            {
                var user = await _userRepository.GetUserByUserName(User.GetUserName());
                var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
                if (photo == null) return NotFound();
                if (photo.IsMain) return BadRequest("You cannot delete your main photo");
                if (photo.PublicId != null)
                {
                    var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                    if (result.Error != null) return BadRequest(result.Error.Message);
                }
                user.Photos.Remove(photo);
                if (await _userRepository.SaveAllAsync()) return Ok();
                return BadRequest("Failed to delete photo");
            }
        }
    }
}