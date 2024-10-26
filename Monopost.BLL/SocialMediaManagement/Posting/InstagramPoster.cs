using Monopost.BLL.Models;
using Monopost.BLL.SocialMediaManagement.Models;
using Newtonsoft.Json;
using Monopost.DAL.Enums;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public class InstagramPoster : ISocialMediaPoster
    {
        private ILogger logger = LoggerConfig.GetLogger();

        private readonly HttpClient _client;
        private readonly string _accessToken;
        private readonly string _userId;
        private readonly string _imgbbApiKey;

        public InstagramPoster(string accessToken, string userId, string imgbbApiKey)
        {
            _client = new HttpClient();
            _accessToken = accessToken;
            _userId = userId;
            _imgbbApiKey = imgbbApiKey;
        }

        private async Task<List<string>> UploadImagesAsync(List<string> imagePaths)
        {
            var uploadedUrls = new List<string>();

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var imageUrl = await UploadImageAsync(imagePath);
                    uploadedUrls.Add(imageUrl);
                }
                catch {}
            }

            return uploadedUrls;
        }

        private async Task<string> UploadImageAsync(string imagePath)
        {
            using var form = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
            imageContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg");
            form.Add(imageContent, "image", Path.GetFileName(imagePath));

            var response = await _client.PostAsync($"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", form);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                return result?.data.url;
            }

            logger.Warning($"Error uploading image: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            throw new Exception($"Error uploading image: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        }

        private async Task<List<string>> CreateMediaObjectsAsync(List<string> urls)
        {
            var mediaIds = new List<string>();

            foreach (var url in urls)
            {
                var payload = new Dictionary<string, string>
                {
                    { "image_url", url },
                    { "access_token", _accessToken }
                };

                var response = await _client.PostAsync($"https://graph.facebook.com/{_userId}/media?access_token={_accessToken}",
                    new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var mediaId = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()).id;
                    mediaIds.Add(mediaId.ToString());
                }
            }

            return mediaIds;
        }

        private async Task<string> CreateCarouselAsync(List<string> mediaIds, string? text)
        {
            var children = string.Join(",", mediaIds);
            var response = await _client.PostAsync($"https://graph.facebook.com/{_userId}/media?caption={Uri.EscapeDataString(text ?? string.Empty)}&media_type=CAROUSEL&children={children}&access_token={_accessToken}", null);

            if (response.IsSuccessStatusCode)
            {
                var creationId = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()).id;

                var publishResponse = await _client.PostAsync($"https://graph.facebook.com/{_userId}/media_publish?creation_id={creationId}&access_token={_accessToken}", null);

                if (publishResponse.IsSuccessStatusCode)
                {
                    var publishedId = JsonConvert.DeserializeObject<dynamic>(await publishResponse.Content.ReadAsStringAsync()).id;
                    return publishedId;
                }
            }
            return null;
        }

        public async Task<Result<PostPageAndId>> CreatePostAsync(string text, List<string> filePaths)
        {
            List<string> mediaIds = new List<string>();

            if (filePaths.Any())
            {
                var uploadedUrls = await UploadImagesAsync(filePaths);

                if (uploadedUrls == null || !uploadedUrls.Any())
                {
                    logger.Warning("Failed to upload images.");
                    return new Result<PostPageAndId>(false, "Failed to upload images.", new PostPageAndId("-1", "-1", SocialMediaType.Instagram));
                }

                mediaIds = await CreateMediaObjectsAsync(uploadedUrls);

                if (mediaIds == null || !mediaIds.Any())
                {
                    logger.Warning("Failed to create media objects.");
                    return new Result<PostPageAndId>(false, "Failed to create media objects.", new PostPageAndId("-1", "-1", SocialMediaType.Instagram));
                }
            }
            string carouselId = string.Empty;
            try
            {
                carouselId = await CreateCarouselAsync(mediaIds, text);
                if (carouselId == null)
                {
                    logger.Warning("Failed to create carousel.");
                    return new Result<PostPageAndId>(false, "Faild to create carousel", new PostPageAndId("-1", "-1", SocialMediaType.Instagram));
                }
                return new Result<PostPageAndId>(true, "Posted successfully", new PostPageAndId(_userId, carouselId, SocialMediaType.Instagram));
            }
            catch (Exception e)
            {
                logger.Warning($"Unexpected error occured tryung to create carousel: {e.Message}");
                return new Result<PostPageAndId>(false, $"Failed to create carousel: {e.Message}", new PostPageAndId("-1", "-1", SocialMediaType.Instagram));
            }
        }

        public async Task<Result<EngagementStats>> GetEngagementStatsAsync(string postId)
        {
            if (!string.IsNullOrEmpty(postId))
            {
                var response = await _client.GetAsync($"https://graph.facebook.com/{postId}/insights?metric=likes,comments,impressions,shares&access_token={_accessToken}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(jsonResponse);

                    int likesValue = 0;
                    int commentsValue = 0;
                    int sharesValue = 0;
                    int impressionsValue = 0;

                    foreach (var item in result.data)
                    {
                        string name = item.name.ToString();

                        foreach (var value in item.values)
                        {
                            if (name == "likes")
                            {
                                likesValue = value.value;
                            }
                            else if (name == "comments")
                            {
                                commentsValue = value.value;
                            }
                            else if (name == "shares")
                            {
                                sharesValue = value.value;
                            }
                            else if (name == "impressions")
                            {
                                impressionsValue = value.value;
                            }
                        }
                    }
                    logger.Information("Fetched engagement stats successfully");
                    return new Result<EngagementStats>(true, "Fetched engagement stats successfully", new EngagementStats(impressionsValue, likesValue, commentsValue, sharesValue));
                }
                else
                {
                    logger.Warning($"Error fetching emgagement stats for media ID {postId}: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                    return new Result<EngagementStats>(false, $"Error fetching emgagement stats for media ID {postId}: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}", new EngagementStats(-1, -1, -1, -1));
                }
            }
            else
            {
                logger.Warning("No published ID available to fetch likes, comments, shares, impressions.");
                return new Result<EngagementStats>(false, "No published ID available to fetch likes, comments, shares, impressions.", new EngagementStats(-1, -1, -1, -1));
            }
        }
        public async Task<Result<string>> GeneratePostLinkByChannelIdAsync(string postId)
        {
            try
            {
                var responseForLink = await _client.GetAsync($"https://graph.facebook.com/{postId}/?fields=permalink&access_token={_accessToken}");
                var permalink = string.Empty;
                if (responseForLink.IsSuccessStatusCode)
                {
                    var jsonLink = await responseForLink.Content.ReadAsStringAsync();
                    var linkResult = JsonConvert.DeserializeObject<dynamic>(jsonLink);
                    permalink = linkResult?.permalink;
                    return new Result<string>(true, "Post link generated successfully", permalink);
                }
                else
                {
                    logger.Warning($"Error generating post link: {responseForLink.StatusCode}, {await responseForLink.Content.ReadAsStringAsync()}");
                    return new Result<string>(false, $"Error generating post link: {responseForLink.StatusCode}, {await responseForLink.Content.ReadAsStringAsync()}", string.Empty);
                }
            }
            catch (Exception e)
            {
                logger.Warning($"Error generating post link: {e.Message}");
                return new Result<string>(false, $"Error generating post link: {e.Message}", string.Empty);
            }
        }
    }
}