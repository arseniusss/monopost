using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface ICredentialRepository : IRepository<Credential>
    {
        Task<IEnumerable<Credential>> GetByTypeAsync(string credentialType);
        Task<IEnumerable<Credential>> GetStoredLocallyAsync();
    }
}