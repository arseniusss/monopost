using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.DAL.Repositories.Implementations
{
    public class TemplateFileRepository : ITemplateFileRepository
    {
        private readonly AppDbContext _context;

        public TemplateFileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TemplateFile> GetByIdAsync(int id)
        {
            var templateFile = await _context.TemplateFiles.FirstOrDefaultAsync(tf => tf.Id == id);
            if (templateFile == null)
            {
                throw new ArgumentException($"TemplateFile with id {id} not found.");
            }
            return templateFile;
        }

        public async Task<IEnumerable<TemplateFile>> GetAllAsync()
        {
            return await _context.TemplateFiles.ToListAsync();
        }

        public async Task AddAsync(TemplateFile templateFile)
        {
            await _context.TemplateFiles.AddAsync(templateFile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TemplateFile templateFile)
        {
            _context.TemplateFiles.Update(templateFile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var templateFile = await GetByIdAsync(id);
            if (templateFile != null)
            {
                _context.TemplateFiles.Remove(templateFile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TemplateFile>> GetTemplateFilesByTemplateIdAsync(int templateId)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template with id {templateId} not found.");
            }
            return await _context.TemplateFiles
                .Where(tf => tf.TemplateId == templateId)
                .ToListAsync();
        }

        public async Task DeleteAllByTemplateIdAsync(int templateId)
        {
            var templateFiles = await _context.TemplateFiles
                .Where(tf => tf.TemplateId == templateId)
                .ToListAsync();
            if (templateFiles.Any())
            {
                _context.TemplateFiles.RemoveRange(templateFiles);
                await _context.SaveChangesAsync();
            }
        }
    }
}