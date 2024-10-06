using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;


namespace Monopost.DAL.Repositories.Implementations
{
    public class RestrictionRepository : IRestrictionRepository
    {
        private readonly AppDbContext _context;

        public RestrictionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Restriction> GetByIdAsync(int id)
        {
            return await _context.Restrictions.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Restriction>> GetAllAsync()
        {
            return await _context.Restrictions.ToListAsync();
        }

        public async Task AddAsync(Restriction restriction)
        {
            await _context.Restrictions.AddAsync(restriction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Restriction restriction)
        {
            _context.Restrictions.Update(restriction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var restriction = await GetByIdAsync(id);
            if (restriction != null)
            {
                _context.Restrictions.Remove(restriction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Restriction>> GetRestrictionsByUserIdAsync(int userId)
        {
            return await _context.Restrictions
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}
