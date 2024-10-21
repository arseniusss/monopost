using Monopost.BLL.Models;


namespace Monopost.BLL.Services.Interfaces
{
    public interface IDataScienceSavingPdfService
    {
        public Result<string> SaveResults(string pathToFolder, string fileName);
    }
}
