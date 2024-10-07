using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface IJarRepository : IRepository<Jar>
    {
        Task<IEnumerable<Jar>> GetByOwnerIdAsync(int ownerId);
    }
}