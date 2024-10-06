using Microsoft.EntityFrameworkCore;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;


namespace Monopost.DAL.Repositories.Implementations
{
    public class PostMediaRepository : IPostMediaRepository
    {
        private readonly AppDbContext _context;

        public PostMediaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PostMedia> GetByIdAsync(int id)
        {
            return await _context.PostMedia.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PostMedia>> GetAllAsync()
        {
            return await _context.PostMedia.ToListAsync();
        }

        public async Task AddAsync(PostMedia postSocialMedia)
        {
            await _context.PostMedia.AddAsync(postSocialMedia);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PostMedia postSocialMedia)
        {
            _context.PostMedia.Update(postSocialMedia);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var postSocialMedia = await GetByIdAsync(id);
            if (postSocialMedia != null)
            {
                _context.PostMedia.Remove(postSocialMedia);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PostMedia>> GetPostMediaByPostIdAsync(int postId)
        {
            return await _context.PostMedia
                .Where(psm => psm.PostId == postId)
                .ToListAsync();
        }
    }
}
