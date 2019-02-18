using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context )
        {
            this._context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhotoAsync(int id)
        {
           var photo = await _context.Photos.FirstOrDefaultAsync(p=> p.Id == id);

           return photo;
        }

          public async Task<Photo> GetMainPhotoForUserAsync(int userId)
        {
           var mainPhoto = await _context.Photos.Where(u=>u.UserId==userId).FirstOrDefaultAsync(p=>p.IsMain);

           return mainPhoto;
        }

        public async Task<User> GetUserAsync(int id)
        {
            var user= await _context.Users.Include(p=>p.Photos).FirstOrDefaultAsync(u=>u.Id==id);

            return user;
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParams userParams)
        {
            // var  users = await _context.Users.Include(p=>p.Photos).ToListAsync();
            var  users =  _context.Users.Include(p=>p.Photos)
                            .OrderByDescending(u=>u.LastActive).AsQueryable();

            users = users.Where(u=>u.Id != userParams.UserId);
            users = users.Where(u=>u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(u=> userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId,userParams.Likers);
                users = users.Where(u=> userLikees.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDateOfBirth =  DateTime.Today.AddYears(-userParams.MaxAge);
                var maxDateOfBirth =  DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u=> u.DateOfBirth >= minDateOfBirth &&  u.DateOfBirth <= maxDateOfBirth );
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {       
                    case "created":
                        users = users.OrderByDescending(u=>u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u=>u.LastActive);
                        break;
                }
                
            }

            

            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {

            var user = await _context.Users
            .Include(x => x.Likers)
            .Include(x => x.Likees)
            .FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);

            }

        }
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u=> u.LikerId==userId && u.LikeeId == recipientId);


        }
    }
}