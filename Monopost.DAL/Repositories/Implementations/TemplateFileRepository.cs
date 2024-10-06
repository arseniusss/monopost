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
            return await _context.TemplateFiles.FirstOrDefaultAsync(tf => tf.Id == id);
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
            return await _context.TemplateFiles
                .Where(tf => tf.TemplateId == templateId)
                .ToListAsync();
        }
    }
}
