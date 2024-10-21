using Monopost.BLL.Models;
using Monopost.BLL.SocialMediaManagement.Models;

namespace Monopost.BLL.SocialMediaManagement.Posting
{
    public interface ISocialMediaPoster
    {
        Task<Result<PostPageAndId>> CreatePostAsync(string text, List<string> filePaths);
        Task<Result<EngagementStats>> GetEngagementStatsAsync(string postId);
        Task<Result<string>> GeneratePostLinkByChannelIdAsync(string postId);
    }
}