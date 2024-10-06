using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface IRestrictionRepository : IRepository<Restriction>
    {
        Task<IEnumerable<Restriction>> GetRestrictionsByUserIdAsync(int userId);
    }
}
