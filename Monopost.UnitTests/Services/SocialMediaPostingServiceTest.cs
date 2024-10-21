using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Models;
using Monopost.BLL.Services;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Implementations;
using Monopost.Logging;
using Serilog;
using Xunit;

namespace Monopost.UnitTests.Services
{
    public class SocialMediaPostingServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private SocialMediaPostingService _socialMediaPostingService;
        private readonly CredentialRepository _credentialRepository;
        private readonly UserRepository _userRepository;
        private readonly PostRepository _postRepository;
        private readonly PostMediaRepository _postMediaRepository;

        private const int UserId = 1;

        public SocialMediaPostingServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            LoggerConfig.ConfigureLogging();

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();

            _credentialRepository = new CredentialRepository(_dbContext);
            _userRepository = new UserRepository(_dbContext);
            _postRepository = new PostRepository(_dbContext);
            _postMediaRepository = new PostMediaRepository(_dbContext);
            _socialMediaPostingService = new SocialMediaPostingService(_credentialRepository, _userRepository, _postRepository, _postMediaRepository, UserId);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            Log.CloseAndFlush();
        }

        [Fact]
        public async Task CreatePostAsync_ShouldPostToSocialMediaSuccessfully()
        {
            await _userRepository.AddAsync(new User
            {
                Id = UserId,
                Email = "testuser",
                Password = "testpassword",
                Age = 18,
                FirstName = "hi",
                LastName = "bye"
            });

            // Arrange
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.InstagramAccessToken,
                CredentialValue = "EAAa9xhnJKfcBOxW4d1STZAZA8LYF0rPHBkX6KT3TnHAB97dlo45FWUEat2EQLAL4b82ZCcsGNGqnFVYNThCaN0WNgZBNJNncPgeXVNOl2zfJivDs1RTYypmPZCgytt9ToavUYjxcfqpmW5NyJ1P37CtywduAeBdpOsYZBPLZBc4N2okJGee6IZCBpqGKJ52eM2RF",
                StoredLocally = false
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.InstagramUserId,
                CredentialValue = "17841459372027912",
                StoredLocally = false
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.ImgbbApiKey,
                CredentialValue = "e0a110edf2286c29a1a29bf2b6a257ad",
                StoredLocally = false

            });
           
            // Act
            var result = await _socialMediaPostingService.CreatePostAsync("Hello World", new List<string> { "D:\\1.jpg", "D:\\2.jpg" });
            // Assert
            Assert.True(result.Success);
            Assert.Equal("Messages posted successfully", result.Message);

            var posts = await _postRepository.GetPostsByAuthorIdAsync(UserId);
            Assert.NotEmpty(posts);

            var latestPost = posts.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            Assert.NotNull(latestPost);

            var postMediaEntries = await _postMediaRepository.GetPostMediaByPostIdAsync(latestPost.PostId);
            Assert.NotEmpty(postMediaEntries);
        }

        [Fact]
        public async Task CreatePostAsync_NoSocialMediaPosters_ShouldReturnError()
        {
            // Act
            var result = await _socialMediaPostingService.CreatePostAsync("Hello World", new List<string>());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No social media posters found", result.Message);
        }

        [Fact]
        public async Task CreatePostAsync_WithNoCredentials_ShouldReturnError()
        {
            // Arrange: Ensure no credentials exist
            await _credentialRepository.DeleteAsync(1);
            await _credentialRepository.DeleteAsync(5);
            // Act
            var result = await _socialMediaPostingService.CreatePostAsync("Hello World", new List<string>());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No social media posters found", result.Message);
        }
    }
}
