using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
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

        private int UserId = 1;

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
        public async Task CreatePostAsync_ShouldPostToSocialMediaSuccessfullyWithLocalCreds()
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

            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPhoneNumber,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppID,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppHash,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramChannelId,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPassword,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });

            var result = await _socialMediaPostingService.CreatePostAsync("This should post only to telegram", new List<string>
            { "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
              "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\2.jpg" });

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
        public async Task CreatePostAsync_ShouldPostToSocialMediaSuccessfullyWithCredsInDb()
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

            var result = await _socialMediaPostingService.CreatePostAsync("This should post only to instagram.", new List<string>
            { "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
              "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\2.jpg" });

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
        public async Task CreatePostAsync_ShouldPostSuccessfullyForMultipleSocialMedias()
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

            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPhoneNumber,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppID,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppHash,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramChannelId,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPassword,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });

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

            var result = await _socialMediaPostingService.CreatePostAsync("This should post to both social media apps", new List<string>
            { "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
              "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\2.jpg" });

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

            Assert.False(result.Success);
            Assert.Equal("No social media posters found", result.Message);
        }

        [Fact]
        public async Task CreatePostAsync_WithNoCredentials_ShouldReturnError()
        {
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPhoneNumber,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppID,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppHash,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramChannelId,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPassword,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.DeleteAsync(1);
            await _credentialRepository.DeleteAsync(5);

            var result = await _socialMediaPostingService.CreatePostAsync("Hello World", new List<string>());

            Assert.False(result.Success);
            Assert.Equal("No social media posters found", result.Message);
        }
        [Fact]
        public async Task GetPostEngagementStatsAsync_ShouldReturnEngagementStats_ForTelegramPost()
        {
            await _userRepository.AddAsync(new User
            {
                Id = UserId,
                Email = "testuser",
                Password = "testpassword",
                Age = 18,
                FirstName = "Test",
                LastName = "User"
            });

            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPhoneNumber,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppID,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppHash,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramChannelId,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPassword,
                StoredLocally = true,
                LocalPath = @"D:\Monopost\Monopost.UnitTests\Resources\constants.txt"
            });
            var postResult = await _socialMediaPostingService.CreatePostAsync("Test post for engagement stats.", new List<string>
            {
                "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
            });

            Assert.True(postResult.Success, postResult.Message);

            var posts = await _postRepository.GetPostsByAuthorIdAsync(UserId);
            Assert.NotEmpty(posts);

            var latestPost = posts.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            Assert.NotNull(latestPost);

            Assert.Equal(latestPost.PostId, 1);
            var postMediaEntries = await _postMediaRepository.GetPostMediaByPostIdAsync(latestPost.PostId);
            Assert.NotEmpty(postMediaEntries);

            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(latestPost.PostId);

            Assert.True(engagementStatsResult.Success);
            Assert.Equal("Stats retrieved successfully", engagementStatsResult.Message);
            Assert.NotEmpty(engagementStatsResult.Data);


            var engagementStats = engagementStatsResult.Data.FirstOrDefault();
            Assert.NotNull(engagementStats);
            Assert.Equal(latestPost.PostId, engagementStats.PostMediaId);
            Assert.Equal(engagementStats.Forwards, 0);
            Assert.Equal(engagementStats.Views, 1);
            Assert.Equal(engagementStats.Reactions, 0);
            Assert.Equal(engagementStats.Comments, 0);
        }
        [Fact]
        public async Task GetPostEngagementStatsAsync_ShouldReturnEngagementStats_ForInstagramPost()
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

            var postResult = await _socialMediaPostingService.CreatePostAsync("This should post only to instagram.", new List<string>
            { "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
              "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\2.jpg" });

            Assert.True(postResult.Success, postResult.Message);

            var posts = await _postRepository.GetPostsByAuthorIdAsync(UserId);
            Assert.NotEmpty(posts);

            var latestPost = posts.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            Assert.NotNull(latestPost);

            var postMediaEntries = await _postMediaRepository.GetPostMediaByPostIdAsync(latestPost.PostId);
            Assert.NotEmpty(postMediaEntries);

            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(latestPost.PostId);

            Assert.True(engagementStatsResult.Success);
            Assert.Equal("Stats retrieved successfully", engagementStatsResult.Message);
            Assert.NotEmpty(engagementStatsResult.Data);


            var engagementStats = engagementStatsResult.Data.FirstOrDefault();
            Assert.NotNull(engagementStats);
            Assert.Equal(latestPost.PostId, 1);
            Assert.Equal(engagementStats.Forwards, 0);
            Assert.Equal(engagementStats.Views, 0);
            Assert.Equal(engagementStats.Reactions, 0);
            Assert.Equal(engagementStats.Comments, 0);
        }
        [Fact]
        public async Task CreatePostAsync_ShouldFail_WhenNoCredentials()
        {
            var postResult = await _socialMediaPostingService.CreatePostAsync("Test post with no credentials.", new List<string>
            {
                "D:\\Monopost\\Monopost.UnitTests\\Resources\\Images\\1.jpg",
            });

            Assert.False(postResult.Success);
            Assert.Equal("No social media posters found", postResult.Message);
        }
        [Fact]
        public async Task CreatePostAsync_ShouldFail_WhenMoreThan10Files()
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

            var filesToUpload = Enumerable.Range(1, 11).Select(i => $"file_{i}.jpg").ToList();

            var postResult = await _socialMediaPostingService.CreatePostAsync("Test post with too many files.", filesToUpload);

            Assert.False(postResult.Success);
            Assert.Equal("Invalid number of files to upload, must be between 1 and 10", postResult.Message);
        }
        [Fact]
        public async Task CreatePostAsync_ShouldFail_WhenTextTooLong()
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
            string longText = new string('a', 301);

            var postResult = await _socialMediaPostingService.CreatePostAsync(longText, new List<string> { "file_1.jpg" });

            Assert.False(postResult.Success);
            Assert.Equal("Text is too long, must be less than 2200 characters", postResult.Message);
        }
        public async Task GetPostEngagementStatsAsync_ShouldFail_WhenPostNotFound()
        {
            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(999); // Non-existent postId

            Assert.False(engagementStatsResult.Success);
            Assert.Equal("Post not found", engagementStatsResult.Message);
            Assert.Empty(engagementStatsResult.Data);
        }
        [Fact]
        public async Task GetPostEngagementStatsAsync_ShouldFail_WhenNoPostMediaExists()
        {
            await _userRepository.AddAsync(new User
            {
                Id = UserId,
                Email = "testuser",
                Password = "testpassword",
                Age = 18,
                FirstName = "Test",
                LastName = "User"
            });

            await _postRepository.AddAsync(new Post
            {
                AuthorId = UserId,
                DatePosted = DateTime.Now
            });

            var latestPosts = await _postRepository.GetPostsByAuthorIdAsync(UserId);
            var latestPost = latestPosts.OrderByDescending(post => post.DatePosted).FirstOrDefault();

            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(latestPost.PostId);

            Assert.False(engagementStatsResult.Success);
            Assert.Equal("Post not found", engagementStatsResult.Message);
            Assert.Empty(engagementStatsResult.Data);
        }
    }
}
