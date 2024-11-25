using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface IDataDeletionService
    {
        Task<Result> DeleteData(int userID, bool credentials = false, bool templates = false, bool posts = false, bool totalAccountDeletion = false);
    }
}
