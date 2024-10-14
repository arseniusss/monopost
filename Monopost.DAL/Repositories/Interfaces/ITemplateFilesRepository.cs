using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface ITemplateFileRepository : IRepository<TemplateFile>
    {
        Task<IEnumerable<TemplateFile>> GetTemplateFilesByTemplateIdAsync(int templateId);
        Task DeleteAllByTemplateIdAsync(int templateId);
    }
}
