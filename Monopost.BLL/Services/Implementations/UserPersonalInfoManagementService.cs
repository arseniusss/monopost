using Monopost.BLL.Models;
using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.Services.Implementations
{
    public class UserPersonalInfoManagementService : IUserPersonalInfoManagementService
    {
        public readonly IUserRepository? _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public UserPersonalInfoManagementService(IUserRepository? userRepository)
        {
            _userRepository = userRepository;
            logger.Information("UserPersonalInfoManagementService created.");
        }

        public async Task<Result> UpdateUserPersonalInfoAsync(UserPersonalInfoModel model)
        {
            logger.Information($"Trying to update user`s personal information. Id: {model.Id}");
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                logger.Warning("Result: Failure\nReason: First Name is required.");
                return new Result(false, "First Name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                logger.Warning("Result: Failure\nReason: Last name is required.");
                return new Result(false, "Last Name is required.");
            }

            if (model.Age <= 0)
            {
                logger.Warning("Result: Failure\nReason: Valid age is required.");
                return new Result(false, "Valid Age is required.");
            }
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(model.Id);
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.Age = model.Age;

                await _userRepository.UpdateAsync(existingUser);
                logger.Information("Result: Success\nMessage: User information updated successfully.");
                return new Result(true, "User updated successfully.");

            }
            catch
            {
                logger.Warning("Result: Failure\nReason: User with such Id does not exist.");
                return new Result(false, "User with such Id does not exist.");
            }

        }
    }
}
