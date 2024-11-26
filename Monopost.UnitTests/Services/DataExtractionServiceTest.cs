using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Services.Implementations;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Implementations;
using Monopost.Logging;
using Serilog;

namespace Monopost.UnitTests.Services
{
    public class DataExtractionServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly DataExtractionService _dataExtractionService;
        private readonly UserRepository _userRepository;
        private readonly CredentialRepository _credentialRepository;
        private readonly TemplateRepository _templateRepository;
        private readonly TemplateFileRepository _templateFileRepository;
        private readonly PostRepository _postRepository;
        private readonly PostMediaRepository _postMediaRepository;

        public DataExtractionServiceTests()
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

            _dataExtractionService = new DataExtractionService(
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
        public async Task ExtractData_UserNotFound_ReturnsFailure()
        {
            var result = await _dataExtractionService.ExtractData(999);

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ExtractData_IncludeCredentials_Success()
        {
            var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Password = "password", Age = 25 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.MonobankAPIToken, CredentialValue = "token123" };
            await _credentialRepository.AddAsync(credential);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includeCredentials: true);

            Assert.True(result.Success);
            Assert.Single(result.Data.Credentials);
            Assert.Equal("token123", result.Data.Credentials.First().CredentialValue);
        }

        [Fact]
        public async Task ExtractData_IncludeTemplates_Success()
        {
            var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Password = "password", Age = 25 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var template = new Template { Id = 1, Name = "Template1", Text = "Template Text", AuthorId = user.Id };
            await _templateRepository.AddAsync(template);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includeTemplates: true);

            Assert.True(result.Success);
            Assert.Single(result.Data.Templates);
            Assert.Equal("Template1", result.Data.Templates.First().Name);
        }

        [Fact]
        public async Task ExtractData_IncludePosts_Success()
        {
            var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Password = "password", Age = 25 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var post = new Post { PostId = 1, AuthorId = user.Id, DatePosted = DateTime.UtcNow };
            await _postRepository.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includePosts: true);

            Assert.True(result.Success);
            Assert.Single(result.Data.Posts);
            Assert.Equal(post.DatePosted, result.Data.Posts.First().DatePosted);
        }

        [Fact]
        public async Task ExtractData_IncludeAllData_Success()
        {
            var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Password = "password", Age = 25 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.MonobankAPIToken, CredentialValue = "token123" };
            var template = new Template { Id = 1, Name = "Template1", Text = "Template Text", AuthorId = user.Id };
            var post = new Post { PostId = 1, AuthorId = user.Id, DatePosted = DateTime.UtcNow };

            await _credentialRepository.AddAsync(credential);
            await _templateRepository.AddAsync(template);
            await _postRepository.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includeCredentials: true, includeTemplates: true, includePosts: true);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Credentials);
            Assert.Single(result.Data.Templates);
            Assert.Single(result.Data.Posts);
        }
        [Fact]
        public async Task ExtractData_IncludeCredentialsAndTemplates_Success()
        {
            var user = new User { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", Password = "password", Age = 28 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.MonobankAPIToken, CredentialValue = "token456" };
            var template = new Template { Id = 1, Name = "Template2", Text = "Another Template Text", AuthorId = user.Id };

            await _credentialRepository.AddAsync(credential);
            await _templateRepository.AddAsync(template);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includeCredentials: true, includeTemplates: true);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Credentials);
            Assert.Single(result.Data.Templates);
            Assert.Empty(result.Data.Posts);
            Assert.Equal("token456", result.Data.Credentials.First().CredentialValue);
            Assert.Equal("Template2", result.Data.Templates.First().Name);
        }

        [Fact]
        public async Task ExtractData_IncludeAllParameters_Success()
        {
            var user = new User { Id = 1, FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com", Password = "securepassword", Age = 35 };
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var credential = new Credential { Id = 1, AuthorId = user.Id, CredentialType = CredentialType.InstagramAccessToken, CredentialValue = "twitter-token" };
            var template = new Template { Id = 1, Name = "TweetTemplate", Text = "Tweet Content", AuthorId = user.Id };
            var post = new Post { PostId = 1, AuthorId = user.Id, DatePosted = DateTime.UtcNow };

            await _credentialRepository.AddAsync(credential);
            await _templateRepository.AddAsync(template);
            await _postRepository.AddAsync(post);
            await _dbContext.SaveChangesAsync();

            var result = await _dataExtractionService.ExtractData(user.Id, includeCredentials: true, includeTemplates: true, includePosts: true, totalDataExtraction: false);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Credentials);
            Assert.Single(result.Data.Templates);
            Assert.Single(result.Data.Posts);
            Assert.Equal("twitter-token", result.Data.Credentials.First().CredentialValue);
            Assert.Equal("TweetTemplate", result.Data.Templates.First().Name);
            Assert.Equal(post.DatePosted, result.Data.Posts.First().DatePosted);
        }
    }
}
