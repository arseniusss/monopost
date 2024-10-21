using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Enums;
using Sprache;
using Monopost.BLL.Services.Interfaces;
using Result = Monopost.BLL.Models.Result;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.SocialMediaManagement;
using Monopost.BLL.SocialMediaManagement.Posting;
using Monopost.DAL.Repositories.Implementations;
using Monopost.BLL.SocialMediaManagement.Models;

namespace Monopost.BLL.Services
{
    public class SocialMediaPostingService
    {
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;
        private readonly CredentialManagementService _credentialManagementService;
        private List<ISocialMediaPoster> _socialMediaPosters;
        private readonly int _userId;

        public static ILogger logger = LoggerConfig.GetLogger();

        public SocialMediaPostingService(ICredentialRepository credentialRepository, IUserRepository userRepository,
            IPostRepository postRepository, IPostMediaRepository postMediaRepository, int userId)
        {
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;
            _credentialManagementService = new CredentialManagementService(_credentialRepository, _userRepository);
            _socialMediaPosters = new List<ISocialMediaPoster>();
            _userId = userId;
            logger.Information($"Social media posting service created");
            AddPosters();
        }

        private bool AddPosters()
        {
            _socialMediaPosters = new List<ISocialMediaPoster>();
            var credentialsResult = _credentialManagementService.GetDecodedCredentialsByUserIdAsync(_userId).Result;
            if (!credentialsResult.Success)
            {
                return false;
            }
            if (credentialsResult.Data == null || !credentialsResult.Data.Any())
            {
                return false;
            }
            logger.Information($"{credentialsResult.Data.ToList().Count.ToString()} credentials found for user with id = {_userId}");
            var credentials = credentialsResult.Data.ToList();


            var instagramAccessToken = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.InstagramAccessToken)?.CredentialValue;
            var instagramUserId = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.InstagramUserId)?.CredentialValue;
            var imgbbApiKey = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.ImgbbApiKey)?.CredentialValue;

            if (!string.IsNullOrEmpty(instagramAccessToken) &&
                !string.IsNullOrEmpty(instagramUserId) &&
                !string.IsNullOrEmpty(imgbbApiKey))
            {
                var instagramPoster = new InstagramPoster(instagramAccessToken, instagramUserId, imgbbApiKey);
                _socialMediaPosters.Add(instagramPoster);
            }

            var telegramAppId = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.TelegramAppID)?.CredentialValue;
            var telegramAppHash = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.TelegramAppHash)?.CredentialValue;
            var telegramChannelId = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.TelegramChannelId)?.CredentialValue;
            var telegramPhoneNumber = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.TelegramPhoneNumber)?.CredentialValue;
            var telegramPassword = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.TelegramPassword)?.CredentialValue;

            if (!string.IsNullOrEmpty(telegramAppId) &&
                !string.IsNullOrEmpty(telegramAppHash) &&
                !string.IsNullOrEmpty(telegramChannelId) &&
                !string.IsNullOrEmpty(telegramPhoneNumber))
            {
                var telegramPoster = new TelegramPoster(telegramAppId, telegramAppHash, telegramPhoneNumber, telegramChannelId, telegramPassword);
                _socialMediaPosters.Add(telegramPoster);
            }
            logger.Information($"{_socialMediaPosters.Count} social media posters added");
            return _socialMediaPosters.Count != 0;
        }

        public async Task<Result<bool>> CreatePostAsync(string text, List<string> filesToUpload)
        {
            if (!AddPosters())
            {
                return new Result<bool>(false, "No social media posters found");
            }
            logger.Information($"Social media posters added");
            var postsToSpecificSocialMedia = new List<PostPageAndId>();

            foreach (var socialMediaPoster in _socialMediaPosters)
            {
                var result = socialMediaPoster.CreatePostAsync(text, filesToUpload).Result;
                if (!result.Success || (result.Success && result.Data == null))
                {
                    return new Result<bool>(false, result.Message);
                }
                postsToSpecificSocialMedia.Add(result.Data);
                _postRepository.AddAsync(new Post
                {
                    AuthorId = _userId,
                    DatePosted = DateTime.Now,
                });
            }
            var latest_posts = _postRepository.GetPostsByAuthorIdAsync(_userId).Result?.ToList();
            var latest_post = latest_posts.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            foreach(var post in postsToSpecificSocialMedia)
            {
                _postMediaRepository.AddAsync(new PostMedia
                {
                    PostId = latest_post.PostId,
                    ChannelId = post.Page,
                    MessageId = post.Id,
                    SocialMediaName = post.SocialMedia.ToString(),
                });
            }
            
            return new Result<bool>(true, "Messages posted successfully");
        }
    }
}