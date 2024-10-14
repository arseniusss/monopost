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

            if (template.TemplateFiles != null && template.TemplateFiles.Any())
            {
                foreach (var templateFile in template.TemplateFiles)
                {
                    _context.TemplateFiles.Add(templateFile);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Template template)
        {
            var existingTemplate = await GetByIdAsync(template.Id);

            if (existingTemplate != null)
            {
                existingTemplate.Name = template.Name;
                existingTemplate.Text = template.Text;

                var existingFiles = existingTemplate.TemplateFiles.ToList();
                foreach (var existingFile in existingFiles)
                {
                    if (!template.TemplateFiles.Any(f => f.Id == existingFile.Id))
                    {
                        _context.TemplateFiles.Remove(existingFile);
                    }
                }

                foreach (var newFile in template.TemplateFiles)
                {
                    var existingFile = existingFiles.FirstOrDefault(f => f.Id == newFile.Id);
                    if (existingFile != null)
                    {
                        _context.Entry(existingFile).CurrentValues.SetValues(newFile);
                    }
                    else
                    {
                        await _context.TemplateFiles.AddAsync(newFile);
                    }
                }

                _context.Templates.Update(existingTemplate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var template = await GetByIdAsync(id);
            if (template != null)
            {
                if (template.TemplateFiles != null)
                {
                    _context.TemplateFiles.RemoveRange(template.TemplateFiles);
                }

                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Template>> GetTemplatesByAuthorIdAsync(int authorId)
        {
            return await _context.Templates
                .Where(t => t.AuthorId == authorId)
                .Include(t => t.TemplateFiles)
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