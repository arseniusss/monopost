using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Enums;
using Sprache;
using Monopost.BLL.Services.Interfaces;
using Result = Monopost.BLL.Models.Result;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.Services
{
    public class CredentialManagementService : ICredentialManagementService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public CredentialManagementService(ICredentialRepository credentialRepository, IUserRepository userRepository)
        {
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            logger.Information($"Credential management service created");
        }

        public async Task<Result<CredentialModel>> AddCredentialAsync(CredentialModel model)
        {
            logger.Information($"Adding credential: {model}");
            if (!model.StoredLocally && string.IsNullOrWhiteSpace(model.CredentialValue))
            {
                logger.Warning($"Result: Failure\nReason: Non-Empty CredentialValue is required for non locally stored credentials.");
                return new Result<CredentialModel>(false, "Non-Empty CredentialValue is required for non locally stored credentials.");
            }

            if(model.StoredLocally && string.IsNullOrWhiteSpace(model.LocalPath))
            {
                logger.Warning($"Result: Failure\nReason: LocalPath is required for locally stored credentials.");
                return new Result<CredentialModel>(false, "LocalPath is required for locally stored credentials.");
            }

            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                logger.Warning($"Result: Failure\nReason: Invalid AuthorId: User does not exist.");
                return new Result<CredentialModel>(false, "Invalid AuthorId: User does not exist.");
            }
            try
            {
                await _credentialRepository.GetByIdAsync(model.Id);
                logger.Warning($"Result: Failure\nReason: Id is already taken.");
                return new Result<CredentialModel>(false, "Id is already taken.");
            }
            catch { };

            var existingUserCredentials = await _credentialRepository.GetByUserIdAsync(model.AuthorId);
            var credentialOfSameType = existingUserCredentials.Any(c => c.CredentialType == model.CredentialType);

            if (credentialOfSameType)
            {
                logger.Warning($"Result: Failure\nReason: A credential of type '{model.CredentialType}' already exists for this user.");
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
            logger.Information($"Result: Success");
            return new Result<CredentialModel>(true, "Credential created successfully.", model);
        }

        public async Task<Result<CredentialModel>> GetCredentialByIdAsync(int id)
        {
            logger.Information($"Trying to get credential by id = : {id}");
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
                logger.Information($"Result: Success");
                return new Result<CredentialModel>(true, "Credential retrieved successfully.", credentialDto);
            }
            catch
            {
                logger.Warning($"Result: Failure\nReason: Credential not found in db");
                return new Result<CredentialModel>(false, "Credential not found.");
            }
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetAllCredentialsAsync()
        {
            logger.Information($"Trying to get all credentials from repo...");
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
            logger.Information($"Result:Success\nMessage: Credentials retrieved successfully.");
            return new Result<IEnumerable<CredentialModel>>(true, "Credentials retrieved successfully.", credentialDtos);
        }

        public async Task<Result<CredentialModel>> UpdateCredentialAsync(CredentialModel model)
        {
            logger.Information($"Trying to update credentials. New credentials: {model}");
            if (!model.StoredLocally && string.IsNullOrWhiteSpace(model.CredentialValue))
            {
                logger.Warning($"Result: Failure\nReason: Non-Empty CredentialValue is required for non locally stored credentials.");
                return new Result<CredentialModel>(false, "Non-Empty CredentialValue is required for non locally stored credentials.");
            }

            if (model.StoredLocally && string.IsNullOrWhiteSpace(model.LocalPath))
            {
                logger.Warning($"Result: Failure\nReason: LocalPath is required for locally stored credentials.");
                return new Result<CredentialModel>(false, "LocalPath is required for locally stored credentials.");
            }

            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                logger.Warning($"Result: Failure\nReason: Invalid AuthorId: User does not exist.");
                return new Result<CredentialModel>(false, "Invalid AuthorId: User does not exist.");
            }
            
            try
            {
                await _credentialRepository.GetByIdAsync(model.Id);
            }
            catch
            {
                logger.Warning($"Result: Failure\nReason: Credential with such id doesnt exist.");
                return new Result<CredentialModel>(false, "Credential with such id doesnt exist.");
            }
          
            if (model == null)
            {
                logger.Warning($"Result: Failure\nReason: Credential data is required.");
                return new Result<CredentialModel>(false, "Credential data is required.");
            }

            var existingUserCredentials = await _credentialRepository.GetByUserIdAsync(model.AuthorId);
            var credentialOfSameType = existingUserCredentials.Any(c => c.CredentialType == model.CredentialType && c.Id!=model.Id);

            if (credentialOfSameType)
            {
                logger.Warning($"Result: Failure\nReason: A credential of type '{model.CredentialType}' already exists for this user.");
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
            logger.Information($"Result: Success.  Message: Credential {model} updated successfully.");
            return new Result<CredentialModel>(true, "Credential updated successfully.", model);
        }

        public async Task<Result> DeleteCredentialAsync(int id)
        {
            logger.Information($"Trying to delete credential with id = {id}");
            try
            {
                await _credentialRepository.GetByIdAsync(id);
            }
            catch
            {
                logger.Warning($"Result: Failure\nReason: Credential with id {id} not found.");
                return new Result(false, $"Credential with id {id} not found.");
            }

            await _credentialRepository.DeleteAsync(id);
            logger.Information($"Result: Success\nMessage: Credential with id = {id} deleted successfully");
            return new Result(true, "Credential deleted successfully.");
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetByTypeAsync(CredentialType credentialType)
        {
            logger.Information($"Trying to get all credentials by type = {credentialType}");
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
            logger.Information($"All credentials of type = {credentialType} were retrieved successfully.");
            return new Result<IEnumerable<CredentialModel>>(true, "Credentials retrieved successfully.", credentialDtos);
        }

        public async Task<Result<IEnumerable<CredentialModel>>> GetCredentialsByUserIdAsync(int userId)
        {
            logger.Information($"Trying to get all credentials of user with id = {userId}");
            var userExists = await _userRepository.UserExistsAsync(userId);
            if (!userExists)
            {
                logger.Warning($"Result:Failure\nReason: Invalid AuthorId: User does not exist.");
                return new Result<IEnumerable<CredentialModel>>(false, "Invalid AuthorId: User does not exist.");
            }

            var credentials = await _credentialRepository.GetByUserIdAsync(userId);

            if (credentials == null || !credentials.Any())
            {
                logger.Warning($"Result:Failure\nReason: No credentials found for the specified user.");
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
            logger.Information($"Result:Success\nMessage: User credentials retrieved successfully.");
            return new Result<IEnumerable<CredentialModel>>(true, "User credentials retrieved successfully.", credentialDtos);
        }
    }
}