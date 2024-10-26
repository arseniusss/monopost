using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface IUserPersonalInfoManagementService
    {
        Task<Result> UpdateUserPersonalInfoAsync(UserPersonalInfoModel model);
    }
}