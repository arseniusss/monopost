using TL;
using WTelegram;
using Monopost.BLL.Models;
using Monopost.BLL.SocialMediaManagement.Models;
using Monopost.DAL.Enums;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public class TelegramPoster : ISocialMediaPoster
    {
        private ILogger logger = LoggerConfig.GetLogger();

        private readonly Client _client;
        private readonly string _channelId;
        public TelegramPoster(string apiId, string apiHash, string phoneNumber, string channelId, string? password = null)
        {
            string sessionFilePath = "telegram_session_.dat";

            _client = new Client(Config);

            _channelId = channelId;
            string Config(string what)
            {
                return what switch
                {
                    "api_id" => apiId,
                    "api_hash" => apiHash,
                    "phone_number" => phoneNumber,
                    "password" => password ?? string.Empty,
                    "verification_code" => GetVerificationCode(),
                    "session_pathname" => sessionFilePath,
                    _ => null // не чіпати бо все впаде
                };
            }

            string GetVerificationCode()
            {
                Console.Write("Code: ");
                return Console.ReadLine() ?? string.Empty;
            }
        }

        private async Task LoginAsync()
        {
            await _client.LoginUserIfNeeded();
            logger.Information($"Client logged in into telegram");
        }

        public async Task<Result<PostPageAndId>> CreatePostAsync(string text, List<string> filePaths)
        {
            await LoginAsync();

            var dialogs = await _client.Messages_GetAllDialogs();
            TL.Channel? chat = null;

            chat = dialogs.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID.ToString() == _channelId);

            if (chat == null)
            {
                return new Result<PostPageAndId>(false, "Chat is not found", new PostPageAndId("-1", "-1", SocialMediaType.Telegram));
            }
            var inputMedias = new List<InputMedia>();

            var fileResults = await Task.WhenAll(filePaths.Select(filePath => _client.UploadFileAsync(filePath)));

            inputMedias.AddRange(fileResults.Select(fileResult => new InputMediaUploadedPhoto
            {
                file = fileResult
            }));

            if (inputMedias.Count > 0)
            {
                try
                {
                    var result = await _client.SendAlbumAsync(chat, inputMedias, text);
                    return new Result<PostPageAndId>(true, "Message successfully posted", new PostPageAndId (_channelId, result.FirstOrDefault()?.id.ToString(), SocialMediaType.Telegram));
                }
                catch (Exception ex)
                {
                    return new Result<PostPageAndId>(false, ex.Message, new PostPageAndId("-1", "-1", SocialMediaType.Telegram));
                }
            }
            else
            {
                return new Result<PostPageAndId>(false, "No files to upload.", new PostPageAndId("-1", "-1", SocialMediaType.Telegram));
            }
        }

        public async Task<Result<EngagementStats>> GetEngagementStatsAsync(string postId)
        {
            await LoginAsync();

            var dialogs = await _client.Messages_GetAllDialogs();
            var chat = dialogs.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID.ToString() == _channelId);

            if (chat == null)
            {
                return new Result<EngagementStats>(false, "Channel not found", new EngagementStats(-1, -1, -1, -1));
            }

            var messages = await _client.Channels_GetMessages(chat, int.Parse(postId));

            var message = messages.Messages.FirstOrDefault();

            if (message == null)
            {
                return new Result<EngagementStats>(false, "Message not found", new EngagementStats(-1, -1, -1, -1));
            }

            int views = 0;
            int reactions = 0;
            int forwards = 0;
            int comments = 0;

            if (message is TL.Message msg)
            {
                views = msg.views;
                forwards = msg.forwards;
                if(msg.replies != null && msg.replies?.replies != null)
                    comments = msg.replies.replies;

                if (msg.reactions != null && msg.reactions.results != null)
                {
                    foreach (var reaction in msg.reactions.results)
                    {
                        reactions += reaction.count;
                    }
                }
            }

            return new Result<EngagementStats>(true, "Engagement stats retrieved successfully", new EngagementStats(views, reactions, comments, forwards));
        }

        public async Task<Result<string>> GeneratePostLinkByChannelIdAsync(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
            {
                return new Result<string>(false, "Post ID cannot be null or empty.", string.Empty);
            }

            var dialogs = await _client.Messages_GetAllDialogs();
            var channel = dialogs.chats.Values
                .OfType<Channel>()
                .FirstOrDefault(c => c.ID.ToString() == _channelId);

            if (channel == null)
            {
                return new Result<string>(false, "Channel not found.", string.Empty);
            }

            if (!string.IsNullOrEmpty(channel.username))
            {
                return new Result<string>(true, "Post link generated successfully", $"https://t.me/{channel.username}/{postId}");
            }
            else
            {
                return new Result<string>(true, "Post link generated successfully", $"https://t.me/c/{channel.id}/{postId}");
            }
        }
    }
}