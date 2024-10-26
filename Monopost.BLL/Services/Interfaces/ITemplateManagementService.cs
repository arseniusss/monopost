using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface ITemplateManagementService
    {
        Task<Result<TemplateModel>> AddTemplateAsync(TemplateModel model);
        Task<Result> DeleteTemplateAsync(int templateId);
        Task<Result> UpdateTemplateAsync(TemplateModel model);
        Task<Result<IEnumerable<TemplateModel>>> GetAllTemplatesAsync();
        Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByUserIdAsync(int userId);
        Task<Result<IEnumerable<TemplateFileModel>>> GetTemplateFilesByTemplateIdAsync(int templateId);
    }
}
