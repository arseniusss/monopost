using System.Threading.Channels;
using System.Threading.Tasks;
using Monopost.DAL.DataAccess;
using TL;
using WTelegram;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public class TelegramPoster
    {
        private readonly Client _client;
        private readonly List<string> _postIds;

        public TelegramPoster(string apiId, string apiHash, string phoneNumber, string? password = null)
        {
            _client = new Client(Config);
            _postIds = new List<string>();

            string Config(string what)
            {
                return what switch
                {
                    "api_id" => apiId,
                    "api_hash" => apiHash,
                    "phone_number" => phoneNumber,
                    "password" => password ?? "",
                    "verification_code" => GetVerificationCode(),
                    "session_pathname" => "telegram_session.dat",
                    _ => null
                };
            }

            string GetVerificationCode()
            {
                Console.Write("Code: ");
                return Console.ReadLine();
            }
        }

        public async Task LoginAsync()
        {
            var user = await _client.LoginUserIfNeeded();
            Console.WriteLine($"Hello, {user.first_name}!");
        }

        public async Task<string> PostToChannelAsync(string? channelName = null, long? channelId = null, string? text = null, List<string>? filePaths = null)
        {
            if (string.IsNullOrEmpty(channelName) && !channelId.HasValue)
                throw new ArgumentException("Either channelName or channelId must be provided.");

            var dialogs = await _client.Messages_GetAllDialogs();
            TL.Channel? chat = null;

            if (!string.IsNullOrEmpty(channelName))
            {
                chat = dialogs.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.title == channelName);
            }
            else if (channelId.HasValue)
            {
                chat = dialogs.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.id == channelId.Value);
            }

            if (chat == null)
            {
                throw new Exception("Channel not found");
            }

            var inputMedias = new List<InputMedia>();

            if (filePaths != null && filePaths.Count > 0)
            {
                var fileResults = await Task.WhenAll(filePaths.Select(filePath => _client.UploadFileAsync(filePath)));

                inputMedias.AddRange(fileResults.Select(fileResult => new InputMediaUploadedPhoto
                {
                    file = fileResult
                }));
            }

            if (inputMedias.Count > 0)
            {
                await _client.SendAlbumAsync(chat, inputMedias, text ?? string.Empty);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                var result = await _client.SendMessageAsync(chat, text);
                _postIds.Add(result.id.ToString());
                return result.id.ToString();
            }

            return "No media or text provided.";
        }

        public async Task<(int views, int forwards, int reactions)> GetPostStatsAsync(string channelName, int messageId)
        {
            var dialogs = await _client.Messages_GetAllDialogs();
            var chat = dialogs.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.title == channelName);

            if (chat == null)
            {
                throw new Exception("Channel not found");
            }

            var messages = await _client.Channels_GetMessages(chat, messageId);

            var message = messages.Messages.FirstOrDefault();

            if (message == null)
            {
                throw new Exception("Message not found");
            }


            int views = 0;
            int reactions = 0;
            int forwards = 0;

            if (message is TL.Message msg)
            {
                views = msg.views;
                forwards = msg.forwards;

                if (msg.reactions != null && msg.reactions.results != null)
                {

                    foreach (var reaction in msg.reactions.results)
                    {
                        reactions += reaction.count;
                    }
                }
            }


            return (views, forwards, reactions);
        }

        public async Task<string> GeneratePostLinkByChannelNameAsync(string channelName, string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
            {
                throw new ArgumentException("Post ID cannot be null or empty.");
            }

            var dialogs = await _client.Messages_GetAllDialogs();
            var channel = dialogs.chats.Values
                .OfType<TL.Channel>()
                .FirstOrDefault(c => c.title.Equals(channelName, StringComparison.OrdinalIgnoreCase));

            if (channel == null)
            {
                throw new Exception("Channel not found.");
            }

            if (!string.IsNullOrEmpty(channel.username))
            {
                return $"https://t.me/{channel.username}/{postId}";
            }
            else
            {
                return $"https://t.me/c/{channel.id}/{postId}";
            }
        }
    }
}