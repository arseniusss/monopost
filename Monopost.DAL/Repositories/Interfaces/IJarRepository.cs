using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;

public interface IJarRepository : IRepository<Jar>
{
    Task<IEnumerable<Jar>> GetByOwnerIdAsync(int ownerId);
}