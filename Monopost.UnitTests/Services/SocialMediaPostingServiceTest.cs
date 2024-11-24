using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Services;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Implementations;
using Monopost.Logging;
using Serilog;
using DotNetEnv;

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

        private static string GetSolutionBaseDirectory()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                if (Directory.GetFiles(currentDirectory, "*.sln").Length > 0)
                {
                    return currentDirectory;
                }
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }
            throw new DirectoryNotFoundException("Solution root directory not found.");
        }

        private static string solutionBaseDirectory = GetSolutionBaseDirectory();
        private string localCredentialsPath = Path.Combine(solutionBaseDirectory, ".env");

        private static string exampleImagePath1 = Path.Combine(solutionBaseDirectory, "Monopost.UnitTests\\Resources\\Images\\1.jpg");
        private static string exampleImagePath2 = Path.Combine(solutionBaseDirectory, "Monopost.UnitTests\\Resources\\Images\\2.jpg");


        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            Log.CloseAndFlush();
        }

        private async Task AddTelegramCreds()
        {
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPhoneNumber,
                StoredLocally = true,
                LocalPath = localCredentialsPath
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppID,
                StoredLocally = true,
                LocalPath = localCredentialsPath
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramAppHash,
                StoredLocally = true,
                LocalPath = localCredentialsPath
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramChannelId,
                StoredLocally = true,
                LocalPath = localCredentialsPath
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.TelegramPassword,
                StoredLocally = true,
                LocalPath = localCredentialsPath
            });
        }

        private async Task AddInstagramCreds()
        {
            Env.Load(localCredentialsPath);
            string instagramAccessToken = Env.GetString("InstagramAccessToken");
            string instagramUserId = Env.GetString("InstagramUserId");
            string imgbbApiKey = Env.GetString("ImgbbApiKey");

            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.InstagramAccessToken,
                CredentialValue = instagramAccessToken,
                StoredLocally = false
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.InstagramUserId,
                CredentialValue = instagramUserId,
                StoredLocally = false
            });
            await _credentialRepository.AddAsync(new Credential
            {
                AuthorId = UserId,
                CredentialType = CredentialType.ImgbbApiKey,
                CredentialValue = imgbbApiKey,
                StoredLocally = false
            });
        }

        [Fact]
        public async Task CreatePostAsync_ShouldPostToSocialMediaSuccessfullyWithLocalCreds()
        {
            _socialMediaPostingService = new SocialMediaPostingService(_credentialRepository, _userRepository, _postRepository, _postMediaRepository, UserId);

            await _userRepository.AddAsync(new User
            {
                Id = UserId,
                Email = "testuser",
                Password = "testpassword",
                Age = 18,
                FirstName = "hi",
                LastName = "bye"
            });

            await AddTelegramCreds();

            var result = await _socialMediaPostingService.CreatePostAsync("This should post only to telegram", new List<string>
            { exampleImagePath1 , exampleImagePath2 });

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

            await AddInstagramCreds();

            var result = await _socialMediaPostingService.CreatePostAsync("This should post only to instagram.",
                new List<string>{ exampleImagePath1,exampleImagePath2 });

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
            _socialMediaPostingService = new SocialMediaPostingService(_credentialRepository, _userRepository, _postRepository, _postMediaRepository, UserId);

            await _userRepository.AddAsync(new User
            {
                Id = UserId,
                Email = "testuser",
                Password = "testpassword",
                Age = 18,
                FirstName = "hi",
                LastName = "bye"
            });

            await AddInstagramCreds();
            await AddTelegramCreds();

            var result = await _socialMediaPostingService.CreatePostAsync("This should post to both social media apps", new List<string> { exampleImagePath1, exampleImagePath2 });


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
            var result = await _socialMediaPostingService.CreatePostAsync("Hello World", new List<string>());

            Assert.False(result.Success);
            Assert.Equal("No social media posters found", result.Message);
        }

        [Fact]
        public async Task CreatePostAsync_WithNoCredentials_ShouldReturnError()
        {
            await AddTelegramCreds();

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

            await AddTelegramCreds();

            var postResult = await _socialMediaPostingService.CreatePostAsync("Test post for engagement stats.", new List<string>
            {
               exampleImagePath1
            });

            Assert.True(postResult.Success, postResult.Message);

            var posts = await _postRepository.GetPostsByAuthorIdAsync(UserId);
            Assert.NotEmpty(posts);

            var latestPost = posts.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            Assert.NotNull(latestPost);

            Assert.Equal(1, latestPost.PostId);
            var postMediaEntries = await _postMediaRepository.GetPostMediaByPostIdAsync(latestPost.PostId);
            Assert.NotEmpty(postMediaEntries);

            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(latestPost.PostId);

            Assert.True(engagementStatsResult.Success);
            Assert.Equal("Stats retrieved successfully", engagementStatsResult.Message);
            Assert.NotEmpty(engagementStatsResult.Data);


            var engagementStats = engagementStatsResult.Data.FirstOrDefault();
            Assert.NotNull(engagementStats);
            Assert.Equal(latestPost.PostId, engagementStats.PostMediaId);
            Assert.Equal(0, engagementStats.Forwards);
            Assert.Equal(1, engagementStats.Views);
            Assert.Equal(0, engagementStats.Reactions);
            Assert.Equal(0, engagementStats.Comments);
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

            await AddInstagramCreds();

            var postResult = await _socialMediaPostingService.CreatePostAsync("This should post only to instagram.", new List<string> { exampleImagePath1, exampleImagePath2 });


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
            Assert.Equal(1, latestPost.PostId);
            Assert.Equal(0, engagementStats.Forwards);
            Assert.Equal(0, engagementStats.Views);
            Assert.Equal(0, engagementStats.Reactions);
            Assert.Equal(0, engagementStats.Comments);
        }
        [Fact]
        public async Task CreatePostAsync_ShouldFail_WhenNoCredentials()
        {
            var postResult = await _socialMediaPostingService.CreatePostAsync("Test post with no credentials.", new List<string>
            {
               exampleImagePath1
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

            await AddInstagramCreds();

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

            await AddInstagramCreds();
            string longText = new string('a', 3001);

            var postResult = await _socialMediaPostingService.CreatePostAsync(longText, new List<string> { exampleImagePath1 });

            Assert.False(postResult.Success);
            Assert.Equal("Text is too long, must be less than 2200 characters", postResult.Message);
        }

        [Fact]
        public async Task GetPostEngagementStatsAsync_ShouldFail_WhenPostNotFound()
        {
            var engagementStatsResult = await _socialMediaPostingService.GetPostEngagementStatsAsync(999);

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
