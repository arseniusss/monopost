using DotNetEnv;
using Monopost.DAL.Repositories.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Monopost.PresentationLayer.Helpers
{
    public static class UserSession
    {
        private static readonly string StorageFilePath;
        private static readonly string EncryptionKey;
        private static readonly int MIN_GUEST_ID = 1000000;
        private static readonly int MAX_GUEST_ID = 9999999;

        static UserSession()
        {
            Env.Load();

            EncryptionKey = Env.GetString("ENCRYPTION_KEY", "default-key");
            string fileName = Env.GetString("SESSION_FILE_NAME", "user_session.enc");
            StorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        public static int GetUserId()
        {
            if (!File.Exists(StorageFilePath)) return -1;

            try
            {
                var encryptedContent = File.ReadAllBytes(StorageFilePath);
                var decryptedContent = Decrypt(encryptedContent, EncryptionKey);

                if (int.TryParse(decryptedContent, out var userId))
                {
                    return userId;
                }
            }
            catch
            {
                ClearUserId();
            }

            return -1;
        }

        public static async Task SetUserId(IUserRepository userRepo, string email)
        {
            if (File.Exists(StorageFilePath))
            {
                File.Delete(StorageFilePath);
            }

            var users = await userRepo.GetAllAsync();
            var userId = -1;

            foreach (var user in users)
            {
                if (user.Email == email)
                {
                    userId = user.Id;
                    break;
                }
            }

            if (userId == -1)
            {
                userId = email != null? new Random().Next(1, MIN_GUEST_ID) : new Random().Next(MIN_GUEST_ID, MAX_GUEST_ID);
            }

            var encryptedContent = Encrypt(userId.ToString(), EncryptionKey);
            File.WriteAllBytes(StorageFilePath, encryptedContent);
        }

        public static void ClearUserId()
        {
            if (File.Exists(StorageFilePath))
            {
                File.Delete(StorageFilePath);
            }
        }

        private static byte[] Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.Key = keyBytes;
            aes.IV = new byte[16];
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static string Decrypt(byte[] cipherText, string key)
        {
            using var aes = Aes.Create();
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.Key = keyBytes;
            aes.IV = new byte[16];
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}