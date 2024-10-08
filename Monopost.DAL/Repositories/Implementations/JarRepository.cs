using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.DAL.Repositories.Implementations
{
    public class JarRepository : IJarRepository
    {
        private readonly AppDbContext _context;

        public JarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Jar> GetByIdAsync(int id)
        {
            var jar = await _context.Jars.FirstOrDefaultAsync(j => j.Id == id);

            if (jar == null)
                throw new Exception("Jar not found");

            return jar;
        }

        public async Task<IEnumerable<Jar>> GetAllAsync()
        {
            return await _context.Jars.ToListAsync();
        }

        public async Task AddAsync(Jar jar)
        {
            await _context.Jars.AddAsync(jar);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Jar jar)
        {
            _context.Jars.Update(jar);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var jar = await GetByIdAsync(id);
            if (jar != null)
            {
                _context.Jars.Remove(jar);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Jar>> GetByOwnerIdAsync(int ownerId)
        {
            return await _context.Jars.Where(j => j.OwnerId == ownerId).ToListAsync();
        }
    }
}
