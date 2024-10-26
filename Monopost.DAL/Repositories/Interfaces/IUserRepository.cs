using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> UserExistsAsync(int id);
    }
}
