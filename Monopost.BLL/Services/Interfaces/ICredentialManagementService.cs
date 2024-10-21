using Monopost.BLL.Models;
using Monopost.DAL.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monopost.BLL.Services.Interfaces
{
    public interface ICredentialManagementService
    {
        Task<Result<CredentialModel>> AddCredentialAsync(CredentialModel model);
        Task<Result<CredentialModel>> GetCredentialByIdAsync(int id);
        Task<Result<IEnumerable<CredentialModel>>> GetAllCredentialsAsync();
        Task<Result<CredentialModel>> UpdateCredentialAsync(CredentialModel credentialDto);
        Task<Result> DeleteCredentialAsync(int id);
        Task<Result<IEnumerable<CredentialModel>>> GetByTypeAsync(CredentialType credentialType);
        Task<Result<IEnumerable<CredentialModel>>> GetCredentialsByUserIdAsync(int userId);
        Task<Result<IEnumerable<DecodedCredential>>> GetDecodedCredentialsByUserIdAsync(int userId);
    }
}