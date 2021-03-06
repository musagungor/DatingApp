using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
           void Add<T> (T entity) where T: class;
           void Delete<T>(T entity) where T: class;
           Task<bool> SaveAllAsync();
           Task<PagedList<User>> GetUsersAsync(UserParams userParams);
           Task<User> GetUserAsync(int id);
           Task<Photo> GetPhotoAsync(int id);
           Task<Photo> GetMainPhotoForUserAsync(int userId);

           Task<Like> GetLike (int userId, int recipientId);

           Task<Message> GetMessage(int id);
           Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
           Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}