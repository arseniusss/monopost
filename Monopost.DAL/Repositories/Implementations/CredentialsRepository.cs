using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Enums;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.DAL.Repositories.Implementations
{
    public class CredentialRepository : ICredentialRepository
    {
        private readonly AppDbContext _context;

        public CredentialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Credential> GetByIdAsync(int id)
        {
            var credential = await _context.Credentials
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (credential == null)
            {
                throw new ArgumentException($"Credential with id {id} not found.");
            }
            return credential;
        }

        public async Task<IEnumerable<Credential>> GetAllAsync()
        {
            return await _context.Credentials
                .Include(c => c.Author)
                .ToListAsync();
        }

        public async Task AddAsync(Credential entity)
        {
            await _context.Credentials.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Credentials.FindAsync(id);
            if (entity != null)
            {
                _context.Credentials.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Credential entity)
        {
            var existingEntity = await _context.Credentials.FindAsync(entity.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Credential>> GetByTypeAsync(CredentialType credentialType)
        {
            return await _context.Credentials
                .Where(c => c.CredentialType == credentialType)
                .Include(c => c.Author)
                .ToListAsync();
        }

        public async Task<IEnumerable<Credential>> GetStoredLocallyAsync()
        {
            return await _context.Credentials
                .Where(c => c.StoredLocally)
                .Include(c => c.Author)
                .ToListAsync();
        }
        public async Task<IEnumerable<Credential>> GetByUserIdAsync(int userId)
        {
            return await _context.Credentials
                .Where(c => c.AuthorId == userId)
                .Include(c => c.Author)
                .ToListAsync();
        }
    }
}