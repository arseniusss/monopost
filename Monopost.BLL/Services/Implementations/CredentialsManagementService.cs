using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Enums;
using Sprache;
using Monopost.BLL.Services.Interfaces;
using Result = Monopost.BLL.Models.Result;

namespace Monopost.BLL.Services
{
    public class CredentialManagementService : ICredentialManagementService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;

        public CredentialManagementService(ICredentialRepository credentialRepository, IUserRepository userRepository)
        {
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<CredentialModel>> AddCredentialAsync(CredentialModel model)
        {
            if (!model.StoredLocally && string.IsNullOrWhiteSpace(model.CredentialValue))
            {
                return new Result<CredentialModel>(false, "Non-Empty CredentialValue is required for non locally stored credentials.");
            }

            if(model.StoredLocally && string.IsNullOrWhiteSpace(model.LocalPath))
            {
                return new Result<CredentialModel>(false, "LocalPath is required for locally stored credentials.");
            }

            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                return new Result<CredentialModel>(false, "Invalid AuthorId: User does not exist.");
            }
            try
            {
                await _credentialRepository.GetByIdAsync(model.Id);
                return new Result<CredentialModel>(false, "Id is already taken.");

            }
            catch { };

            var existingUserCredentials = await _credentialRepository.GetByUserIdAsync(model.AuthorId);
            var credentialOfSameType = existingUserCredentials.Any(c => c.CredentialType == model.CredentialType);

            if (credentialOfSameType)
            {
                return new Result<CredentialModel>(false, $"A credential of type '{model.CredentialType}' already exists for this user.");
            }

            var credential = new Credential
            {
                AuthorId = model.AuthorId,
                CredentialType = model.CredentialType,
                CredentialValue = model.CredentialValue,
                StoredLocally = model.StoredLocally,
                LocalPath = model.LocalPath
            };

            await _credentialRepository.AddAsync(credential);

            return new Result<CredentialModel>(true, "Credential created successfully.", model);
        }

        public async Task<Result<CredentialModel>> GetCredentialByIdAsync(int id)
        {
            try
            {
                var credential = await _credentialRepository.GetByIdAsync(id);
                var credentialDto = new CredentialModel
                {
                    Id = credential.Id,
                    AuthorId = credential.AuthorId,
                    CredentialType = credential.CredentialType,
                    CredentialValue = credential.CredentialValue,
                    StoredLocally = credential.StoredLocally,
                    LocalPath = credential.LocalPath
                };
                return new Result<CredentialModel>(true, "Credential retrieved successfully.", credentialDto);
            }
            catch
            {
                return new Result<CredentialModel>(false, "Credential not found.");
            }
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetAllCredentialsAsync()
        {
            var credentials = await _credentialRepository.GetAllAsync();
            var credentialDtos = new List<CredentialModel>();

            foreach (var credential in credentials)
            {
                credentialDtos.Add(new CredentialModel
                {
                    Id = credential.Id,
                    AuthorId = credential.AuthorId,
                    CredentialType = credential.CredentialType,
                    CredentialValue = credential.CredentialValue,
                    StoredLocally = credential.StoredLocally,
                    LocalPath = credential.LocalPath
                });
            }

            return new Result<IEnumerable<CredentialModel>>(true, "Credentials retrieved successfully.", credentialDtos);
        }

        public async Task<Result<CredentialModel>> UpdateCredentialAsync(CredentialModel model)
        {
            if (!model.StoredLocally && string.IsNullOrWhiteSpace(model.CredentialValue))
            {
                return new Result<CredentialModel>(false, "Non-Empty CredentialValue is required for non locally stored credentials.");
            }

            if (model.StoredLocally && string.IsNullOrWhiteSpace(model.LocalPath))
            {
                return new Result<CredentialModel>(false, "LocalPath is required for locally stored credentials.");
            }

            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                return new Result<CredentialModel>(false, "Invalid AuthorId: User does not exist.");
            }
            
            try
            {
                await _credentialRepository.GetByIdAsync(model.Id);
            }
            catch
            {
                return new Result<CredentialModel>(false, "Credential with such id doesnt exist.");

            }
          
            if (model == null)
            {
                return new Result<CredentialModel>(false, "Credential data is required.");
            }

            var existingUserCredentials = await _credentialRepository.GetByUserIdAsync(model.AuthorId);
            var credentialOfSameType = existingUserCredentials.Any(c => c.CredentialType == model.CredentialType && c.Id!=model.Id);

            if (credentialOfSameType)
            {
                return new Result<CredentialModel>(false, $"A credential of type '{model.CredentialType}' already exists for this user.");
            }

            var credential = new Credential
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                CredentialType = model.CredentialType,
                CredentialValue = model.CredentialValue,
                StoredLocally = model.StoredLocally,
                LocalPath = model.LocalPath
            };

            await _credentialRepository.UpdateAsync(credential);
            return new Result<CredentialModel>(true, "Credential updated successfully.", model);
        }

        public async Task<Result> DeleteCredentialAsync(int id)
        {
            try
            {
                await _credentialRepository.GetByIdAsync(id);
            }
            catch
            {
                return new Result(false, $"Credential with id {id} not found.");
            }

            await _credentialRepository.DeleteAsync(id);
            return new Result(true, "Credential deleted successfully.");
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetByTypeAsync(CredentialType credentialType)
        {
            var credentials = await _credentialRepository.GetByTypeAsync(credentialType);
            var credentialDtos = new List<CredentialModel>();

            foreach (var credential in credentials)
            {
                credentialDtos.Add(new CredentialModel
                {
                    Id = credential.Id,
                    AuthorId = credential.AuthorId,
                    CredentialType = credential.CredentialType,
                    CredentialValue = credential.CredentialValue,
                    StoredLocally = credential.StoredLocally,
                    LocalPath = credential.LocalPath
                });
            }

            return new Result<IEnumerable<CredentialModel>>(true, "Credentials retrieved successfully.", credentialDtos);
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetCredentialsByUserIdAsync(int userId)
        {
            var userExists = await _userRepository.UserExistsAsync(userId);
            if (!userExists)
            {
                return new Result<IEnumerable<CredentialModel>>(false, "Invalid AuthorId: User does not exist.");
            }

            var credentials = await _credentialRepository.GetByUserIdAsync(userId);

            if (credentials == null || !credentials.Any())
            {
                return new Result<IEnumerable<CredentialModel>>(false, "No credentials found for the specified user.");
            }

            var credentialDtos = credentials.Select(credential => new CredentialModel
            {
                Id = credential.Id,
                AuthorId = credential.AuthorId,
                CredentialType = credential.CredentialType,
                CredentialValue = credential.CredentialValue,
                StoredLocally = credential.StoredLocally,
                LocalPath = credential.LocalPath
            }).ToList();

            return new Result<IEnumerable<CredentialModel>>(true, "User credentials retrieved successfully.", credentialDtos);
        }
    }
}