using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface ITemplateRepository : IRepository<Template>
    {
        Task<IEnumerable<Template>> GetTemplatesByAuthorIdAsync(int authorId);
        Task<Template> GetTemplateWithMediaAsync(int templateId);

    }
}
