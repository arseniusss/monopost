using Monopost.DAL.Entities;

namespace Monopost.DAL.Repositories.Interfaces
{
    public interface IPostMediaRepository : IRepository<PostMedia>
    {
        Task<IEnumerable<PostMedia>> GetPostMediaByPostIdAsync(int postId);
    }
}

