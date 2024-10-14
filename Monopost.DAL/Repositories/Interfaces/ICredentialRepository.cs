using Monopost.DAL.Entities;
using Monopost.DAL.Enums;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface ICredentialRepository : IRepository<Credential>
    {
        Task<IEnumerable<Credential>> GetByTypeAsync(CredentialType credentialType);
        Task<IEnumerable<Credential>> GetStoredLocallyAsync();
        Task<IEnumerable<Credential>> GetByUserIdAsync(int userId);
    }
}