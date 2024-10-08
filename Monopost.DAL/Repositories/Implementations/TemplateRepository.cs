using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.DAL.Repositories.Implementations
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly AppDbContext _context;

        public TemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Template> GetByIdAsync(int id)
        {
            var template = await _context.Templates
                .Include(t => t.TemplateFiles)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
                throw new Exception("Template not found");

            return template;
        }

        public async Task<IEnumerable<Template>> GetAllAsync()
        {
            return await _context.Templates
                .Include(t => t.TemplateFiles)
                .ToListAsync();
        }

        public async Task AddAsync(Template template)
        {
            await _context.Templates.AddAsync(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Template template)
        {
            _context.Templates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var template = await GetByIdAsync(id);
            if (template != null)
            {
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Template>> GetTemplatesByAuthorIdAsync(int authorId)
        {
            return await _context.Templates
                .Where(t => t.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<Template> GetTemplateWithMediaAsync(int templateId)
        {
            var template = await _context.Templates
                .Include(t => t.TemplateFiles)
                .FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
                throw new Exception("Template not found");

            return template;
        }
    }
}
