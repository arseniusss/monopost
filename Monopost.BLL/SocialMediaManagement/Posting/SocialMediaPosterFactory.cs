using Microsoft.Extensions.Configuration;
using Monopost.BLL.Models;
using Monopost.DAL.Enums;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public static class SocialMediaPosterFactory
    {
        private static IConfiguration _configuration;
        private static bool UseHardcodedCredentials;

        static SocialMediaPosterFactory()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            UseHardcodedCredentials = _configuration.GetValue<bool>("UseHardcodedCredentials");
        }

        public static ISocialMediaPoster? CreatePoster(CredentialType credentialType, List<DecodedCredential> credentials)
        {
            if (UseHardcodedCredentials)
            {
                switch (credentialType)
                {
                    case CredentialType.InstagramAccessToken:
                        var instagramAccessToken = _configuration.GetValue<string>("Instagram:AccessToken");
                        var instagramUserId = _configuration.GetValue<string>("Instagram:UserId");
                        var imgbbApiKey = _configuration.GetValue<string>("Instagram:ImgbbApiKey");

                        if (!string.IsNullOrEmpty(instagramAccessToken) &&
                            !string.IsNullOrEmpty(instagramUserId) &&
                            !string.IsNullOrEmpty(imgbbApiKey))
                        {
                            return new InstagramPoster(instagramAccessToken, instagramUserId, imgbbApiKey);
                        }
                        break;

                    case CredentialType.TelegramAppID:
                        var telegramAppId = _configuration.GetValue<string>("Telegram:AppId");
                        var telegramAppHash = _configuration.GetValue<string>("Telegram:AppHash");
                        var telegramChannelId = _configuration.GetValue<string>("Telegram:ChannelId");
                        var telegramPhoneNumber = _configuration.GetValue<string>("Telegram:PhoneNumber");
                        var telegramPassword = _configuration.GetValue<string>("Telegram:Password");

                        if (!string.IsNullOrEmpty(telegramAppId) &&
                            !string.IsNullOrEmpty(telegramAppHash) &&
                            !string.IsNullOrEmpty(telegramChannelId) &&
                            !string.IsNullOrEmpty(telegramPhoneNumber))
                        {
                            return new TelegramPoster(telegramAppId, telegramAppHash, telegramPhoneNumber, telegramChannelId, telegramPassword);
                        }
                        break;
                }
            }
            else
            {
                switch (credentialType)
                {
                    case CredentialType.InstagramAccessToken:
                        var instagramAccessToken = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.InstagramAccessToken)?.CredentialValue;
                        var instagramUserId = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.InstagramUserId)?.CredentialValue;
                        var imgbbApiKey = credentials.FirstOrDefault(c => c.CredentialType == CredentialType.ImgbbApiKey)?.CredentialValue;

                        if (!string.IsNullOrEmpty(instagramAccessToken) &&
                            !string.IsNullOrEmpty(instagramUserId) &&
                            !string.IsNullOrEmpty(imgbbApiKey))
                        {
                            return new InstagramPoster(instagramAccessToken, instagramUserId, imgbbApiKey);
                        }
                        break;

                    case CredentialType.TelegramAppID:
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
                            return new TelegramPoster(telegramAppId, telegramAppHash, telegramPhoneNumber, telegramChannelId, telegramPassword);
                        }
                        break;
                }
            }

            return null;
        }
    }
}
