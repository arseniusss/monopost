using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Implementations;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Implementations;
using Monopost.Logging;
using Serilog;
using Xunit;

namespace Monopost.UnitTests.Services
{
    public class DataDeletionServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly DataDeletionService _dataDeletionService;
        private readonly UserRepository _userRepository;
        private readonly CredentialRepository _credentialRepository;
        private readonly TemplateRepository _templateRepository;
        private readonly TemplateFileRepository _templateFileRepository;
        private readonly PostRepository _postRepository;
        private readonly PostMediaRepository _postMediaRepository;

        public DataDeletionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            LoggerConfig.ConfigureLogging();

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();

            _userRepository = new UserRepository(_dbContext);
            _credentialRepository = new CredentialRepository(_dbContext);
            _templateRepository = new TemplateRepository(_dbContext);
            _templateFileRepository = new TemplateFileRepository(_dbContext);
            _postRepository = new PostRepository(_dbContext);
            _postMediaRepository = new PostMediaRepository(_dbContext);

            _dataDeletionService = new DataDeletionService(
                _userRepository,
                _credentialRepository,
                _templateRepository,
                _templateFileRepository,
                _postRepository,
                _postMediaRepository
            );
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            Log.CloseAndFlush();
        }

        [Fact]
        public async Task DeleteData_UserNotFound_ReturnsFailure()
        {
            var result = await _dataDeletionService.DeleteData(999);

            Assert.False(result.Success);
            Assert.Equal("User with such Id does not exist.", result.Message);
        }

        [Fact]
        public async Task DeleteData_DeleteCredentials_Success()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.MonobankAPIToken, CredentialValue = "value" };
            await _credentialRepository.AddAsync(credential);
            await _dbContext.SaveChangesAsync();

            var result = await _dataDeletionService.DeleteData(user.Id, credentials: true);

            Assert.True(result.Success);
            Assert.Equal("User's data deleted successfully.", result.Message);
            Assert.Empty(await _credentialRepository.GetByUserIdAsync(user.Id));
        }

        [Fact]
        public async Task DeleteData_DeleteTemplates_Success()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var template = new Template { Id = 1, Name = "Template 1", Text = "This is a test template.", AuthorId = user.Id };
            await _templateRepository.AddAsync(template);
            await _dbContext.SaveChangesAsync();

            var result = await _dataDeletionService.DeleteData(user.Id, templates: true);

            Assert.True(result.Success);
            Assert.Equal("User's data deleted successfully.", result.Message);
            Assert.Empty(await _templateRepository.GetTemplatesByAuthorIdAsync(user.Id));
        }

        [Fact]
        public async Task DeleteData_DeletePosts_Success()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var post = new Post { PostId = 1, AuthorId = user.Id, DatePosted = DateTime.Now };
            await _postRepository.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            var result = await _dataDeletionService.DeleteData(user.Id, posts: true);

            Assert.True(result.Success);
            Assert.Equal("User's data deleted successfully.", result.Message);
            Assert.Empty(await _postRepository.GetPostsByAuthorIdAsync(user.Id));
        }

        [Fact]
        public async Task DeleteData_TotalAccountDeletion_Success()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _dataDeletionService.DeleteData(user.Id, totalAccountDeletion: true);

            Assert.True(result.Success);
            Assert.Equal("User's data deleted successfully.", result.Message);
            Assert.Null(await _userRepository.GetByIdAsync(user.Id));
        }

        [Fact]
        public async Task DeleteData_DeleteAllData_Success()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.MonobankAPIToken, CredentialValue = "value" };
            await _credentialRepository.AddAsync(credential);
            await _dbContext.SaveChangesAsync();

            var template = new Template { Id = 1, Name = "Template 1", Text = "This is a test template.", AuthorId = user.Id };
            await _templateRepository.AddAsync(template);
            await _dbContext.SaveChangesAsync();

            var post = new Post { PostId = 1, AuthorId = user.Id, DatePosted = DateTime.Now };
            await _postRepository.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            var result = await _dataDeletionService.DeleteData(user.Id, credentials: true, templates: true, posts: true, totalAccountDeletion: true);

            Assert.True(result.Success);
            Assert.Equal("User's data deleted successfully.", result.Message);
            Assert.Null(await _userRepository.GetByIdAsync(user.Id));
            Assert.Empty(await _credentialRepository.GetByUserIdAsync(user.Id));
            Assert.Empty(await _templateRepository.GetTemplatesByAuthorIdAsync(user.Id));
            Assert.Empty(await _postRepository.GetPostsByAuthorIdAsync(user.Id));
        }
    }
}
