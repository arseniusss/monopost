using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Models;
using Monopost.BLL.Services;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Implementations;


namespace Monopost.UnitTests.Services
{
    public class CredentialManagementServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly CredentialManagementService _credentialManagementService;
        private readonly UserRepository _userRepository;
        private readonly CredentialRepository _credentialRepository;

        public CredentialManagementServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();

            _userRepository = new UserRepository(_dbContext);
            _credentialRepository = new CredentialRepository(_dbContext);
            _credentialManagementService = new CredentialManagementService(_credentialRepository, _userRepository);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task AddCredentialAsync_ValidCredential_ReturnsSuccess()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credentialModel = new CredentialModel
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.MonobankAPIToken,
                CredentialValue = "value",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(credentialModel);

            Assert.True(result.Success);
            Assert.Equal("Credential created successfully.", result.Message);
        }

        [Fact]
        public async Task AddCredentialAsync_NonEmptyCredentialValueRequired_ReturnsFailure()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credentialModel = new CredentialModel
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = null,
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(credentialModel);

            Assert.False(result.Success);
            Assert.Equal("Non-Empty CredentialValue is required for non locally stored credentials.", result.Message);
        }

        [Fact]
        public async Task AddCredentialAsync_LocalPathRequiredForLocallyStored_ReturnsFailure()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credentialModel = new CredentialModel
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.FacebookAccessToken,
                CredentialValue = "someValue",
                StoredLocally = true,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(credentialModel);

            Assert.False(result.Success);
            Assert.Equal("LocalPath is required for locally stored credentials.", result.Message);
        }

        [Fact]
        public async Task AddCredentialAsync_InvalidAuthorId_ReturnsFailure()
        {
            var credentialModel = new CredentialModel
            {
                Id = 1,
                AuthorId = 99,
                CredentialType = CredentialType.FacebookPageID,
                CredentialValue = "someValue",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(credentialModel);

            Assert.False(result.Success);
            Assert.Equal("Invalid AuthorId: User does not exist.", result.Message);
        }

        [Fact]
        public async Task AddCredentialAsync_IdTaken_ReturnsFailure()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var existingCredential = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.MonobankAPIToken,
                CredentialValue = "existingValue",
                StoredLocally = false,
                LocalPath = null
            };
            await _credentialRepository.AddAsync(existingCredential);
            await _dbContext.SaveChangesAsync();

            var credentialModel = new CredentialModel
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "newValue",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(credentialModel);

            Assert.False(result.Success);
            Assert.Equal("Id is already taken.", result.Message);
        }

        [Fact]
        public async Task AddCredentialAsync_CredentialOfSameTypeExists_ReturnsFailure()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var existingCredential = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "existingValue",
                StoredLocally = false,
                LocalPath = null
            };
            await _credentialRepository.AddAsync(existingCredential);
            await _dbContext.SaveChangesAsync();

            var newCredentialModel = new CredentialModel
            {
                Id = 2,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "newValue",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.AddCredentialAsync(newCredentialModel);

            Assert.False(result.Success);
            Assert.Equal("A credential of type 'TelegramAppID' already exists for this user.", result.Message);
        }

        [Fact]
        public async Task GetCredentialByIdAsync_ValidId_ReturnsCredential()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.MonobankAPIToken,
                CredentialValue = "value",
                StoredLocally = false,
                LocalPath = null
            };
            await _credentialRepository.AddAsync(credential);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.GetCredentialByIdAsync(1);

            Assert.True(result.Success);
            Assert.Equal("Credential retrieved successfully.", result.Message);
            Assert.Equal(credential.CredentialType, result.Data?.CredentialType);
        }

        [Fact]
        public async Task GetCredentialByIdAsync_InvalidId_ReturnsFailure()
        {
            var result = await _credentialManagementService.GetCredentialByIdAsync(99);

            Assert.False(result.Success);
            Assert.Equal("Credential not found.", result.Message);
        }

        [Fact]
        public async Task UpdateCredentialAsync_ValidUpdate_ReturnsSuccess()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var existingCredential = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "existingValue",
                StoredLocally = false,
                LocalPath = null
            };
            await _credentialRepository.AddAsync(existingCredential);
            await _dbContext.SaveChangesAsync();

            var updateModel = new CredentialModel
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "newValue",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.UpdateCredentialAsync(updateModel);

            Assert.True(result.Success);
            Assert.Equal("Credential updated successfully.", result.Message);
            var updatedCredential = await _credentialRepository.GetByIdAsync(1);
            Assert.Equal("newValue", updatedCredential.CredentialValue);
        }

        [Fact]
        public async Task UpdateCredentialAsync_InvalidId_ReturnsFailure()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var updateModel = new CredentialModel
            {
                Id = 99,
                AuthorId = 1,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "Invalid AuthorId: User does not exist.",
                StoredLocally = false,
                LocalPath = null
            };

            var result = await _credentialManagementService.UpdateCredentialAsync(updateModel);

            Assert.False(result.Success);
            Assert.Equal("Credential with such id doesnt exist.", result.Message);
        }

        [Fact]
        public async Task DeleteCredentialAsync_ValidId_ReturnsSuccess()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.MonobankAPIToken,
                CredentialValue = "value",
                StoredLocally = false,
                LocalPath = null
            };
            await _credentialRepository.AddAsync(credential);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.DeleteCredentialAsync(1);

            Assert.True(result.Success);
            Assert.Equal("Credential deleted successfully.", result.Message);
            var deletedCredential = await _credentialRepository.GetByIdAsync(1);
            Assert.Null(deletedCredential);
        }

        [Fact]
        public async Task DeleteCredentialAsync_InvalidId_ReturnsFailure()
        {
            var result = await _credentialManagementService.DeleteCredentialAsync(99);

            Assert.False(result.Success);
            Assert.Equal("Credential not found.", result.Message);
        }

        [Fact]
        public async Task GetAllCredentialsAsync_ShouldReturnAllCredentials_WhenCredentialsExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential1 = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "someValue1",
                StoredLocally = false,
                LocalPath = null
            };

            var credential2 = new Credential
            {
                Id = 2,
                AuthorId = user.Id,
                CredentialType = CredentialType.FacebookAccessToken,
                CredentialValue = "someValue2",
                StoredLocally = true,
                LocalPath = "path/to/credential"
            };

            await _dbContext.Credentials.AddRangeAsync(credential1, credential2);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.GetAllCredentialsAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            Assert.Equal("Credentials retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetCredentialsByUserIdAsync_ShouldReturnUserCredentials_WhenCredentialsExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential1 = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "someValue1",
                StoredLocally = false,
                LocalPath = null
            };

            var credential2 = new Credential
            {
                Id = 2,
                AuthorId = user.Id,
                CredentialType = CredentialType.FacebookAccessToken,
                CredentialValue = "someValue2",
                StoredLocally = true,
                LocalPath = "path/to/credential"
            };

            await _dbContext.Credentials.AddRangeAsync(credential1, credential2);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.GetCredentialsByUserIdAsync(user.Id);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            Assert.Equal("User credentials retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetCredentialsByUserIdAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            var result = await _credentialManagementService.GetCredentialsByUserIdAsync(999);

            Assert.False(result.Success);
            Assert.Equal("Invalid AuthorId: User does not exist.", result.Message);
        }

        [Fact]
        public async Task GetCredentialsByUserIdAsync_ShouldReturnFailure_WhenNoCredentialsExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.GetCredentialsByUserIdAsync(user.Id);

            Assert.False(result.Success);
            Assert.Equal("No credentials found for the specified user.", result.Message);
        }

        [Fact]
        public async Task GetByTypeAsync_ShouldReturnCredentialsByType_WhenCredentialsExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential1 = new Credential
            {
                Id = 1,
                AuthorId = user.Id,
                CredentialType = CredentialType.TelegramAppID,
                CredentialValue = "someValue1",
                StoredLocally = false,
                LocalPath = null
            };

            var credential2 = new Credential
            {
                Id = 2,
                AuthorId = user.Id,
                CredentialType = CredentialType.FacebookAccessToken,
                CredentialValue = "someValue2",
                StoredLocally = true,
                LocalPath = "path/to/credential"
            };

            await _dbContext.Credentials.AddRangeAsync(credential1, credential2);
            await _dbContext.SaveChangesAsync();

            var result = await _credentialManagementService.GetByTypeAsync(CredentialType.TelegramAppID);

            Assert.True(result.Success);
            IEnumerable<CredentialModel>? data = result.Data;
            Assert.Single(data);
            Assert.Equal("Credentials retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetByTypeAsync_ShouldReturnFailure_WhenNoCredentialsExistForType()
        {
            var result = await _credentialManagementService.GetByTypeAsync(CredentialType.TelegramAppID);

            Assert.True(result.Success);
            Assert.Empty(result.Data);
        }
    }
}