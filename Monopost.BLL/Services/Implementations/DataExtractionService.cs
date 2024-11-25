using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.Services.Implementations
{
    public class DataExtractionService
    {
        public readonly IUserRepository? _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public DataExtractionService(IUserRepository? userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> ExtractData(int userID, bool credentials = false, bool templates = false, bool posts = false)
        {
            logger.Information($"Trying to extract user`s data. Id: {userID}");

            var existingUser = await _userRepository.GetByIdAsync(userID);

            if (existingUser == null)
            {
                logger.Warning("Result: Failure\nReason: User with such Id does not exist.");
                return new Result(false, "User with such Id does not exist.");
            }

            return new Result(true, "User updated successfully.");
        }
    }
}
