using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public class FacebookMetaPoster
    {
        private readonly string _accessToken;
        private readonly string _pageId;
        private readonly HttpClient _httpClient;

        public FacebookMetaPoster(string accessToken, string pageId)
        {
            _accessToken = accessToken;
            _pageId = pageId;
            _httpClient = new HttpClient();
        }

        public async Task<string> PostToPageAsync(string message, List<string>? filePaths = null)
        {
            var url = $"https://graph.facebook.com/v12.0/{_pageId}/photos";
            var formData = new MultipartFormDataContent();

            if (!string.IsNullOrEmpty(message))
            {
                formData.Add(new StringContent(message), "message");
            }

            if (filePaths != null)
            {
                foreach (var filePath in filePaths)
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg"); // або відповідний тип
                    formData.Add(fileContent, "file", Path.GetFileName(filePath));
                }
            }

            formData.Add(new StringContent(_accessToken), "access_token");

            var response = await _httpClient.PostAsync(url, formData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to post to Facebook: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response JSON: " + responseContent); // Виводимо JSON-відповідь

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return jsonResponse.GetProperty("id").GetString() ?? "";
        }

        public async Task<(int likes, int comments, int shares)> GetPostStatsAsync(string postId)
        {
            var url = $"https://graph.facebook.com/v12.0/{postId}?fields=shares,likes.summary(true),comments.summary(true)&access_token={_accessToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get post stats: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            int likes = jsonResponse.GetProperty("likes").GetProperty("summary").GetProperty("total_count").GetInt32();
            int comments = jsonResponse.GetProperty("comments").GetProperty("summary").GetProperty("total_count").GetInt32();
            int shares = jsonResponse.TryGetProperty("shares", out var sharesProperty) ? sharesProperty.GetProperty("count").GetInt32() : 0;

            return (likes, comments, shares);
        }

        public string GeneratePostLink(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
            {
                throw new ArgumentException("Post ID cannot be null or empty.");
            }

            return $"https://www.facebook.com/{_pageId}/posts/{postId}";
        }
    }
}
