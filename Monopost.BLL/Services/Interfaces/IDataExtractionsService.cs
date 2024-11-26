using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface IDataExtractionService
    {
        Task<Result<ExtractedUserData>> ExtractData(int userID, bool includeCredentials = false, bool includeTemplates = false,
            bool includePosts = false, bool totalDataExtraction = false);
        Result SaveResultToJson(Result<ExtractedUserData> result, string filePath);
    }
}