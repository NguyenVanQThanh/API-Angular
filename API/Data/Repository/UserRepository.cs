using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            return await _context.Users.Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<AppUser> GetByIdAsync(int id)
        {
             return await _context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<MemberDTOs> GetMemberAsync(string username)
        {
            var user = await _context.Users
                        .Where(u => u.UserName == username)
                        .ProjectTo<MemberDTOs>(_mapper.ConfigurationProvider)
                        // .Select(user => new MemberDTOs{
                        //     Id = user.Id,
                        //     UserName = user.UserName,
                        //     KnownAs = user.KnownAs,
                        //     Age = user.DateOfBirth.CalculateAge(),
                        //     Created = user.Created,
                        //     LastActive = user.LastActive,
                        //     Gender = user.Gender
                        // })
                        .SingleOrDefaultAsync();
            return user;
        }

        public async Task<IEnumerable<MemberDTOs>> GetMembersAsync()
        {
            return await _context.Users
                    .ProjectTo<MemberDTOs>(_mapper.ConfigurationProvider)
                    .ToArrayAsync();
        }

        public async Task<AppUser> GetUserByUserName(string userName)
        {
            return await _context.Users.Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName == userName);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}