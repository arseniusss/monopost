using Microsoft.Extensions.Configuration;
using Monopost.BLL.Models;
using Monopost.DAL.Enums;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public static class SocialMediaPosterFactory
    {
        private static bool UseHardcodedCredentials;

        static SocialMediaPosterFactory()
        {
        }

        public static ISocialMediaPoster? CreatePoster(CredentialType credentialType, List<DecodedCredential> credentials)
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
            return null;
        }
    }
}
