using Monopost.BLL.Models;
using Monopost.BLL.SocialMediaManagement.Models;
using Monopost.DAL.Enums;
using Monopost.Logging;
using Serilog;
using TL;
using WTelegram;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public class TelegramPoster : ISocialMediaPoster
    {
        private ILogger logger = LoggerConfig.GetLogger();

        private readonly Client _client;
        private readonly string _channelId;
        private readonly string _sessionFilePath;

        public TelegramPoster(string apiId, string apiHash, string phoneNumber, string channelId, string? password = null)
        {
            // Generate session file name with a random number
            var random = new Random();
            _sessionFilePath = $"telegram_session_{random.Next(100000, 999999)}.dat";

            CopyExistingSessionFile("telegram_session_.dat", _sessionFilePath);

            _channelId = channelId;

            _client = new Client(Config);

            string Config(string what)
            {
                return what switch
                {
                    "api_id" => apiId,
                    "api_hash" => apiHash,
                    "phone_number" => phoneNumber,
                    "password" => password ?? string.Empty,
                    "verification_code" => GetVerificationCode(),
                    "session_pathname" => _sessionFilePath,
                    _ => null // не чіпати бо все впаде
                };
            }

            string GetVerificationCode()
            {
                Console.Write("Code: ");
                return Console.ReadLine() ?? string.Empty;
            }
        }

        private void CopyExistingSessionFile(string sourceFilePath, string targetFilePath)
        {
            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath, overwrite: true);
                Console.WriteLine($"Session file copied to {targetFilePath}");
            }
            else
            {
                Console.WriteLine($"Source file '{sourceFilePath}' does not exist.");
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
                    return new Result<PostPageAndId>(true, "Message successfully posted", new PostPageAndId(_channelId, result.FirstOrDefault()?.id.ToString(), SocialMediaType.Telegram));
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
                if (msg.replies != null && msg.replies?.replies != null)
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
            await LoginAsync();

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