using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.Services.Implementations
{
    public class DataDeletionService
    {
        public readonly IUserRepository? _userRepository;
        public readonly ICredentialRepository _credentialRepository;
        public readonly ITemplateRepository _templateRepository;
        public readonly ITemplateFileRepository _templateFileRepository;
        public readonly IPostRepository _postRepository;
        public readonly IPostMediaRepository _postMediaRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public DataDeletionService(IUserRepository? userRepository, ICredentialRepository credentialRepository, ITemplateRepository templateRepository,
            ITemplateFileRepository templateFileRepository, IPostRepository postRepository, IPostMediaRepository postMediaRepository)
        {
            _userRepository = userRepository;
            _credentialRepository = credentialRepository;
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;
        }

        public async Task<Result> DeleteData(int userID, bool credentials = false, bool templates = false, bool posts = false, bool totalAccountDeletion = false)
        {
            logger.Information($"Trying to delete data of user with id=${userID}, credentials={credentials}, templates={templates}, posts={posts}");

            var existingUser = await _userRepository.GetByIdAsync(userID);

            if (existingUser == null)
            {
                logger.Warning("Result: Failure\nReason: User with such Id does not exist.");
                return new Result(false, "User with such Id does not exist.");
            }

            try
            {
                if (credentials || totalAccountDeletion)
                {
                    var userCredentials = await _credentialRepository.GetByUserIdAsync(userID);
                    foreach (var credential in userCredentials)
                    {
                        await _credentialRepository.DeleteAsync(credential.Id);
                    }
                }

                if (templates || totalAccountDeletion)
                {
                    var userTemplates = await _templateRepository.GetTemplatesByAuthorIdAsync(userID);
                    if (userTemplates.Count() != 0)
                    {
                        foreach (var template in userTemplates)
                        {
                            await _templateFileRepository.DeleteAllByTemplateIdAsync(template.Id);
                            await _templateRepository.DeleteAsync(template.Id);
                        }
                    }
                }

                if (posts || totalAccountDeletion)
                {
                    var userPosts = await _postRepository.GetPostsByAuthorIdAsync(userID);
                    foreach (var post in userPosts)
                    {
                        await _postMediaRepository.DeleteAsync(post.PostId);
                        await _postRepository.DeleteAsync(post.PostId);
                    }
                }

                if (totalAccountDeletion)
                {
                    await _userRepository.DeleteAsync(userID);
                }
            } 
            catch (Exception e)
            {
                logger.Error($"Result: Failure\nReason: {e.Message}");
                return new Result(false, e.Message);
            }


            return new Result(true, "User's data deleted successfully.");
        }
    }
}