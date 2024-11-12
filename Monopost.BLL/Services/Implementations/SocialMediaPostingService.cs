using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Enums;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.SocialMediaManagement.Posting;
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
        }

        private async Task<bool> AddPosters()
        {
            if (_socialMediaPosters.Count != 0)
            {
                return true;
            }

            _socialMediaPosters = new List<ISocialMediaPoster>();
            var credentialsResult = await _credentialManagementService.GetDecodedCredentialsByUserIdAsync(_userId);
            if (!credentialsResult.Success)
            {
                logger.Information("Failed to fetch decoded creds");
                return false;
            }
            if (credentialsResult.Data == null || !credentialsResult.Data.Any())
            {
                return false;
            }

            logger.Information($"{credentialsResult.Data.Count()} credentials found for user with id = {_userId}");
            var credentials = credentialsResult.Data.ToList();

            var instagramPoster = SocialMediaPosterFactory.CreatePoster(CredentialType.InstagramAccessToken, credentials);
            if (instagramPoster != null)
            {
                _socialMediaPosters.Add(instagramPoster);
            }

            var telegramPoster = SocialMediaPosterFactory.CreatePoster(CredentialType.TelegramAppID, credentials);
            if (telegramPoster != null)
            {
                _socialMediaPosters.Add(telegramPoster);
            }

            logger.Information($"{_socialMediaPosters.Count} social media posters added");
            return _socialMediaPosters.Count != 0;
        }

        public async Task<Result<bool>> CreatePostAsync(string text, List<string> filesToUpload, List<int> posterIds = null)
        {
            const int MIN_ALLOWED_FILES_TO_POST = 1;
            const int MAX_ALLOWED_FILES_TO_POST = 10;
            const int MAX_ALLOWED_TEXT_LENGTH = 2200;

            var postersAdded = await AddPosters();
            logger.Information($"Trying to create a post with text='{text}' and {filesToUpload.Count} files");

            if (!postersAdded)
            {
                logger.Warning("No social media posters found");
                return new Result<bool>(false, "No social media posters found");
            }

            if (filesToUpload.Count < MIN_ALLOWED_FILES_TO_POST || filesToUpload.Count > MAX_ALLOWED_FILES_TO_POST)
            {
                logger.Warning("Invalid number of files to upload");
                return new Result<bool>(false, "Invalid number of files to upload, must be between 1 and 10");
            }

            if (text.Length > MAX_ALLOWED_TEXT_LENGTH)
            {
                logger.Warning("Text is too long");
                return new Result<bool>(false, $"Text is too long, must be less than {MAX_ALLOWED_TEXT_LENGTH} characters");
            }

            if (posterIds != null)
            {
                if (posterIds.Count == 0)
                {
                    logger.Warning("No poster IDs provided");
                    return new Result<bool>(false, "Poster IDs cannot be empty");
                }

                foreach (var id in posterIds)
                {
                    if (id < 0 || id >= _socialMediaPosters.Count)
                    {
                        logger.Warning($"Invalid poster ID: {id}. It must be between 0 and {_socialMediaPosters.Count - 1}");
                        return new Result<bool>(false, $"Invalid poster ID: {id}. It must be between 0 and {_socialMediaPosters.Count - 1}");
                    }
                }
            }

            logger.Information($"{_socialMediaPosters.Count} social media posters added");

            var postsToSpecificSocialMedia = new List<PostPageAndId>();
            var postersToUse = posterIds != null && posterIds.Count > 0
                ? posterIds.Select(id => _socialMediaPosters[id]).ToList()
                : _socialMediaPosters;

            foreach (var socialMediaPoster in postersToUse)
            {
                var result = await socialMediaPoster.CreatePostAsync(text, filesToUpload);
                if (!result.Success || (result.Success && result.Data == null))
                {
                    logger.Warning($"Result: Error, Reason: {result.Message}");
                    return new Result<bool>(false, result.Message);
                }
                postsToSpecificSocialMedia.Add(result.Data);
            }

            await _postRepository.AddAsync(new Post
            {
                AuthorId = _userId,
                DatePosted = DateTime.UtcNow,
            });

            var latestPosts = await _postRepository.GetPostsByAuthorIdAsync(_userId);
            var latestPost = latestPosts?.OrderByDescending(post => post.DatePosted).FirstOrDefault();
            if (latestPost == null)
            {
                logger.Warning("Failed to get latest post");
                return new Result<bool>(false, "Failed to get latest post");
            }

            foreach (var post in postsToSpecificSocialMedia)
            {
                await _postMediaRepository.AddAsync(new PostMedia
                {
                    PostId = latestPost.PostId,
                    ChannelId = post.Page,
                    MessageId = post.Id,
                    SocialMediaName = post.SocialMedia,
                });
            }

            logger.Information("Post created successfully");
            return new Result<bool>(true, "Messages posted successfully");
        }

        public async Task<Result<List<PostEngagementStats>>> GetPostEngagementStatsAsync(int postId)
        {
            logger.Information($"Trying to get post engagement stats of post with id = {postId}");
            var postMedia = _postMediaRepository.GetPostMediaByPostIdAsync(postId).Result;
            if (postMedia == null || !postMedia.Any())
            {
                logger.Warning($"Post with id = {postId} not found");
                return new Result<List<PostEngagementStats>>(false, "Post not found", new List<PostEngagementStats>());
            }
            List<PostEngagementStats> stats = new List<PostEngagementStats>();
            foreach (var postSpecificSocialMedia in postMedia)
            {
                logger.Information($"Getting engagement stats for post with id = {postId} and social media = {postSpecificSocialMedia.SocialMediaName}");
                if (postSpecificSocialMedia.SocialMediaName == SocialMediaType.Telegram)
                {
                    try
                    {

                        var telegramPoster = _socialMediaPosters.FirstOrDefault(poster => poster is TelegramPoster);
                        if (telegramPoster == null)
                        {
                            logger.Warning($"No TelegramPoster found for post with id = {postId}");
                            continue;
                        }
                        if (postSpecificSocialMedia.MessageId == null)
                        {
                            logger.Warning($"MessageId is null for post with id = {postId}");
                            continue;
                        }
                        var response = await telegramPoster.GetEngagementStatsAsync(postSpecificSocialMedia.MessageId);

                        if (response.Data == null)
                        {
                            logger.Warning($"Response data is null for post with id = {postId} and social media = {postSpecificSocialMedia.SocialMediaName}");
                            continue;
                        }

                        if (response.Success)
                        {
                            stats.Add(new PostEngagementStats(postId, response.Data.Views, response.Data.Reactions, response.Data.Comments, response.Data.Forwards));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Failed to get engagement stats for post with id = {postId} and social media = {postSpecificSocialMedia.SocialMediaName}, reason = {ex}");
                    }
                }
                else if (postSpecificSocialMedia.SocialMediaName == SocialMediaType.Instagram)
                {
                    try
                    {
                        var instagramPoster = _socialMediaPosters.FirstOrDefault(poster => poster is InstagramPoster);

                        if (instagramPoster == null)
                        {
                            logger.Warning($"No InstagramPoster found for post with id = {postId}");
                            continue; 
                        }

                        if (postSpecificSocialMedia.MessageId == null)
                        {
                            logger.Warning($"MessageId is null for post with id = {postId}");
                            continue;
                        }

                        var response = await instagramPoster.GetEngagementStatsAsync(postSpecificSocialMedia.MessageId);

                        if (response.Data == null)
                        {
                            logger.Warning($"Response data is null for post with id = {postId} and social media = {postSpecificSocialMedia.SocialMediaName}");
                            continue;
                        }

                        if (response.Success)
                        {
                            stats.Add(new PostEngagementStats(postId, response.Data.Views, response.Data.Reactions, response.Data.Comments, response.Data.Forwards));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Failed to get engagement stats for post with id = {postId} and social media = {postSpecificSocialMedia.SocialMediaName}, reason = {ex}");
                    }
                }
            }
            logger.Information($"Stats retrieved successfully");
            return new Result<List<PostEngagementStats>>(true, "Stats retrieved successfully", stats);
        }
    }
}