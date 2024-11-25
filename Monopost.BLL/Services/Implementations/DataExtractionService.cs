using Monopost.BLL.Models;
using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Serilog;
using System.Text.Json;

namespace Monopost.BLL.Services.Implementations
{
    public class DataExtractionService: IDataExtractionService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;
        private static readonly ILogger logger = LoggerConfig.GetLogger();

        public DataExtractionService(IUserRepository userRepository, ICredentialRepository credentialRepository, ITemplateRepository templateRepository,
            ITemplateFileRepository templateFileRepository, IPostRepository postRepository, IPostMediaRepository postMediaRepository)
        {
            _userRepository = userRepository;
            _credentialRepository = credentialRepository;
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;
        }

        public async Task<Result<ExtractedUserData>> ExtractData(int userID, bool includeCredentials = false, bool includeTemplates = false, bool includePosts = false, bool totalDataExtraction = false)
        {
            var user = await _userRepository.GetByIdAsync(userID);
            if (user == null)
            {
                return new Result<ExtractedUserData>(false, "User not found", null);
            }

            try
            {
                var extractedUserData = new ExtractedUserData
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Age = user.Age,
                    Email = user.Email,
                    Credentials = includeCredentials || totalDataExtraction ? (await _credentialRepository.GetByUserIdAsync(userID)).Select(c => new DecodedCredential
                    {
                        Id = c.Id,
                        AuthorId = c.AuthorId,
                        CredentialType = c.CredentialType,
                        CredentialValue = c.CredentialValue
                    }).ToList() : new List<DecodedCredential>(),
                    Templates = includeTemplates || totalDataExtraction ? (await _templateRepository.GetTemplatesByAuthorIdAsync(userID)).Select(async t => new ExtractedTemplateModel
                    {
                        Name = t.Name,
                        Text = t.Text,
                        TemplateFiles = (await Task.WhenAll((await _templateFileRepository.GetTemplateFilesByTemplateIdAsync(t.Id)).Select(async f => new ExtractedTemplateFileModel
                        {
                            FileName = f.FileName,
                            FileData = f.FileData,
                        }))).ToList()
                    }).Select(t => t.Result).ToList() : new List<ExtractedTemplateModel>(),
                    Posts = includePosts || totalDataExtraction ? (await _postRepository.GetPostsByAuthorIdAsync(userID)).Select(async p => new ExtractedPostModel
                    {
                        DatePosted = p.DatePosted,
                        PostMedia = (await Task.WhenAll((await _postMediaRepository.GetPostMediaByPostIdAsync(p.PostId)).Select(async m => new ExtractedPostMediaModel
                        {
                            ChannelId = m.ChannelId,
                            MessageId = m.MessageId,
                            SocialMediaName = m.SocialMediaName,
                            MediaUrl = m.SocialMediaName == DAL.Enums.SocialMediaType.Telegram ? "tg link" : "insta link",
                        }))).ToList()
                    }).Select(p => p.Result).ToList() : new List<ExtractedPostModel>()
                };
                return new Result<ExtractedUserData>(true, "Data extracted successfully", extractedUserData);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while extracting data");
                return new Result<ExtractedUserData>(false, "Error while extracting data", null);
            }

        }

        public Result SaveResultToJson(Result<ExtractedUserData> result, string filePath)
        {
            var userEmail = result?.Data?.Email ?? "unknown";

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new Result(false, "File path not provided");
            }

            logger.Information($"User {userEmail} requested to save the result.");

            if (result == null)
            {
                logger.Information($"No result available for user {userEmail}");
                return new Result(false, $"No result available");
            }

            if (!result.Success)
            {
                logger.Information($"Unsuccessful result for user {userEmail}: {result.Message}");
                return new Result(false, $"Unsuccessfull result");
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var res = JsonSerializer.Serialize(result, options);
            try
            {
                File.WriteAllText(filePath, res);
                logger.Information($"File saved to: {filePath} for user {userEmail}");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to write file: {ex.Message} for user {userEmail}");
                return new Result(false, $"Failed to write file: {ex.Message}");
            }

            return new Result(true, "File saved successfully");
        }
    }
}